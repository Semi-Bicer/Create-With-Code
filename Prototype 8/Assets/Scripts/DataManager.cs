using UnityEngine;

public class DataManager : MonoBehaviour
{
    // ENCAPSULATION
    public static DataManager Instance { get; private set; }

    private string m_PlayerName = "Player";

    // ENCAPSULATION
    public string PlayerName
    {
        get { return m_PlayerName; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                m_PlayerName = "Player";
            }
            else if (value.Length > 15)
            {
                m_PlayerName = value.Substring(0, 15);
            }
            else
            {
                m_PlayerName = value;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

