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
    public Text bestScoreText;

    private void Start()
    {
        if (DataManager.instance != null)
        {
            bestScoreText.text = $"Best Score : {DataManager.instance.bestPlayerName} : {DataManager.instance.bestScore}";

            if (!string.IsNullOrEmpty(DataManager.instance.playerName))
            {
                nameInputField.text = DataManager.instance.playerName;
            }
        }
    }

    public void NewNameSelected(string name)
    {
        if (DataManager.instance != null)
        {
            DataManager.instance.playerName = name;
        }
    }

    public void StartNew()
    {
        if (DataManager.instance != null && nameInputField != null)
        {
            DataManager.instance.playerName = nameInputField.text;
        }

        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
        if (DataManager.instance != null)
        {
            DataManager.instance.SaveHighScore();
        }

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}

