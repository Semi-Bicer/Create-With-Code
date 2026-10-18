using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * Holds the dice for the next roll and throws them.
 * Before a throw the dice wait in the hand in front of the camera, each turning about its own random axis, so the
 * player sees what they are about to roll. A throw releases them one after another rather than all at once: five dice
 * reaching a baffle edge together wedge into an arch across the gap, a stream of dice does not.
 * With an aim target (the tower mouth) each die gets the ballistic velocity that carries it to a point above the
 * target in the flight time. The swipe shifts that point sideways and a faster swipe shortens the flight, so every
 * throw is different but none misses the tower. Without a target the die is simply thrown along the swipe.
 * Held dice sit in rows on the hand anchor. Without one they wait on a ring around the spawn anchor, each a golden
 * angle (~137.5 deg) further round than the previous one so consecutive dice never overlap.
 */
public class DiceSpawner : MonoBehaviour
{
    [Serializable]
    public struct Entry
    {
        public Die prefab;
        [Min(0)] public int count;
    }

    [Header("Dice")]
    [SerializeField] Entry[] diceSet = new Entry[0];

    [Header("Hand")]
    [SerializeField] Transform handAnchor;
    [SerializeField, Min(1)] int handColumns = 5;
    [SerializeField] float handSpacing = 2f;
    [SerializeField, Tooltip("Degrees per second, picked per die between x and y.")] Vector2 handSpin = new Vector2(60f, 180f);

    [Header("Without a hand")]
    [SerializeField] Transform spawnAnchor;
    [SerializeField] float ringRadius = 1.6f;
    [SerializeField] Vector3 spawnJitter = new Vector3(0.2f, 0.4f, 0.2f);

    [Header("Throw")]
    [SerializeField, Min(0f)] float releaseInterval = 0.2f;
    [SerializeField] float spinSpeed = 18f;
    [SerializeField] Transform aimTarget;
    [SerializeField, Tooltip("How far above the aim target the dice are thrown to; enough to clear the funnel lip.")] float aimHeight = 3.5f;
    [SerializeField, Tooltip("Sideways shift of the landing point for a fully sideways swipe.")] float swipeAim = 1.8f;
    [SerializeField] float aimJitter = 0.7f;
    [SerializeField, Tooltip("Flight time for a full-strength swipe (x) and a weak one (y).")] Vector2 flightTime = new Vector2(0.75f, 1.05f);
    [SerializeField] float minThrowSpeed = 4f;
    [SerializeField] float maxThrowSpeed = 11f;

    const float GoldenAngle = 2.39996323f;

    readonly List<Die> held = new List<Die>();
    readonly List<Vector3> heldSpin = new List<Vector3>();
    readonly List<Die> dice = new List<Die>();
    Coroutine release;

    // Dice of the roll in progress (or the last one); the hand is separate.
    public IReadOnlyList<Die> Dice => dice;
    public IReadOnlyList<Die> Held => held;
    public bool IsReleasing => release != null;
    public int EntryCount => diceSet.Length;

    public Die GetPrefab(int index) => diceSet[index].prefab;
    public int GetCount(int index) => diceSet[index].count;
    public void SetCount(int index, int count) => diceSet[index].count = Mathf.Max(0, count);

    public int DiceCount
    {
        get
        {
            int total = 0;
            foreach (Entry entry in diceSet)
            {
                if (entry.prefab != null) total += entry.count;
            }
            return total;
        }
    }

    // Fills the hand with a fresh set for the next roll; the dice of the last roll stay where they are.
    public void PrepareHand()
    {
        ClearHand();
        List<Die> order = ShuffledSet();
        for (int i = 0; i < order.Count; i++)
        {
            Die die = Instantiate(order[i], HoldPosition(i, order.Count), Random.rotation, transform);
            die.name = $"{order[i].name}_{i}";
            die.Hold();
            held.Add(die);
            heldSpin.Add(Random.onUnitSphere * Random.Range(handSpin.x, handSpin.y));
        }
    }

