using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Tooltip("Enemy")]
    public GameObject enemyObject;
    private int enemyCount;
    private int waveNumber = 1;
    private float spawnRange = 9;
    [Tooltip("Powerup")] public GameObject powerup;    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(powerup, GenerateSpawnPos(), powerup.transform.rotation);   
        SpawnEnemyWave(waveNumber);
    }

    private Vector3 GenerateSpawnPos()
    {
        float spawnPosX = Random.Range(-spawnRange, spawnRange);
        float spawnPosZ = Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(spawnPosX, 0, spawnPosZ);
        return randomPos;
    }

    void SpawnEnemyWave(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Instantiate(enemyObject, GenerateSpawnPos(), enemyObject.transform.rotation);
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        enemyCount = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length;
        if (enemyCount == 0)
        {
            waveNumber++;
            Instantiate(powerup, GenerateSpawnPos(), enemyObject.transform.rotation);
            SpawnEnemyWave(waveNumber);
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
