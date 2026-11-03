using UnityEngine;
using System.IO;


public class DataManager : MonoBehaviour
{
    public static DataManager instance; // singleton

    public string playerName;
    
    public string bestPlayerName;
    public int bestScore;
    
    
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighScore();
        }
    }
    
    public void SaveHighScore()
    {
        SaveData data = new SaveData();
        data.bestPlayerName = bestPlayerName;
        data.bestScore = bestScore;
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    public void LoadHighScore()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            bestPlayerName = data.bestPlayerName;
            bestScore = data.bestScore;
            
        }
        
    }
    
    [System.Serializable]
    class SaveData
    {
        public string bestPlayerName;
        public int bestScore;
    }
}
