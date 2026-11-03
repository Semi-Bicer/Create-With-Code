using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(1000)]
public class MenuUIHandler : MonoBehaviour
{
    public InputField nameInputField;

    private void Start()
    {
        if (DataManager.Instance != null && nameInputField != null)
        {
            nameInputField.text = DataManager.Instance.PlayerName;
        }
    }

    // ABSTRACTION
    public void StartNew()
    {
        SavePlayerName();
        SceneManager.LoadScene(1);
    }

    // ABSTRACTION
    private void SavePlayerName()
    {
        if (DataManager.Instance != null && nameInputField != null)
        {
            DataManager.Instance.PlayerName = nameInputField.text;
        }
    }

    // ABSTRACTION
    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}

