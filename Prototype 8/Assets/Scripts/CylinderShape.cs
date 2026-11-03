using System.Collections;
using UnityEngine;

// INHERITANCE
public class CylinderShape : Shape
{
    public float pulseMultiplier = 1.4f;
    private Vector3 m_InitialScale;
    private bool m_IsAnimating = false;

    protected override void Start()
    {
        ShapeName = "Cylinder";
        ShapeColor = new Color(0.25f, 0.85f, 0.40f, 1.0f);
        m_InitialScale = transform.localScale;
        base.Start();
    }

    // POLYMORPHISM
    public override string GetDescription()
    {
        return $"{base.GetDescription()} | I pulse to {pulseMultiplier:0.0}x scale!";
    }

    // POLYMORPHISM
    protected override void AnimateShape()
    {
        if (!m_IsAnimating)
        {
            StartCoroutine(PulseRoutine());
        }
    }

    // ABSTRACTION
    private IEnumerator PulseRoutine()
    {
        m_IsAnimating = true;
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float factor = 1.0f + Mathf.Sin((elapsed / duration) * Mathf.PI) * (pulseMultiplier - 1.0f);
            transform.localScale = m_InitialScale * factor;
            yield return null;
        }

        transform.localScale = m_InitialScale;
        m_IsAnimating = false;
    }
}

