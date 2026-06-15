using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ZombieWaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZombieController zombiePrefab;
    [SerializeField] private BrainCapsule brainCapsule;
    [SerializeField] private Transform[] spawnPoints = new Transform[2];
    [SerializeField] private bool hideSceneTemplateOnStart = true;

    [Header("Wave Settings")]
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private int firstWaveZombieCount = 15;
    [SerializeField] private int finalWaveZombieCount = 100;
    [SerializeField] private int finalWaveNumber = 10;
    [SerializeField] private float timeBetweenWaves = 8f;
    [SerializeField] private float timeBetweenSpawns = 0.45f;
    [SerializeField] private int maxAliveZombies = 35;

    [Header("Zombie Scaling")]
    [SerializeField] private float healthMultiplierPerWave = 1.15f;
    [SerializeField] private float speedMultiplierPerWave = 1.08f;

    [Header("Events")]
    public UnityEvent<int> onWaveStarted;
    public UnityEvent<int> onWaveCompleted;
    public UnityEvent<int, int, int> onWaveStatusChanged;
    public UnityEvent onAllWavesCleared;

    private int currentWave;
    private int aliveZombies;
    private int currentWaveZombieCount;
    private bool allWavesCleared;
    private Coroutine waveRoutine;
    private readonly List<Damageable> aliveZombieHealth = new List<Damageable>();

    public int CurrentWave => currentWave;
    public int AliveZombies => aliveZombies;
    public int CurrentWaveZombieCount => currentWaveZombieCount;
    public int FinalWaveNumber => finalWaveNumber;
    public bool AllWavesCleared => allWavesCleared;
    public string CurrentWaveText => $"Horda {currentWave}/{finalWaveNumber}";

    private void Start()
    {
        HideSceneTemplateIfNeeded();

        if (startAutomatically)
        {
            StartWaves();
        }
    }

    public void StartWaves()
    {
        if (waveRoutine != null)
        {
            return;
        }

        waveRoutine = StartCoroutine(WaveLoop());
    }

    public void StopWaves()
    {
        if (waveRoutine == null)
        {
            return;
        }

        StopCoroutine(waveRoutine);
        waveRoutine = null;
    }

    private IEnumerator WaveLoop()
    {
        while ((brainCapsule == null || !brainCapsule.IsDestroyed) && currentWave < finalWaveNumber)
        {
            currentWave++;
            onWaveStarted?.Invoke(currentWave);
            NotifyWaveStatusChanged();

            currentWaveZombieCount = GetZombieCountForWave(currentWave);
            NotifyWaveStatusChanged();
            yield return SpawnWave(currentWaveZombieCount);

            yield return new WaitUntil(() => GetAliveZombieCount() <= 0 || brainCapsule == null || brainCapsule.IsDestroyed);
            onWaveCompleted?.Invoke(currentWave);
            NotifyWaveStatusChanged();

            if (brainCapsule != null && brainCapsule.IsDestroyed)
            {
                break;
            }

            if (currentWave >= finalWaveNumber)
            {
                break;
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        if (currentWave >= finalWaveNumber && GetAliveZombieCount() <= 0 && (brainCapsule == null || !brainCapsule.IsDestroyed))
        {
            allWavesCleared = true;
            onAllWavesCleared?.Invoke();
        }

        waveRoutine = null;
    }

    private IEnumerator SpawnWave(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            yield return new WaitUntil(() => GetAliveZombieCount() < maxAliveZombies);

            SpawnZombie();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private void SpawnZombie()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieWaveSpawner precisa de um Zombie Prefab.", this);
            StopWaves();
            return;
        }

        Transform spawnPoint = GetSpawnPoint();
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        ZombieController zombie = Instantiate(zombiePrefab, spawnPosition, spawnRotation);
        zombie.gameObject.SetActive(true);
        zombie.SetTarget(brainCapsule);

        ZombieNavMeshMover mover = zombie.GetComponent<ZombieNavMeshMover>();
        if (mover != null)
        {
            mover.SetTarget(brainCapsule);
            mover.ConfigureForWave(GetSpeedMultiplierForWave(currentWave));
        }

        ScaleZombieForWave(zombie);

        Damageable zombieHealth = zombie.GetComponent<Damageable>();
        aliveZombieHealth.Add(zombieHealth);
        aliveZombies = GetAliveZombieCount();
        zombieHealth.onDeath.AddListener(() =>
        {
            aliveZombieHealth.Remove(zombieHealth);
            aliveZombies = GetAliveZombieCount();
            NotifyWaveStatusChanged();
        });
        NotifyWaveStatusChanged();
    }

    private Transform GetSpawnPoint()
    {
        Transform[] validSpawnPoints = System.Array.FindAll(spawnPoints, point => point != null);

        if (validSpawnPoints.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, validSpawnPoints.Length);
        return validSpawnPoints[index];
    }

    private void ScaleZombieForWave(ZombieController zombie)
    {
        float healthMultiplier = Mathf.Pow(healthMultiplierPerWave, currentWave - 1);
        zombie.ConfigureForWave(healthMultiplier);
    }

    private float GetSpeedMultiplierForWave(int wave)
    {
        return Mathf.Pow(speedMultiplierPerWave, wave - 1);
    }

    private int GetZombieCountForWave(int wave)
    {
        if (finalWaveNumber <= 1)
        {
            return finalWaveZombieCount;
        }

        float progress = Mathf.InverseLerp(1f, finalWaveNumber, wave);
        return Mathf.RoundToInt(Mathf.Lerp(firstWaveZombieCount, finalWaveZombieCount, progress));
    }

    private void NotifyWaveStatusChanged()
    {
        aliveZombies = GetAliveZombieCount();
        onWaveStatusChanged?.Invoke(currentWave, aliveZombies, currentWaveZombieCount);
    }

    private int GetAliveZombieCount()
    {
        for (int i = aliveZombieHealth.Count - 1; i >= 0; i--)
        {
            Damageable zombieHealth = aliveZombieHealth[i];

            if (zombieHealth == null || zombieHealth.IsDead)
            {
                aliveZombieHealth.RemoveAt(i);
            }
        }

        return aliveZombieHealth.Count;
    }

    private void HideSceneTemplateIfNeeded()
    {
        if (!hideSceneTemplateOnStart || zombiePrefab == null || !zombiePrefab.gameObject.scene.IsValid())
        {
            return;
        }

        zombiePrefab.gameObject.SetActive(false);
    }
}
