using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public GameObject coinPrefab;

    public float spawnInterval = 0.9f;
    public float minX = -4.0f;
    public float maxX = 4.0f;
    public float spawnY = 6f;
    public float coinSpawnChance = 0.25f;

    public float difficultyStepTime = 15f;

    private float timer;
    private float runTime;
    private bool stopSpawning = false;

    void Update()
    {
        if (stopSpawning) return;

        timer += Time.deltaTime;
        runTime += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPattern();
        }
    }

    void SpawnPattern()
    {
        if (Random.value < coinSpawnChance)
        {
            SpawnCoin();
            return;
        }

        int difficultyLevel = Mathf.FloorToInt(runTime / difficultyStepTime);

        if (difficultyLevel < 2)
        {
            SpawnSingle();
        }
        else if (difficultyLevel < 4)
        {
            if (Random.value < 0.5f)
                SpawnSingle();
            else
                SpawnDouble();
        }
        else
        {
            float r = Random.value;

            if (r < 0.4f)
                SpawnSingle();
            else if (r < 0.75f)
                SpawnDouble();
            else
                SpawnGap();
        }
    }

    void SpawnSingle()
    {
        float x = Random.Range(minX, maxX);
        SpawnObstacleAt(x);
    }

    void SpawnDouble()
    {
        float gap = 2.2f;
        float center = Random.Range(minX + gap, maxX - gap);

        SpawnObstacleAt(center - gap);
        SpawnObstacleAt(center + gap);
    }

    void SpawnGap()
    {
        float gapSize = 1.4f;
        float gapCenter = Random.Range(minX + gapSize, maxX - gapSize);

        int lanes = 7;
        float laneWidth = (maxX - minX) / lanes;

        for (int i = 0; i < lanes; i++)
        {
            float x = minX + laneWidth * i + laneWidth / 2f;

            if (Mathf.Abs(x - gapCenter) > gapSize)
            {
                SpawnObstacleAt(x);
            }
        }
    }

    void SpawnObstacleAt(float x)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return;

        int difficultyLevel = Mathf.FloorToInt(runTime / difficultyStepTime);
        int unlockedCount = 1 + (difficultyLevel / 2);
        unlockedCount = Mathf.Clamp(unlockedCount, 1, obstaclePrefabs.Length);

        GameObject prefab = obstaclePrefabs[Random.Range(0, unlockedCount)];
        Vector3 pos = new Vector3(x, spawnY, 0f);

        Instantiate(prefab, pos, prefab.transform.rotation);
    }

    void SpawnCoin()
    {
        float x = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        Instantiate(coinPrefab, pos, coinPrefab.transform.rotation);
    }

    public void StopSpawning()
    {
        stopSpawning = true;
    }
}