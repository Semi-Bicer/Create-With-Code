using System;
using System.Collections.Generic;
using UnityEngine;

public enum RollState
{
    Idle,
    Throwing,
    Rolling,
    Settling,
    Result
}

[Serializable]
public struct DieResult
{
    public int value;
    public int sides;
    public Color color;
}

public class RollResult
{
    public int Total;
    public readonly List<DieResult> Dice = new List<DieResult>();
}

/*
 * Runs one roll from swipe to result: Idle -> Throwing -> Rolling -> Settling -> Result.
 * Rolling waits until every die has settled. Settling then checks each die: it must lie in the tray and show a clean
 * face. A die that is cocked or stuck on a baffle is nudged and the roll returns to Rolling, so a wrong sum is never
 * reported. Cocked dice mostly lie on the end of the exit ramp or lean on the pile by the exit, so a nudge pushes them
 * toward the open middle of the tray. A die that is still bad after its nudges is re-dropped from above the emptiest
 * spot in the tray (the table-top rule of re-rolling a cocked die): with ten dice the pile by the exit keeps knocking
 * nudged dice back, while a die dropped onto open felt almost always lands flat. Dice that leave the play area are
 * re-dropped the same way.
 */
public class RollManager : MonoBehaviour
{
    [SerializeField] DiceSpawner spawner;
    [SerializeField] DiceTowerBuilder tower;

    [Header("Timing")]
    [SerializeField] float throwingDuration = 0.4f;
    [SerializeField] float rollingTimeout = 8f;

    [Header("Recovery")]
    [SerializeField] int maxNudgesPerDie = 1;
    [SerializeField, Tooltip("Gravity is -28, so 7 lifts a die about 0.9 units: enough to clear the pile by the exit.")]
    float nudgeUpSpeed = 7f;
    [SerializeField] float nudgeSideSpeed = 6f;
    [SerializeField] float nudgeSpin = 10f;
    [SerializeField] float killHeight = -10f;
    [SerializeField] float maxDistanceFromTower = 60f;
    [SerializeField] float trayMargin = 1.5f;

    public RollState State { get; private set; } = RollState.Idle;
    public RollResult LastResult { get; private set; }

    public event Action<RollState> StateChanged;
    public event Action<RollResult> RollCompleted;
    public event Action DiceReachedTray;

    readonly Dictionary<Die, int> nudges = new Dictionary<Die, int>();
    float stateTime;
    bool trayReached;

    public bool CanThrow => State == RollState.Idle || State == RollState.Result;

    // Hooked to SwipeThrowInput.onSwipe. A swipe on the result screen starts the next roll straight away.
    public void Throw(Vector3 direction, float strength)
    {
        if (!CanThrow || spawner.DiceCount == 0) return;

        nudges.Clear();
        trayReached = false;
        LastResult = null;
        spawner.Throw(direction, strength);
        SetState(RollState.Throwing);
    }

    public void ClearTable()
    {
        spawner.Clear();
        LastResult = null;
        SetState(RollState.Idle);
    }

    void Update()
    {
        stateTime += Time.deltaTime;

        switch (State)
        {
            case RollState.Throwing:
                if (!spawner.IsReleasing && stateTime >= throwingDuration) SetState(RollState.Rolling);
                break;

            case RollState.Rolling:
                RescueEscapedDice();
                CheckTrayReached();
                if (AllSettled())
                {
                    SetState(RollState.Settling);
                }
                else if (stateTime > rollingTimeout)
                {
                    NudgeUnsettledDice();
                    stateTime = 0f;
                }
                break;

            case RollState.Settling:
                Evaluate();
                break;
        }
    }

    void Evaluate()
    {
        var result = new RollResult();
        bool clean = true;

        foreach (Die die in spawner.Dice)
        {
            bool inTray = InTray(die);
            if (inTray && die.TryRead(out int value))
            {
                result.Total += value;
                result.Dice.Add(new DieResult
                {
                    value = value,
                    sides = die.Reader.faceTable != null ? die.Reader.faceTable.sides : 0,
                    color = die.Tint != null ? die.Tint.Current : Color.white
                });
                continue;
            }

            clean = false;
            Recover(die, inTray);
        }

        if (!clean)
        {
            SetState(RollState.Rolling);
            return;
        }

        LastResult = result;
        SetState(RollState.Result);
        RollCompleted?.Invoke(result);
    }

