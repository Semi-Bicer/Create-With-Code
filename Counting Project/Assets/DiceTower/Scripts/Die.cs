using UnityEngine;

/*
 * One handle per die so the spawner and the roll manager never have to fetch the reader, settle detector and body
 * separately. It owns the ways a die gets set in motion: held in the hand, launched by the spawner, nudged by the
 * manager.
 */
[RequireComponent(typeof(Rigidbody), typeof(DiceReader), typeof(DiceSettle))]
public class Die : MonoBehaviour
{
    public Rigidbody Body { get; private set; }
    public DiceReader Reader { get; private set; }
    public DiceSettle Settle { get; private set; }
    public DiceColor Tint { get; private set; }
    public bool IsHeld { get; private set; }

    Collider shape;
    RigidbodyInterpolation interpolation;

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
        Reader = GetComponent<DiceReader>();
        Settle = GetComponent<DiceSettle>();
        Tint = GetComponent<DiceColor>();
        shape = GetComponent<Collider>();
        interpolation = Body.interpolation;
    }

    // Parks the die in the hand: no physics and no collisions, so it can be turned in place by its transform.
    public void Hold()
    {
        IsHeld = true;
        Body.isKinematic = true;
        Body.interpolation = RigidbodyInterpolation.None;
        shape.enabled = false;
    }

    public void Release()
    {
        IsHeld = false;
        shape.enabled = true;
        Body.isKinematic = false;
        Body.interpolation = interpolation;
    }

    public void Launch(Vector3 velocity, Vector3 angularVelocity)
    {
        Body.linearVelocity = velocity;
        Body.angularVelocity = angularVelocity;
        Settle.ResetSettle();
    }

    // Kicks a cocked or stuck die with the given velocity change and a fresh spin.
    public void Nudge(Vector3 velocityChange, float spin)
    {
        Body.AddForce(velocityChange, ForceMode.VelocityChange);
        Body.AddTorque(Random.insideUnitSphere * spin, ForceMode.VelocityChange);
        Settle.ResetSettle();
    }

    // Puts a die that escaped the tower back above the tray instead of losing it.
    public void Teleport(Vector3 position)
    {
        Body.position = position;
        Body.rotation = Random.rotation;
        Body.linearVelocity = Vector3.zero;
        Body.angularVelocity = Random.insideUnitSphere * 5f;
        transform.SetPositionAndRotation(Body.position, Body.rotation);
        Settle.ResetSettle();
    }

    public bool TryRead(out int value) => Reader.TryReadValue(out value);
}
