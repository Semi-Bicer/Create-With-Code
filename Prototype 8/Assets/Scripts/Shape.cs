using UnityEngine;

// INHERITANCE
public abstract class Shape : MonoBehaviour
{
    [SerializeField] private string m_ShapeName = "Shape";
    [SerializeField] private Color m_ShapeColor = Color.white;
    private int m_ClickCount = 0;

    // ENCAPSULATION
    public string ShapeName
    {
        get { return m_ShapeName; }
        protected set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                m_ShapeName = value;
            }
        }
    }

    // ENCAPSULATION
    public Color ShapeColor
    {
        get { return m_ShapeColor; }
        protected set { m_ShapeColor = value; }
    }

    // ENCAPSULATION
    public int ClickCount
    {
        get { return m_ClickCount; }
        protected set
        {
            if (value < 0)
            {
                Debug.LogError("ClickCount cannot be negative!");
            }
            else
            {
                m_ClickCount = value;
            }
        }
    }

    protected virtual void Start()
    {
        ApplyMaterialColor();
    }

    protected virtual void OnValidate()
    {
        ApplyMaterialColor();
    }

    // ABSTRACTION
    protected void ApplyMaterialColor()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", m_ShapeColor);
            block.SetColor("_Color", m_ShapeColor);
            renderer.SetPropertyBlock(block);
        }
    }

    // ABSTRACTION
    public string HandleSelection()
    {
        ClickCount++;
        AnimateShape();
        return GetDescription();
    }

    // POLYMORPHISM
    public virtual string GetDescription()
    {
        return $"{ShapeName} clicked {ClickCount} time(s).";
    }

    // POLYMORPHISM
    protected abstract void AnimateShape();
}

