using System;
using UnityEngine;

/*
 * Decides when a die has come to rest. The die must stay below its own linear and angular velocity thresholds for a
 * quiet duration before it latches as settled; any motion above the thresholds clears the latch again.
 * Rigidbody.IsSleeping() is deliberately not used: its threshold is global and it fires while dice still creep.
 */
[RequireComponent(typeof(Rigidbody))]
public class DiceSettle : MonoBehaviour
{
    [SerializeField] float linearThreshold = 0.05f;
    [SerializeField] float angularThreshold = 0.1f;
    [SerializeField] float quietDuration = 0.4f;

    public bool IsSettled { get; private set; }
    public Rigidbody Body => rb;

    public event Action<DiceSettle> Settled;

    Rigidbody rb;
    float quietTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return;

        bool quiet = rb.linearVelocity.sqrMagnitude < linearThreshold * linearThreshold
                  && rb.angularVelocity.sqrMagnitude < angularThreshold * angularThreshold;

        if (!quiet)
        {
            quietTimer = 0f;
            IsSettled = false;
            return;
        }

        if (IsSettled) return;

        quietTimer += Time.fixedDeltaTime;
        if (quietTimer >= quietDuration)
        {
            IsSettled = true;
            Settled?.Invoke(this);
        }
    }

    // Called whenever the die is thrown or nudged so a stale latch from the previous roll is never reported.
    public void ResetSettle()
    {
        quietTimer = 0f;
        IsSettled = false;
    }
}
