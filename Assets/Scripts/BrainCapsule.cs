using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Damageable))]
public class BrainCapsule : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] private bool pauseGameOnDestroyed = true;

    [Header("Events")]
    public UnityEvent onCapsuleDestroyed;

    private Damageable damageable;
    private bool destroyed;

    public Damageable Health
    {
        get
        {
            EnsureDamageable();
            return damageable;
        }
    }
    public bool IsDestroyed => destroyed;
    public float CurrentHealth => Health != null ? Health.CurrentHealth : 0f;
    public float MaxHealth => Health != null ? Health.MaxHealth : 0f;
    public float HealthPercent => Health != null ? Health.HealthPercent : 0f;

    private void Awake()
    {
        EnsureDamageable();
        damageable.onDeath.AddListener(HandleDestroyed);
    }

    private void EnsureDamageable()
    {
        if (damageable != null)
        {
            return;
        }

        damageable = GetComponent<Damageable>();
        damageable.EnsureInitialized();
    }

    private void HandleDestroyed()
    {
        if (destroyed)
        {
            return;
        }

        destroyed = true;
        onCapsuleDestroyed?.Invoke();

        if (pauseGameOnDestroyed)
        {
            Time.timeScale = 0f;
        }
    }

    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }

    public float GetMaxHealth()
    {
        return MaxHealth;
    }

    public float GetHealthPercent()
    {
        return HealthPercent;
    }

    private void OnDestroy()
    {
        if (damageable != null)
        {
            damageable.onDeath.RemoveListener(HandleDestroyed);
        }
    }
}
