using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
 * Lets the player pick how many of each die type go into the next roll. Rows are cloned from an inactive template, one
 * per entry in the spawner's dice set. The total stays between 1 and maxTotal so the tower never gets an empty roll or
 * more dice than it passes without jamming. Counts are kept between sessions in PlayerPrefs, and the panel is locked
 * while a roll is in progress.
 */
public class DiceSetPanel : MonoBehaviour
{
    const string PrefsKey = "DiceTower.Count.";

    [SerializeField] DiceSpawner spawner;
    [SerializeField] RollManager manager;
    [SerializeField] DiceCountRow rowTemplate;
    [SerializeField] TMP_Text totalText;
    [SerializeField] CanvasGroup group;
    [SerializeField, Min(1)] int maxTotal = 10;
    [SerializeField, Range(0f, 1f)] float lockedAlpha = 0.45f;

    readonly List<DiceCountRow> rows = new List<DiceCountRow>();

    public int MaxTotal => maxTotal;

    // Awake, not Start, so the saved set is in the spawner before anything can throw.
    void Awake()
    {
        LoadCounts();
    }

    void OnEnable()
    {
        manager.StateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        manager.StateChanged -= OnStateChanged;
    }

    void Start()
    {
        rowTemplate.gameObject.SetActive(false);
        for (int i = 0; i < spawner.EntryCount; i++)
        {
            int index = i;
            DiceCountRow row = Instantiate(rowTemplate, rowTemplate.transform.parent);
            row.name = $"Row_{DieName(spawner.GetPrefab(i))}";
            row.gameObject.SetActive(true);
            row.Bind(DieName(spawner.GetPrefab(i)), () => Change(index, -1), () => Change(index, 1));
            rows.Add(row);
        }

        Refresh();
        OnStateChanged(manager.State);
    }

    public void Change(int index, int delta)
    {
        if (!manager.CanThrow) return;

        int total = spawner.DiceCount + delta;
        int next = spawner.GetCount(index) + delta;
        if (next < 0 || total < 1 || total > maxTotal) return;

        spawner.SetCount(index, next);
        spawner.PrepareHand();
        PlayerPrefs.SetInt(Key(index), next);
        PlayerPrefs.Save();
        Refresh();
    }

    void Refresh()
    {
        int total = spawner.DiceCount;
        for (int i = 0; i < rows.Count; i++)
        {
            int count = spawner.GetCount(i);
            rows[i].Show(count, count > 0 && total > 1, total < maxTotal);
        }
        totalText.text = $"{total} / {maxTotal}";
    }

    void OnStateChanged(RollState state)
    {
        bool open = manager.CanThrow;
        group.interactable = open;
        group.blocksRaycasts = open;
        group.alpha = open ? 1f : lockedAlpha;
    }

    // Saved counts replace the scene defaults unless they add up to an impossible set.
    void LoadCounts()
    {
        var defaults = new int[spawner.EntryCount];
        for (int i = 0; i < defaults.Length; i++)
        {
            defaults[i] = spawner.GetCount(i);
            if (PlayerPrefs.HasKey(Key(i))) spawner.SetCount(i, PlayerPrefs.GetInt(Key(i)));
        }

        int total = spawner.DiceCount;
        if (total >= 1 && total <= maxTotal) return;
        for (int i = 0; i < defaults.Length; i++) spawner.SetCount(i, defaults[i]);
    }

    string Key(int index) => PrefsKey + spawner.GetPrefab(index).name;

    static string DieName(Die prefab)
    {
        var reader = prefab.GetComponent<DiceReader>();
        bool known = reader != null && reader.faceTable != null && reader.faceTable.sides > 0;
        return known ? "d" + reader.faceTable.sides : prefab.name;
    }
}
