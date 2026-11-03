using System.Collections;
using UnityEngine;

// INHERITANCE
public class CubeShape : Shape
{
    public float spinSpeed = 720f;
    private bool m_IsAnimating = false;

    protected override void Start()
    {
        ShapeName = "Cube";
        ShapeColor = new Color(0.2f, 0.55f, 0.95f, 1.0f);
        base.Start();
    }

    // POLYMORPHISM
    public override string GetDescription()
    {
        return $"[Cube] Spins 360 degrees! Clicked {ClickCount} time(s).";
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

        transform.rotation = Quaternion.identity;
        m_IsAnimating = false;
    }
}

