using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged;
    public UnityEvent onDeath;

    private bool isDead;
    private bool initialized;

    public float MaxHealth => maxHealth;
    public float CurrentHealth
    {
        get
        {
            EnsureInitialized();
            return currentHealth;
        }
        private set => currentHealth = value;
    }
    public bool IsDead => isDead;
    public float HealthPercent => maxHealth <= 0f ? 0f : CurrentHealth / maxHealth;

    private float currentHealth;

    private void Awake()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        CurrentHealth = maxHealth;
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        EnsureInitialized();

        if (isDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        EnsureInitialized();

        if (isDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        initialized = true;
        isDead = false;
        CurrentHealth = maxHealth;
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void SetMaxHealth(float value, bool fillHealth = true)
    {
        EnsureInitialized();

        maxHealth = Mathf.Max(1f, value);

        if (fillHealth)
        {
            CurrentHealth = maxHealth;
        }
        else
        {
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        }

        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void Kill()
    {
        EnsureInitialized();

        if (isDead)
        {
            return;
        }

        CurrentHealth = 0f;
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Die();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        onDeath?.Invoke();

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
