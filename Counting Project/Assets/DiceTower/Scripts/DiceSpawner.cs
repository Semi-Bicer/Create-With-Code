using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Spawns the dice for one roll and throws them. Dice start on a ring around the spawn anchor so they never overlap,
 * each with a random rotation. The throw direction comes from the swipe; when an aim target is set (the tower mouth)
 * the swipe direction is bent toward it so a sloppy swipe still lands the dice in the tower.
 * Dice are released one after another rather than all at once: five dice reaching a baffle edge together wedge into
 * an arch across the gap, a stream of dice does not.
 */
public class DiceSpawner : MonoBehaviour
{
    [Header("Dice")]
    [SerializeField] Die dicePrefab;
    [SerializeField, Min(1)] int diceCount = 5;

    [Header("Spawn")]
    [SerializeField] Transform spawnAnchor;
    [SerializeField] float ringRadius = 1.6f;
    [SerializeField] Vector3 spawnJitter = new Vector3(0.2f, 0.4f, 0.2f);
    [SerializeField, Min(0f)] float releaseInterval = 0.2f;

    [Header("Throw")]
    [SerializeField] float minThrowSpeed = 4f;
    [SerializeField] float maxThrowSpeed = 11f;
    [SerializeField, Range(0f, 0.5f)] float throwSpread = 0.12f;
    [SerializeField] float spinSpeed = 18f;
    [SerializeField] Transform aimTarget;
    [SerializeField, Range(0f, 1f)] float aimAssist = 0.75f;

    readonly List<Die> dice = new List<Die>();
    Coroutine release;

    public IReadOnlyList<Die> Dice => dice;
    public bool IsReleasing => release != null;
    public int DiceCount => diceCount;

    public void Throw(Vector3 swipeDirection, float strength)
    {
        Clear();
        release = StartCoroutine(Release(swipeDirection, strength));
    }

    IEnumerator Release(Vector3 swipeDirection, float strength)
    {
        Transform anchor = spawnAnchor != null ? spawnAnchor : transform;
        Vector3 direction = ThrowDirection(anchor.position, swipeDirection);
        float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, Mathf.Clamp01(strength));
        var wait = new WaitForSeconds(releaseInterval);

        for (int i = 0; i < diceCount; i++)
        {
            if (i > 0 && releaseInterval > 0f) yield return wait;

            float angle = (i + Random.value * 0.3f) * Mathf.PI * 2f / diceCount;
            Vector3 ring = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (diceCount > 1 ? ringRadius : 0f);
            Vector3 jitter = Vector3.Scale(Random.insideUnitSphere, spawnJitter);
            Vector3 position = anchor.TransformPoint(ring + jitter);

            Die die = Instantiate(dicePrefab, position, Random.rotation, transform);
            die.name = $"{dicePrefab.name}_{i}";
            dice.Add(die);

            Vector3 dieDirection = (direction + Random.insideUnitSphere * throwSpread).normalized;
            die.Launch(dieDirection * speed, Random.insideUnitSphere * spinSpeed);
        }

        release = null;
    }

    public void Clear()
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

    Vector3 ThrowDirection(Vector3 from, Vector3 swipeDirection)
    {
        Vector3 swipe = swipeDirection.sqrMagnitude > 0.0001f ? swipeDirection.normalized : Vector3.forward;
        if (aimTarget == null) return swipe;

        Vector3 toTarget = (aimTarget.position - from).normalized;
        return Vector3.Slerp(swipe, toTarget, aimAssist).normalized;
    }
}
