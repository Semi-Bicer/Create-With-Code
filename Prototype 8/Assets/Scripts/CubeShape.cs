using System.Collections;
using UnityEngine;

// INHERITANCE
public class CubeShape : Shape
{
    public float spinSpeed = 360f;
    private Quaternion m_InitialRotation;
    private bool m_IsAnimating = false;

    protected override void Start()
    {
        ShapeName = "Cube";
        ShapeColor = new Color(0.20f, 0.60f, 0.95f, 1.0f);
        m_InitialRotation = transform.rotation;
        base.Start();
    }

    // POLYMORPHISM
    public override string GetDescription()
    {
        return $"{base.GetDescription()} | I spin at {spinSpeed:0} deg/s!";
    }

    // POLYMORPHISM
    protected override void AnimateShape()
    {
        if (!m_IsAnimating)
        {
            StartCoroutine(SpinRoutine());
        }
    }

    // ABSTRACTION
    private IEnumerator SpinRoutine()
    {
        m_IsAnimating = true;
        float rotated = 0f;

        while (rotated < 360f)
        {
            float step = spinSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, step, Space.World);
            rotated += step;
            yield return null;
        }

        transform.rotation = m_InitialRotation;
        m_IsAnimating = false;
    }
}

