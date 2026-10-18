using UnityEngine;

/*
 * One handle per die so the spawner and the roll manager never have to fetch the reader, settle detector and body
 * separately. It owns the two ways a die gets set in motion: a launch from the spawner and a nudge from the manager.
 */
[RequireComponent(typeof(Rigidbody), typeof(DiceReader), typeof(DiceSettle))]
public class Die : MonoBehaviour
{
    public Rigidbody Body { get; private set; }
    public DiceReader Reader { get; private set; }
    public DiceSettle Settle { get; private set; }
    public DiceColor Tint { get; private set; }

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
        Reader = GetComponent<DiceReader>();
        Settle = GetComponent<DiceSettle>();
        Tint = GetComponent<DiceColor>();
    }

    public void Launch(Vector3 velocity, Vector3 angularVelocity)
    {
        Body.linearVelocity = velocity;
        Body.angularVelocity = angularVelocity;
        Settle.ResetSettle();
    }

    // Pops a cocked or stuck die back into the air with a fresh spin.
    public void Nudge(float upSpeed, float spin)
    {
        Vector3 direction = (Vector3.up + Random.insideUnitSphere * 0.35f).normalized;
        Body.AddForce(direction * upSpeed, ForceMode.VelocityChange);
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
