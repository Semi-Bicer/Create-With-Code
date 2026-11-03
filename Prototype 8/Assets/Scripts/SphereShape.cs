using System.Collections;
using UnityEngine;

// INHERITANCE
public class SphereShape : Shape
{
    public float bounceHeight = 0.8f;
    private Vector3 m_InitialPosition;
    private bool m_IsAnimating = false;

    protected override void Start()
    {
        ShapeName = "Sphere";
        ShapeColor = new Color(0.2f, 0.85f, 0.35f, 1.0f);
        m_InitialPosition = transform.position;
        base.Start();
    }

    // POLYMORPHISM
    public override string GetDescription()
    {
        return $"[Sphere] Bounces smoothly! Clicked {ClickCount} time(s).";
    }

    // POLYMORPHISM
    protected override void AnimateShape()
    {
        if (!m_IsAnimating)
        {
            StartCoroutine(BounceRoutine());
        }
    }

    // ABSTRACTION
    private IEnumerator BounceRoutine()
    {
        m_IsAnimating = true;
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float yOffset = Mathf.Sin((elapsed / duration) * Mathf.PI) * bounceHeight;
            transform.position = m_InitialPosition + Vector3.up * yOffset;
            yield return null;
        }

        transform.position = m_InitialPosition;
        m_IsAnimating = false;
    }
}

