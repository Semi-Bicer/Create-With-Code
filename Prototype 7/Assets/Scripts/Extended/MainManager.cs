using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }

    public Color TeamColor;
    
    public ResourceDatabase ResourceDatabase;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        TeamColor = Color.white;
        
        ResourceDatabase.Init();

        //we try to load the savefile if there is one to retrieve the right color
        LoadColor();
    }

    //Code related to saving and loading the color

    //we wrap the color in a class as Json utility need it. And mark this class as Serializable.
    //If we ever want to save more thing (like name, or score etc..) we can just add it to that class!
    [System.Serializable]
    class SaveData
    {
        public Color TeamColor;
    }
    
    public void SaveColor()
    {
        SaveData data = new SaveData();
        data.TeamColor = TeamColor;

        string json = JsonUtility.ToJson(data);
        
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    public void LoadColor()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            TeamColor = data.TeamColor;
        }
    }
}
