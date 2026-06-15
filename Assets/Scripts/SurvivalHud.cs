using UnityEngine;
using UnityEngine.UI;

public class SurvivalHud : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BrainCapsule brainCapsule;
    [SerializeField] private CarHealth carHealth;
    [SerializeField] private ZombieWaveSpawner waveSpawner;
    [SerializeField] private ZombieController trackedZombie;

    [Header("Nexus UI")]
    [SerializeField] private Slider nexusHealthSlider;
    [SerializeField] private Text nexusHealthText;

    [Header("Car UI")]
    [SerializeField] private Slider carHealthSlider;
    [SerializeField] private Text carHealthText;

    [Header("Zombie UI")]
    [SerializeField] private Slider zombieHealthSlider;
    [SerializeField] private Text zombieHealthText;

    [Header("Wave UI")]
    [SerializeField] private Text waveText;
    [SerializeField] private Text aliveZombiesText;

    private Damageable nexusDamageable;
    private Damageable trackedZombieDamageable;

    private void Start()
    {
        FindMissingReferences();
        Subscribe();
        RefreshAll();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void TrackZombie(ZombieController zombie)
    {
        if (trackedZombieDamageable != null)
        {
            trackedZombieDamageable.onHealthChanged.RemoveListener(UpdateZombieHealth);
        }

        trackedZombie = zombie;
        trackedZombieDamageable = trackedZombie != null ? trackedZombie.GetHealthComponent() : null;

        if (trackedZombieDamageable != null)
        {
            trackedZombieDamageable.onHealthChanged.AddListener(UpdateZombieHealth);
            UpdateZombieHealth(trackedZombieDamageable.CurrentHealth, trackedZombieDamageable.MaxHealth);
        }
    }

    private void Subscribe()
    {
        if (brainCapsule != null)
        {
            nexusDamageable = brainCapsule.Health;
            if (nexusDamageable != null)
            {
                nexusDamageable.onHealthChanged.AddListener(UpdateNexusHealth);
            }
        }

        if (carHealth != null)
        {
            carHealth.onCarHealthChanged.AddListener(UpdateCarHealth);
        }

        if (waveSpawner != null)
        {
            waveSpawner.onWaveStatusChanged.AddListener(UpdateWave);
        }

        TrackZombie(trackedZombie);
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
    }

    private void Unsubscribe()
    {
        if (nexusDamageable != null)
        {
            nexusDamageable.onHealthChanged.RemoveListener(UpdateNexusHealth);
        }

        if (carHealth != null)
        {
            carHealth.onCarHealthChanged.RemoveListener(UpdateCarHealth);
        }

        if (waveSpawner != null)
        {
            waveSpawner.onWaveStatusChanged.RemoveListener(UpdateWave);
        }

        if (trackedZombieDamageable != null)
        {
            trackedZombieDamageable.onHealthChanged.RemoveListener(UpdateZombieHealth);
        }
    }

    private void RefreshAll()
    {
        if (brainCapsule != null)
        {
            UpdateNexusHealth(brainCapsule.CurrentHealth, brainCapsule.MaxHealth);
        }

        if (carHealth != null)
        {
            UpdateCarHealth(carHealth.CurrentHealth, carHealth.MaxHealth);
        }

        if (trackedZombie != null)
        {
            UpdateZombieHealth(trackedZombie.GetCurrentHealth(), trackedZombie.GetMaxHealth());
        }

        if (waveSpawner != null)
        {
            UpdateWave(waveSpawner.CurrentWave, waveSpawner.AliveZombies, waveSpawner.CurrentWaveZombieCount);
        }
    }

    private void UpdateNexusHealth(float currentHealth, float maxHealth)
    {
        SetHealthUI(nexusHealthSlider, nexusHealthText, "Nexus", currentHealth, maxHealth);
    }

    private void UpdateCarHealth(float currentHealth, float maxHealth)
    {
        SetHealthUI(carHealthSlider, carHealthText, "Carro", currentHealth, maxHealth);
    }

    private void UpdateZombieHealth(float currentHealth, float maxHealth)
    {
        SetHealthUI(zombieHealthSlider, zombieHealthText, "Zumbi", currentHealth, maxHealth);
    }

    private void UpdateWave(int currentWave, int aliveZombies, int waveZombieCount)
    {
        if (waveText != null)
        {
            waveText.text = currentWave <= 0 || waveSpawner == null ? "Horda 0" : $"Horda {currentWave}/{waveSpawner.FinalWaveNumber}";
        }

        if (aliveZombiesText != null)
        {
            aliveZombiesText.text = $"Zumbis {aliveZombies}/{waveZombieCount}";
        }
    }

    private void SetHealthUI(Slider slider, Text label, string name, float currentHealth, float maxHealth)
    {
        float percent = maxHealth <= 0f ? 0f : currentHealth / maxHealth;

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = percent;
        }

        if (label != null)
        {
            label.text = $"{name} {Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }
}
