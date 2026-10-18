using UnityEngine;

/*
 * Keeps a die tumbling on its way down the tower. On slippery baffles a die slides down like a block and barely
 * turns, and grippier baffles make dice stick and jam. So friction stays low and the tumbling is added here instead:
 * every hit against tower geometry adds a random spin scaled by the impact speed, and a die that slides along a
 * surface without turning is rolled over in the direction it slides. Contacts below minHeight (the tray) are left
 * alone so dice come to rest normally.
 */
[RequireComponent(typeof(Rigidbody))]
public class TumbleOnImpact : MonoBehaviour
{
    [SerializeField] LayerMask surfaces;
    [SerializeField] float minHeight = 2.5f;

    [Header("Impacts")]
    [SerializeField] float minImpactSpeed = 2f;
    [SerializeField] float spinPerImpactSpeed = 1.2f;
    [SerializeField] float maxImpactSpin = 20f;

    [Header("Sliding")]
    [SerializeField] float slideSpeed = 2f;
    [SerializeField] float slideSpinBelow = 4f;
    [SerializeField] float rollAcceleration = 60f;

    Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!Counts(collision)) return;

        float speed = collision.relativeVelocity.magnitude;
        if (speed < minImpactSpeed) return;

        float spin = Mathf.Min(maxImpactSpin, speed * spinPerImpactSpeed);
        body.AddTorque(Random.onUnitSphere * spin, ForceMode.VelocityChange);
    }

    void OnCollisionStay(Collision collision)
    {
        if (!Counts(collision)) return;

        Vector3 velocity = body.linearVelocity;
        if (velocity.magnitude < slideSpeed || body.angularVelocity.magnitude > slideSpinBelow) return;

        Vector3 normal = collision.GetContact(0).normal;
        Vector3 rollAxis = Vector3.Cross(normal, velocity);
        if (rollAxis.sqrMagnitude < 0.0001f) return;
        body.AddTorque(rollAxis.normalized * rollAcceleration, ForceMode.Acceleration);
    }

    bool Counts(Collision collision)
    {
        return (surfaces.value & (1 << collision.gameObject.layer)) != 0 && transform.position.y > minHeight;
    }
}
