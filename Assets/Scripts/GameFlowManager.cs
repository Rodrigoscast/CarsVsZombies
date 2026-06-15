using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BrainCapsule brainCapsule;
    [SerializeField] private CarHealth carHealth;
    [SerializeField] private ZombieWaveSpawner waveSpawner;

    [Header("Screens")]
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject defeatScreen;

    [Header("Options")]
    [SerializeField] private bool pauseOnGameEnd = true;

    private bool gameEnded;

    private void Start()
    {
        Time.timeScale = 1f;
        FindMissingReferences();
        SetScreen(victoryScreen, false);
        SetScreen(defeatScreen, false);

        if (brainCapsule != null)
        {
            brainCapsule.onCapsuleDestroyed.AddListener(Defeat);
        }

        if (carHealth != null)
        {
            carHealth.onCarDestroyed.AddListener(Defeat);
        }

        if (waveSpawner != null)
        {
            waveSpawner.onAllWavesCleared.AddListener(Victory);
        }
    }

    public void Victory()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        SetScreen(victoryScreen, true);
        EndGameTime();
    }

    public void Defeat()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;
        SetScreen(defeatScreen, true);
        EndGameTime();
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void FindMissingReferences()
    {
        if (brainCapsule == null)
        {
            GameObject nexusObject = GameObject.FindGameObjectWithTag("BrainCapsule");
            if (nexusObject != null)
            {
                brainCapsule = nexusObject.GetComponent<BrainCapsule>();
            }
        }

        if (carHealth == null)
        {
            carHealth = FindAnyObjectByType<CarHealth>();
        }

        if (waveSpawner == null)
        {
            waveSpawner = FindAnyObjectByType<ZombieWaveSpawner>();
        }
    }

    private void EndGameTime()
    {
        if (pauseOnGameEnd)
        {
            Time.timeScale = 0f;
        }
    }

    private void SetScreen(GameObject screen, bool active)
    {
        if (screen != null)
        {
            screen.SetActive(active);
        }
    }

    private void OnDestroy()
    {
        if (brainCapsule != null)
        {
            brainCapsule.onCapsuleDestroyed.RemoveListener(Defeat);
        }

        if (carHealth != null)
        {
            carHealth.onCarDestroyed.RemoveListener(Defeat);
        }

        if (waveSpawner != null)
        {
            waveSpawner.onAllWavesCleared.RemoveListener(Victory);
        }
    }
}
