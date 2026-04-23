using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private enum SpawnPatternType
    {
        Single,
        Double,
        Gap,
        Triple,
        WideGapWall,
        RiskRewardGate,
        CoinSingle,
        CoinTrail
    }

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
    private SpawnPatternType lastPatternType = SpawnPatternType.Single;
    private int consecutiveHardPatterns;
    private const float OpeningGraceDuration = 12f;
    private const float OpeningFocusDuration = 24f;

    void Update()
    {
        if (stopSpawning)
            return;

        float worldSpeedMultiplier = 1f;
        float adjustedSpawnInterval = spawnInterval;

        if (GameManager.instance != null)
        {
            worldSpeedMultiplier = GameManager.instance.GetWorldSpeedMultiplier();
            adjustedSpawnInterval *= GameManager.instance.GetSpawnIntervalMultiplier();
        }

        adjustedSpawnInterval = Mathf.Max(0.64f, adjustedSpawnInterval);
        timer += Time.deltaTime * worldSpeedMultiplier;
        runTime += Time.deltaTime;

        if (timer >= adjustedSpawnInterval)
        {
            timer -= adjustedSpawnInterval;
            SpawnPattern();
        }
    }

    void SpawnPattern()
    {
        float currentCoinSpawnChance = coinSpawnChance;

        if (GameManager.instance != null)
            currentCoinSpawnChance = GameManager.instance.GetCurrentCoinSpawnChance(coinSpawnChance);

        int difficultyLevel = Mathf.FloorToInt(runTime / difficultyStepTime);

        if (ShouldForceRecoveryWindow(difficultyLevel))
        {
            SpawnRecoveryPattern(difficultyLevel);
            return;
        }

        if (Random.value < currentCoinSpawnChance)
        {
            if (difficultyLevel >= 3 && Random.value < 0.5f)
            {
                SpawnCoinTrail();
                RecordPattern(SpawnPatternType.CoinTrail);
            }
            else
            {
                SpawnCoin();
                RecordPattern(SpawnPatternType.CoinSingle);
            }

            return;
        }

        if (runTime < OpeningGraceDuration)
        {
            SpawnSingle();
            RecordPattern(SpawnPatternType.Single);
            return;
        }

        float patternRoll = Random.value;

        if (difficultyLevel < 2)
        {
            if (runTime < OpeningFocusDuration || patternRoll < 0.84f)
            {
                SpawnSingle();
                RecordPattern(SpawnPatternType.Single);
            }
            else
            {
                SpawnDouble();
                RecordPattern(SpawnPatternType.Double);
            }
        }
        else if (difficultyLevel < 4)
        {
            if (patternRoll < 0.38f)
            {
                SpawnSingle();
                RecordPattern(SpawnPatternType.Single);
            }
            else if (patternRoll < 0.74f)
            {
                SpawnDouble();
                RecordPattern(SpawnPatternType.Double);
            }
            else
            {
                SpawnTriple();
                RecordPattern(SpawnPatternType.Triple);
            }
        }
        else if (difficultyLevel < 7)
        {
            if (patternRoll < 0.24f)
            {
                SpawnSingle();
                RecordPattern(SpawnPatternType.Single);
            }
            else if (patternRoll < 0.48f)
            {
                SpawnDouble();
                RecordPattern(SpawnPatternType.Double);
            }
            else if (patternRoll < 0.72f)
            {
                SpawnGap(difficultyLevel);
                RecordPattern(SpawnPatternType.Gap);
            }
            else if (patternRoll < 0.88f)
            {
                SpawnTriple();
                RecordPattern(SpawnPatternType.Triple);
            }
            else
            {
                SpawnRiskRewardGate();
                RecordPattern(SpawnPatternType.RiskRewardGate);
            }
        }
        else
        {
            if (patternRoll < 0.2f)
            {
                SpawnDouble();
                RecordPattern(SpawnPatternType.Double);
            }
            else if (patternRoll < 0.42f)
            {
                SpawnGap(difficultyLevel);
                RecordPattern(SpawnPatternType.Gap);
            }
            else if (patternRoll < 0.62f)
            {
                SpawnTriple();
                RecordPattern(SpawnPatternType.Triple);
            }
            else if (patternRoll < 0.8f)
            {
                SpawnWideGapWall();
                RecordPattern(SpawnPatternType.WideGapWall);
            }
            else
            {
                SpawnRiskRewardGate();
                RecordPattern(SpawnPatternType.RiskRewardGate);
            }
        }
    }

    bool ShouldForceRecoveryWindow(int difficultyLevel)
    {
        if (consecutiveHardPatterns >= 2)
            return true;

        return difficultyLevel >= 4 &&
               (lastPatternType == SpawnPatternType.WideGapWall || lastPatternType == SpawnPatternType.RiskRewardGate) &&
               Random.value < 0.45f;
    }

    void SpawnRecoveryPattern(int difficultyLevel)
    {
        if (difficultyLevel >= 3 && Random.value < 0.58f)
        {
            SpawnCoinTrail();
            RecordPattern(SpawnPatternType.CoinTrail);
        }
        else
        {
            SpawnSingle();
            RecordPattern(SpawnPatternType.Single);
        }
    }

    void SpawnSingle()
    {
        float playerX = GetPlayerX();
        float x = Random.Range(minX, maxX);
        float minDistanceFromPlayer = runTime < OpeningGraceDuration ? 1.45f : 0.92f;

        if (Mathf.Abs(x - playerX) < minDistanceFromPlayer)
        {
            float direction = playerX >= 0f ? -1f : 1f;
            float minOffset = runTime < OpeningGraceDuration ? 1.55f : 1.1f;
            float maxOffset = runTime < OpeningFocusDuration ? 2.05f : 1.75f;
            x = Mathf.Clamp(playerX + (direction * Random.Range(minOffset, maxOffset)), minX, maxX);
        }

        SpawnObstacleAt(x, allowAdvancedHazards: true, allowWideHazards: true);
    }

    void SpawnDouble()
    {
        float gap = runTime < OpeningFocusDuration ? 2.5f : 2.2f;
        float playerX = GetPlayerX();
        float randomCenter = Random.Range(minX + gap, maxX - gap);
        float center = Mathf.Clamp(Mathf.Lerp(randomCenter, -playerX * 0.22f, 0.35f), minX + gap, maxX - gap);

        SpawnObstacleAt(center - gap, allowAdvancedHazards: false, allowWideHazards: false);
        SpawnObstacleAt(center + gap, allowAdvancedHazards: false, allowWideHazards: false);
    }

    void SpawnGap(int difficultyLevel)
    {
        float gapSize = difficultyLevel >= 8 ? 1.28f : 1.46f;
        float gapCenter = GetBiasedGapCenter(gapSize, difficultyLevel);
        int lanes = 7;
        float laneWidth = (maxX - minX) / lanes;

        for (int i = 0; i < lanes; i++)
        {
            float x = minX + laneWidth * i + laneWidth / 2f;

            if (Mathf.Abs(x - gapCenter) > gapSize)
                SpawnObstacleAt(x, allowAdvancedHazards: false, allowWideHazards: false);
        }
    }

    void SpawnTriple()
    {
        float[] lanes = GetLanePositions(5);
        int patternIndex = Random.Range(0, 4);

        switch (patternIndex)
        {
            case 0:
                SpawnObstacleAt(lanes[0], allowAdvancedHazards: false, allowWideHazards: false);
                SpawnObstacleAt(lanes[2], allowAdvancedHazards: false, allowWideHazards: false);
                SpawnObstacleAt(lanes[4], allowAdvancedHazards: false, allowWideHazards: false);
                break;
            case 1:
                SpawnObstacleAt(lanes[0], allowAdvancedHazards: false, allowWideHazards: false);
                SpawnObstacleAt(lanes[1], allowAdvancedHazards: false, allowWideHazards: false);
                SpawnObstacleAt(lanes[3], allowAdvancedHazards: false, allowWideHazards: false);
                break;
            case 2:
                SpawnObstacleAt(lanes[1], allowAdvancedHazards: false, allowWideHazards: false);
                SpawnObstacleAt(lanes[3], allowAdvancedHazards: false, allowWideHazards: false);
                SpawnObstacleAt(lanes[4], allowAdvancedHazards: false, allowWideHazards: false);
                break;
            default:
                SpawnObstacleAt(lanes[0], allowAdvancedHazards: false, allowWideHazards: false);
                SpawnObstacleAt(lanes[2], allowAdvancedHazards: false, allowWideHazards: false);
                SpawnObstacleAt(lanes[3], allowAdvancedHazards: false, allowWideHazards: false);
                break;
        }
    }

    void SpawnWideGapWall()
    {
        float[] lanes = GetLanePositions(7);
        int preferredIndex = GetNearestLaneIndex(lanes, GetPlayerX());
        int safeStartIndex = Mathf.Clamp(preferredIndex - 1, 1, 4);

        if (Random.value < 0.35f)
            safeStartIndex = Mathf.Clamp(safeStartIndex + Random.Range(-1, 2), 1, 4);

        for (int i = 0; i < lanes.Length; i++)
        {
            if (i == safeStartIndex || i == safeStartIndex + 1)
                continue;

            SpawnObstacleAt(lanes[i], allowAdvancedHazards: false, allowWideHazards: false);
        }
    }

    void SpawnRiskRewardGate()
    {
        float gateHalfWidth = 1.6f;
        float gateCenter = GetBiasedGapCenter(gateHalfWidth, Mathf.FloorToInt(runTime / difficultyStepTime));
        float leftX = gateCenter - gateHalfWidth;
        float rightX = gateCenter + gateHalfWidth;

        SpawnObstacleAt(leftX, allowAdvancedHazards: false, allowWideHazards: false);
        SpawnObstacleAt(rightX, allowAdvancedHazards: false, allowWideHazards: false);
        SpawnCoinAt(gateCenter, spawnY - 0.55f);

        if (Random.value < 0.45f)
            SpawnCoinAt(gateCenter, spawnY - 1.15f);
    }

    void SpawnObstacleAt(float x, bool allowAdvancedHazards, bool allowWideHazards)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return;

        GameObject prefab = SelectObstaclePrefab(Mathf.FloorToInt(runTime / difficultyStepTime), allowAdvancedHazards, allowWideHazards);

        if (prefab == null)
            return;

        Vector3 pos = new Vector3(x, spawnY, 0f);
        Instantiate(prefab, pos, prefab.transform.rotation);
    }

    GameObject SelectObstaclePrefab(int difficultyLevel, bool allowAdvancedHazards, bool allowWideHazards)
    {
        List<int> pool = new List<int>();

        AddPrefabIndexIfValid(pool, 0);

        if (runTime >= 14f)
            AddPrefabIndexIfValid(pool, 2);

        if (difficultyLevel >= 2 && runTime >= 24f)
            AddPrefabIndexIfValid(pool, 1);

        if (difficultyLevel >= 4 && allowWideHazards && runTime >= 40f)
            AddPrefabIndexIfValid(pool, 5);

        if (difficultyLevel >= 5 && allowAdvancedHazards && runTime >= 48f)
            AddPrefabIndexIfValid(pool, 3);

        if (difficultyLevel >= 7 && allowAdvancedHazards && runTime >= 60f)
            AddPrefabIndexIfValid(pool, 4);

        if (pool.Count == 0)
            return obstaclePrefabs[0];

        return obstaclePrefabs[pool[Random.Range(0, pool.Count)]];
    }

    void AddPrefabIndexIfValid(List<int> pool, int index)
    {
        if (obstaclePrefabs == null || index < 0 || index >= obstaclePrefabs.Length || obstaclePrefabs[index] == null)
            return;

        pool.Add(index);
    }

    void SpawnCoin()
    {
        float x = Mathf.Lerp(Random.Range(minX, maxX), GetPlayerX(), 0.28f);
        SpawnCoinAt(x, spawnY);
    }

    void SpawnCoinTrail()
    {
        float startX = Mathf.Lerp(Random.Range(minX + 0.8f, maxX - 0.8f), GetPlayerX(), 0.42f);
        float horizontalStep = Random.value < 0.5f ? 0.68f : -0.68f;
        float verticalStep = 0.62f;

        for (int i = 0; i < 4; i++)
        {
            float x = Mathf.Clamp(startX + (horizontalStep * i), minX + 0.15f, maxX - 0.15f);
            float y = spawnY + (verticalStep * i);
            SpawnCoinAt(x, y);
        }
    }

    void SpawnCoinAt(float x, float y)
    {
        if (coinPrefab == null)
            return;

        Vector3 pos = new Vector3(x, y, 0f);
        Instantiate(coinPrefab, pos, coinPrefab.transform.rotation);
    }

    float[] GetLanePositions(int laneCount)
    {
        float[] positions = new float[laneCount];
        float laneWidth = (maxX - minX) / Mathf.Max(1, laneCount - 1);

        for (int i = 0; i < laneCount; i++)
            positions[i] = minX + (laneWidth * i);

        return positions;
    }

    float GetPlayerX()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
            return GameManager.instance.player.transform.position.x;

        return 0f;
    }

    float GetBiasedGapCenter(float padding, int difficultyLevel)
    {
        float playerX = Mathf.Clamp(GetPlayerX(), minX + padding, maxX - padding);
        float randomCenter = Random.Range(minX + padding, maxX - padding);
        float randomness = Mathf.Lerp(0.24f, 0.62f, Mathf.Clamp01(difficultyLevel / 8f));
        return Mathf.Clamp(Mathf.Lerp(playerX, randomCenter, randomness), minX + padding, maxX - padding);
    }

    int GetNearestLaneIndex(float[] lanes, float x)
    {
        int bestIndex = 0;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < lanes.Length; i++)
        {
            float distance = Mathf.Abs(lanes[i] - x);

            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestIndex = i;
        }

        return bestIndex;
    }

    void RecordPattern(SpawnPatternType type)
    {
        bool hardPattern = type == SpawnPatternType.Gap ||
                           type == SpawnPatternType.Triple ||
                           type == SpawnPatternType.WideGapWall ||
                           type == SpawnPatternType.RiskRewardGate;

        consecutiveHardPatterns = hardPattern ? consecutiveHardPatterns + 1 : 0;
        lastPatternType = type;
    }

    public void StopSpawning()
    {
        stopSpawning = true;
    }
}
