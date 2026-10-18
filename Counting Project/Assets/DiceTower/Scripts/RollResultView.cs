using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Shows the state hint and, once a roll completes, the total plus one chip per die in that die's color. Chips are
 * cloned from an inactive template under the chip row, so the layout is edited in the scene rather than in code.
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

        for (int i = 0; i < chips.Count; i++)
        {
            bool used = i < result.Dice.Count;
            chips[i].gameObject.SetActive(used);
            if (!used) continue;

            DieResult die = result.Dice[i];
            chips[i].color = die.color;
            TMP_Text label = chips[i].GetComponentInChildren<TMP_Text>(true);
            label.text = die.value.ToString();
            label.color = Luminance(die.color) > 0.55f ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
        }
    }

    static float Luminance(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
}
