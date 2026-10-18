using UnityEngine;

/*
 * Tints a die through a MaterialPropertyBlock so every die keeps sharing dice.mat; a material is never instantiated
 * at runtime. The palette is a Fixed-mode gradient, so each sample lands on one of its key colors instead of a muddy
 * blend between two of them.
 */
[RequireComponent(typeof(Renderer))]
public class DiceColor : MonoBehaviour
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] Gradient palette;
    [SerializeField] bool randomizeOnAwake = true;

    public Color Current { get; private set; } = Color.white;

    Renderer rend;
    MaterialPropertyBlock block;

    void Reset()
    {
        palette = DefaultPalette();
    }

    void Awake()
    {
        EnsureInit();
        if (randomizeOnAwake) Randomize();
    }

    void EnsureInit()
    {
        if (rend != null) return;
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        if (palette == null || palette.colorKeys.Length < 2) palette = DefaultPalette();
    }

    public void Randomize()
    {
        EnsureInit();
        SetColor(palette.Evaluate(Random.value));
    }

    public void SetColor(Color color)
    {
        EnsureInit();
        Current = color;
        rend.GetPropertyBlock(block);
        block.SetColor(BaseColorId, color);
        rend.SetPropertyBlock(block);
    }

    static Gradient DefaultPalette()
    {
        var colors = new[]
        {
            new Color(0.93f, 0.26f, 0.26f), // red
            new Color(1.00f, 0.58f, 0.18f), // orange
            new Color(1.00f, 0.85f, 0.25f), // yellow
            new Color(0.35f, 0.80f, 0.38f), // green
            new Color(0.22f, 0.76f, 0.80f), // teal
            new Color(0.30f, 0.52f, 0.96f), // blue
            new Color(0.62f, 0.40f, 0.92f), // purple
            new Color(0.97f, 0.97f, 0.97f), // white
        };

        var keys = new GradientColorKey[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            // In Fixed mode a key owns the range that ends at its time, so key i covers ((i)/n, (i+1)/n].
            keys[i] = new GradientColorKey(colors[i], (i + 1f) / colors.Length);
        }

        var gradient = new Gradient { mode = GradientMode.Fixed };
        gradient.SetKeys(keys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return gradient;
    }
}
