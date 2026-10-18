using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One line of the dice panel: die name, minus button, count, plus button.
public class DiceCountRow : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text countText;
    [SerializeField] Button minusButton;
    [SerializeField] Button plusButton;

    public void Bind(string dieName, Action onMinus, Action onPlus)
    {
        nameText.text = dieName;
        minusButton.onClick.RemoveAllListeners();
        plusButton.onClick.RemoveAllListeners();
        minusButton.onClick.AddListener(() => onMinus());
        plusButton.onClick.AddListener(() => onPlus());
    }

    public void Show(int count, bool canRemove, bool canAdd)
    {
        countText.text = count.ToString();
        countText.alpha = count > 0 ? 1f : 0.45f;
        minusButton.interactable = canRemove;
        plusButton.interactable = canAdd;
    }
}
