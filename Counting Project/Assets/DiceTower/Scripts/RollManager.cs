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
 * reported; after too many nudges the die is dropped into the tray instead. Dice that leave the play area are
 * dropped into the tray as well.
 */
public class RollManager : MonoBehaviour
{
    [SerializeField] DiceSpawner spawner;
    [SerializeField] DiceTowerBuilder tower;

    [Header("Timing")]
    [SerializeField] float throwingDuration = 0.4f;
    [SerializeField] float rollingTimeout = 8f;

    [Header("Recovery")]
    [SerializeField] int maxNudgesPerDie = 3;
    [SerializeField] float nudgeUpSpeed = 4f;
    [SerializeField] float nudgeSpin = 10f;
    [SerializeField] float killHeight = -10f;
    [SerializeField] float maxDistanceFromTower = 60f;

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
        if (!CanThrow) return;

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
                result.Dice.Add(new DieResult { value = value, color = die.Tint != null ? die.Tint.Current : Color.white });
                continue;
            }

            clean = false;
            Recover(die);
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

    void Recover(Die die)
    {
        nudges.TryGetValue(die, out int count);
        nudges[die] = count + 1;

        if (count >= maxNudgesPerDie) DropIntoTray(die);
        else die.Nudge(nudgeUpSpeed, nudgeSpin);
    }

    void NudgeUnsettledDice()
    {
        foreach (Die die in spawner.Dice)
        {
            if (!die.Settle.IsSettled) Recover(die);
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
        Vector3 drop = tower != null && tower.TrayDropPoint != null ? tower.TrayDropPoint.position : Vector3.up * 3f;
        Vector2 offset = UnityEngine.Random.insideUnitCircle * 2f;
        die.Teleport(drop + new Vector3(offset.x, 0f, offset.y));
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
