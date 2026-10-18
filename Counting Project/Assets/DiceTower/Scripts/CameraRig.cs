using System.Collections;
using UnityEngine;

/*
 * Moves the one game camera between two anchors: the tower view while throwing and the tray view once dice reach the
 * tray. Position is lerped and rotation slerped along an easing curve. The tray view should be close to top-down so
 * what the player reads off the dice matches the world-up face the game reads.
 */
public class CameraRig : MonoBehaviour
{
    [SerializeField] RollManager manager;
    [SerializeField] Transform towerView;
    [SerializeField] Transform trayView;
    [SerializeField] float duration = 0.9f;
    [SerializeField] AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Coroutine move;
    Transform current;

    void OnEnable()
    {
        manager.StateChanged += OnStateChanged;
        manager.DiceReachedTray += OnDiceReachedTray;
    }

    void OnDisable()
    {
        manager.StateChanged -= OnStateChanged;
        manager.DiceReachedTray -= OnDiceReachedTray;
    }

    void Start()
    {
        current = towerView;
        transform.SetPositionAndRotation(towerView.position, towerView.rotation);
    }

    void OnStateChanged(RollState state)
    {
        if (state == RollState.Throwing || state == RollState.Idle) MoveTo(towerView);
        else if (state == RollState.Result) MoveTo(trayView);
    }

    void OnDiceReachedTray()
    {
        MoveTo(trayView);
    }

    void MoveTo(Transform target)
    {
        if (target == current) return;
        current = target;
        if (move != null) StopCoroutine(move);
        move = StartCoroutine(Move(target));
    }

    IEnumerator Move(Transform target)
    {
        Vector3 fromPosition = transform.position;
        Quaternion fromRotation = transform.rotation;

        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            float k = ease.Evaluate(t);
            transform.SetPositionAndRotation(
                Vector3.LerpUnclamped(fromPosition, target.position, k),
                Quaternion.SlerpUnclamped(fromRotation, target.rotation, k));
            yield return null;
        }

        transform.SetPositionAndRotation(target.position, target.rotation);
        move = null;
    }
}
