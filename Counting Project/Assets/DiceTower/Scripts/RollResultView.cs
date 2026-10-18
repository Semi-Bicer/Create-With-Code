using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Shows the state hint and, once a roll completes, the total plus one chip per die in that die's color, with the value
 * and the die type (d4 ... d20), grouped by type. Chips are cloned from an inactive template under the chip row, so the
 * layout is edited in the scene rather than in code; the panel widens to fit however many dice were rolled.
 */
public class RollResultView : MonoBehaviour
{
    [SerializeField] RollManager manager;

    [Header("Hint")]
    [SerializeField] TMP_Text hintText;
    [SerializeField] string idleHint = "Swipe up to roll";
    [SerializeField] string rollingHint = "Rolling...";
    [SerializeField] string resultHint = "Swipe to roll again";

    [Header("Result")]
    [SerializeField] CanvasGroup resultPanel;
    [SerializeField] TMP_Text totalText;
    [SerializeField] RectTransform chipRow;
    [SerializeField] Image chipTemplate;
    [SerializeField] float fadeDuration = 0.25f;
    [SerializeField] float minPanelWidth = 760f;
    [SerializeField] float panelPadding = 80f;

    readonly List<Image> chips = new List<Image>();
    float targetAlpha;

    void OnEnable()
    {
        manager.StateChanged += OnStateChanged;
        manager.RollCompleted += OnRollCompleted;
    }

    void OnDisable()
    {
        manager.StateChanged -= OnStateChanged;
        manager.RollCompleted -= OnRollCompleted;
    }

    void Start()
    {
        chipTemplate.gameObject.SetActive(false);
        resultPanel.alpha = 0f;
        OnStateChanged(manager.State);
    }

    void Update()
    {
        float step = fadeDuration > 0f ? Time.deltaTime / fadeDuration : 1f;
        resultPanel.alpha = Mathf.MoveTowards(resultPanel.alpha, targetAlpha, step);
    }

    void OnStateChanged(RollState state)
    {
        switch (state)
        {
            case RollState.Idle:
                hintText.text = idleHint;
                targetAlpha = 0f;
                break;
            case RollState.Throwing:
            case RollState.Rolling:
            case RollState.Settling:
                hintText.text = rollingHint;
                targetAlpha = 0f;
                break;
            case RollState.Result:
                hintText.text = resultHint;
                targetAlpha = 1f;
                break;
        }
    }

    void OnRollCompleted(RollResult result)
    {
        totalText.text = result.Total.ToString();

        while (chips.Count < result.Dice.Count)
        {
            chips.Add(Instantiate(chipTemplate, chipRow));
        }

        List<DieResult> dice = result.Dice.OrderBy(d => d.sides).ToList();
        for (int i = 0; i < chips.Count; i++)
        {
            bool used = i < dice.Count;
            chips[i].gameObject.SetActive(used);
            if (!used) continue;

            DieResult die = dice[i];
            Color ink = Luminance(die.color) > 0.55f ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
            chips[i].color = die.color;
            SetLabel(chips[i], "Value", die.value.ToString(), ink);
            SetLabel(chips[i], "Type", die.sides > 0 ? "d" + die.sides : "", ink);
        }

        FitPanel(dice.Count);
    }

    void FitPanel(int chipCount)
    {
        var layout = chipRow.GetComponent<HorizontalLayoutGroup>();
        float spacing = layout != null ? layout.spacing : 0f;
        float width = chipCount * chipTemplate.rectTransform.sizeDelta.x + Mathf.Max(0, chipCount - 1) * spacing + panelPadding;
        var panel = (RectTransform)resultPanel.transform;
        panel.sizeDelta = new Vector2(Mathf.Max(minPanelWidth, width), panel.sizeDelta.y);
    }

    static void SetLabel(Image chip, string childName, string text, Color color)
    {
        Transform child = chip.transform.Find(childName);
        if (child == null) return;
        TMP_Text label = child.GetComponent<TMP_Text>();
        label.text = text;
        label.color = color;
    }

    static float Luminance(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
}