    public void Throw(Vector3 swipeDirection, float strength)
    {
        if (held.Count == 0) PrepareHand();
        ClearRolled();
        release = StartCoroutine(Release(swipeDirection, Mathf.Clamp01(strength)));
    }

    public void Clear()
    {
        ClearRolled();
        ClearHand();
    }

    void Update()
    {
        for (int i = 0; i < held.Count; i++)
        {
            if (held[i] == null) continue;
            Vector3 spin = heldSpin[i];
            held[i].transform.rotation = Quaternion.AngleAxis(spin.magnitude * Time.deltaTime, spin.normalized) * held[i].transform.rotation;
        }
    }

    IEnumerator Release(Vector3 swipeDirection, float strength)
    {
        var wait = new WaitForSeconds(releaseInterval);
        bool first = true;

        while (held.Count > 0)
        {
            if (!first && releaseInterval > 0f) yield return wait;
            first = false;

            Die die = held[0];
            held.RemoveAt(0);
            heldSpin.RemoveAt(0);
            if (die == null) continue;

            die.Release();
            dice.Add(die);
            die.Launch(ThrowVelocity(die.transform.position, swipeDirection, strength), Random.insideUnitSphere * spinSpeed);
        }

        release = null;
    }

    Vector3 ThrowVelocity(Vector3 from, Vector3 swipeDirection, float strength)
    {
        if (aimTarget == null)
        {
            Vector3 direction = swipeDirection.sqrMagnitude > 0.0001f ? swipeDirection.normalized : Vector3.forward;
            return direction * Mathf.Lerp(minThrowSpeed, maxThrowSpeed, strength);
        }

        Vector3 toTarget = aimTarget.position - from;
        toTarget.y = 0f;
        Vector3 side = Vector3.Cross(Vector3.up, toTarget.normalized);
        float sideways = Vector3.Dot(swipeDirection, side);
        Vector2 jitter = Random.insideUnitCircle * aimJitter;

        Vector3 landing = aimTarget.position + Vector3.up * aimHeight + side * (sideways * swipeAim) + new Vector3(jitter.x, 0f, jitter.y);
        float time = Mathf.Lerp(flightTime.y, flightTime.x, strength);
        return (landing - from) / time - 0.5f * time * Physics.gravity;
    }

    Vector3 HoldPosition(int index, int count)
    {
        if (handAnchor != null)
        {
            int row = index / handColumns;
            int inRow = Mathf.Min(handColumns, count - row * handColumns);
            int column = index % handColumns;
            float x = (column - (inRow - 1) * 0.5f) * handSpacing;
            return handAnchor.TransformPoint(new Vector3(x, row * handSpacing, 0f));
        }

        Transform anchor = spawnAnchor != null ? spawnAnchor : transform;
        float angle = index * GoldenAngle + Random.value * 0.3f;
        Vector3 ring = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (count > 1 ? ringRadius : 0f);
        Vector3 jitter = Vector3.Scale(Random.insideUnitSphere, spawnJitter);
        return anchor.TransformPoint(ring + jitter);
    }

    void ClearHand()
    {
        foreach (Die die in held)
        {
            if (die != null) Destroy(die.gameObject);
        }
        held.Clear();
        heldSpin.Clear();
    }

    void ClearRolled()
    {
        if (release != null)
        {
            StopCoroutine(release);
            release = null;
        }

        foreach (Die die in dice)
        {
            if (die != null) Destroy(die.gameObject);
        }
        dice.Clear();
    }

    // Mixed sets leave the hand in random order rather than grouped by type.
    List<Die> ShuffledSet()
    {
        var order = new List<Die>();
        foreach (Entry entry in diceSet)
        {
            if (entry.prefab == null) continue;
            for (int i = 0; i < entry.count; i++) order.Add(entry.prefab);
        }

        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        return order;
    }
}
