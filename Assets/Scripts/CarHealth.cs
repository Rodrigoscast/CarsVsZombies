using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Damageable))]
public class CarHealth : MonoBehaviour
{
    [Header("Collision Damage")]
    [SerializeField] private float minimumDamageSpeed = 10f;
    [SerializeField] private float damagePerImpactSpeed = 1.2f;
    [SerializeField] private float collisionDamageCooldown = 0.4f;
    [SerializeField] private string zombieTag = "Zombie";
    [SerializeField] private LayerMask solidDamageLayers = ~0;

    [Header("Collision Sound")]
    [SerializeField] private AudioSource collisionAudioSource;
    [SerializeField] private AudioClip collisionClip;
    [SerializeField] private float minimumSoundSpeed = 3f;
    [SerializeField] private float maxSoundVolumeSpeed = 24f;

    [Header("Events")]
    public UnityEvent<float, float> onCarHealthChanged;
    public UnityEvent onCarDestroyed;

    private Damageable health;
    private float nextDamageTime;

    public float CurrentHealth => health != null ? health.CurrentHealth : 0f;
    public float MaxHealth => health != null ? health.MaxHealth : 0f;
    public float HealthPercent => health != null ? health.HealthPercent : 0f;

    private void Awake()
    {
        health = GetComponent<Damageable>();

        if (collisionAudioSource == null)
        {
            collisionAudioSource = GetComponent<AudioSource>();
        }

        health.onHealthChanged.AddListener(HandleHealthChanged);
        health.onDeath.AddListener(HandleDestroyed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsZombieCollision(collision.collider))
        {
            return;
        }

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (Time.time < nextDamageTime || impactSpeed < minimumDamageSpeed)
        {
            return;
        }

        if (!IsSolidDamageLayer(collision.collider.gameObject.layer))
        {
            return;
        }

        float damage = (impactSpeed - minimumDamageSpeed) * damagePerImpactSpeed;
        health.TakeDamage(damage);
        PlayCollisionSound(impactSpeed);
        nextDamageTime = Time.time + collisionDamageCooldown;
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

    private bool IsZombieCollision(Collider other)
    {
        ZombieController zombie = other.GetComponentInParent<ZombieController>();
        if (zombie != null)
        {
            return true;
        }

        return !string.IsNullOrEmpty(zombieTag) && other.CompareTag(zombieTag);
    }

    private bool IsSolidDamageLayer(int layer)
    {
        return (solidDamageLayers.value & (1 << layer)) != 0;
    }

    private void PlayCollisionSound(float impactSpeed)
    {
        if (collisionAudioSource == null || collisionClip == null || impactSpeed < minimumSoundSpeed)
        {
            return;
        }

        float volume = Mathf.InverseLerp(minimumSoundSpeed, maxSoundVolumeSpeed, impactSpeed);
        collisionAudioSource.PlayOneShot(collisionClip, Mathf.Clamp01(volume));
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        onCarHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void HandleDestroyed()
    {
        onCarDestroyed?.Invoke();
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.onHealthChanged.RemoveListener(HandleHealthChanged);
            health.onDeath.RemoveListener(HandleDestroyed);
        }
    }
}