    // A die that never made it into the tray (usually wedged in the exit behind the pile) is re-dropped straight away;
    // a cocked die in the tray gets a nudge first.
    void Recover(Die die, bool inTray)
    {
        nudges.TryGetValue(die, out int count);
        nudges[die] = count + 1;

        if (!inTray || count >= maxNudgesPerDie) DropIntoTray(die);
        else die.Nudge(NudgeVelocity(die), nudgeSpin);
    }

    Vector3 NudgeVelocity(Die die)
    {
        Vector3 toOpen = Vector3.zero;
        if (tower != null && tower.TrayZone != null)
        {
            toOpen = tower.TrayZone.bounds.center - die.transform.position;
            toOpen.y = 0f;
        }

        Vector2 jitter = UnityEngine.Random.insideUnitCircle * 0.35f;
        Vector3 side = toOpen.sqrMagnitude > 0.01f ? toOpen.normalized : Vector3.zero;
        side = (side + new Vector3(jitter.x, 0f, jitter.y)).normalized;
        return side * nudgeSideSpeed + Vector3.up * nudgeUpSpeed;
    }

    void NudgeUnsettledDice()
    {
        foreach (Die die in spawner.Dice)
        {
            if (!die.Settle.IsSettled) Recover(die, InTray(die));
        }
    }

    void RescueEscapedDice()
    {
        Vector3 center = tower != null ? tower.transform.position : Vector3.zero;
        foreach (Die die in spawner.Dice)
        {
            Vector3 p = die.transform.position;
            if (p.y < killHeight || Vector3.Distance(p, center) > maxDistanceFromTower) DropIntoTray(die);
        }
    }

    void DropIntoTray(Die die)
    {
        die.Teleport(EmptiestTraySpot(die));
    }

    // Best of a few random points in the tray, scored by distance to the nearest other die.
    Vector3 EmptiestTraySpot(Die dropping)
    {
        if (tower == null || tower.TrayZone == null) return Vector3.up * 3f;

        Bounds tray = tower.TrayZone.bounds;
        float height = tower.TrayDropPoint != null ? tower.TrayDropPoint.position.y : tray.max.y + 1f;
        Vector3 best = new Vector3(tray.center.x, height, tray.center.z);
        float bestGap = -1f;

        for (int i = 0; i < 24; i++)
        {
            var candidate = new Vector3(
                UnityEngine.Random.Range(tray.min.x + trayMargin, tray.max.x - trayMargin),
                height,
                UnityEngine.Random.Range(tray.min.z + trayMargin, tray.max.z - trayMargin));

            float gap = float.MaxValue;
            foreach (Die other in spawner.Dice)
            {
                if (other == dropping) continue;
                Vector3 offset = other.transform.position - candidate;
                offset.y = 0f;
                gap = Mathf.Min(gap, offset.magnitude);
            }

            if (gap > bestGap)
            {
                bestGap = gap;
                best = candidate;
            }
        }
        return best;
    }

    void CheckTrayReached()
    {
        if (trayReached) return;
        foreach (Die die in spawner.Dice)
        {
            if (!InTray(die)) continue;
            trayReached = true;
            DiceReachedTray?.Invoke();
            return;
        }
    }

    bool AllSettled()
    {
        if (spawner.IsReleasing || spawner.Dice.Count == 0) return false;
        foreach (Die die in spawner.Dice)
        {
            if (!die.Settle.IsSettled) return false;
        }
        return true;
    }

    bool InTray(Die die)
    {
        if (tower == null || tower.TrayZone == null) return true;
        return tower.TrayZone.bounds.Contains(die.transform.position);
    }

    void SetState(RollState next)
    {
        State = next;
        stateTime = 0f;
        StateChanged?.Invoke(next);
    }
}
