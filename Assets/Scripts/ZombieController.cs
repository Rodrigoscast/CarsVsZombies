using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(ZombieHitboxSetup))]
public class ZombieController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private BrainCapsule brainCapsule;
    [SerializeField] private string capsuleTag = "BrainCapsule";

    [Header("Brain Detection")]
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float attackRangePadding = 0.25f;
    [SerializeField] private float attackStickTime = 0.75f;

    [Header("Attack")]
    [SerializeField] private float damagePerAttack = 10f;
    [SerializeField] private float attacksPerSecond = 1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string runningParameter = "IsRunning";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string deathTrigger = "Die";
    [SerializeField] private float runningSpeedThreshold = 3.5f;

    [Header("Death")]
    [SerializeField] private bool destroyAfterDeath = true;
    [SerializeField] private float deathDestroyDelay = 2f;

    [Header("Events")]
    public UnityEvent<float, float> onZombieHealthChanged;
    public UnityEvent onZombieDied;

    private Damageable health;
    private Damageable capsuleHealth;
    private Collider brainCapsuleCollider;
    private float nextAttackTime;
    private float attackLockUntil;
    private bool hasSpeedParameter;
    private bool hasRunningParameter;
    private bool hasAttackTrigger;
    private bool hasDeathTrigger;

    private void Awake()
    {
        health = GetComponent<Damageable>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        CacheAnimatorParameters();
        health.onDeath.AddListener(HandleDeath);
        health.onHealthChanged.AddListener(HandleHealthChanged);
    }

    private void Start()
    {
        FindTargetIfNeeded();
    }

    private void Update()
    {
        if (health.IsDead || brainCapsule == null || brainCapsule.IsDestroyed)
        {
            SetAnimationSpeed(0f);
            return;
        }

        if (IsInAttackRange())
        {
            TryAttack();
        }
    }

    public void SetTarget(BrainCapsule target)
    {
        brainCapsule = target;
        capsuleHealth = target != null ? target.Health : null;
        brainCapsuleCollider = target != null ? target.GetComponentInChildren<Collider>() : null;
    }

    public float AttackRange => attackRange;

    public bool IsInAttackRange()
    {
        if (brainCapsule == null)
        {
            return false;
        }

        Vector3 targetPoint = brainCapsule.transform.position;

        if (brainCapsuleCollider != null)
        {
            targetPoint = brainCapsuleCollider.ClosestPoint(transform.position);
        }

        float distance = Vector3.Distance(transform.position, targetPoint);

        if (distance <= attackRange + attackRangePadding)
        {
            attackLockUntil = Time.time + attackStickTime;
            return true;
        }

        return Time.time <= attackLockUntil;
    }

    public void ConfigureForWave(float healthMultiplier)
    {
        healthMultiplier = Mathf.Max(1f, healthMultiplier);
        health.SetMaxHealth(health.MaxHealth * healthMultiplier);
    }

    public float GetCurrentHealth()
    {
        return health != null ? health.CurrentHealth : 0f;
    }

    public float GetMaxHealth()
    {
        return health != null ? health.MaxHealth : 0f;
    }

    public float GetHealthPercent()
    {
        return health != null ? health.HealthPercent : 0f;
    }

    public Damageable GetHealthComponent()
    {
        return health;
    }

    public void SetMovementSpeed(float speed)
    {
        if (animator != null && hasSpeedParameter)
        {
            animator.SetFloat(speedParameter, speed);
        }

        if (animator != null && hasRunningParameter)
        {
            animator.SetBool(runningParameter, speed >= runningSpeedThreshold);
        }
    }

    private void FindTargetIfNeeded()
    {
        if (brainCapsule == null)
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag(capsuleTag);
            if (targetObject != null)
            {
                brainCapsule = targetObject.GetComponent<BrainCapsule>();
            }
        }

        if (brainCapsule != null)
        {
            capsuleHealth = brainCapsule.Health;
            brainCapsuleCollider = brainCapsule.GetComponentInChildren<Collider>();
        }
    }

    private void TryAttack()
    {
        if (capsuleHealth == null || capsuleHealth.IsDead || Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, attacksPerSecond);
        capsuleHealth.TakeDamage(damagePerAttack);

        if (animator != null && hasAttackTrigger)
        {
            animator.SetTrigger(attackTrigger);
        }
    }

    private void HandleDeath()
    {
        SetAnimationSpeed(0f);
        onZombieDied?.Invoke();

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider zombieCollider in colliders)
        {
            zombieCollider.enabled = false;
        }

        if (animator != null && hasDeathTrigger)
        {
            animator.SetTrigger(deathTrigger);
        }

        if (destroyAfterDeath)
        {
            Destroy(gameObject, deathDestroyDelay);
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        onZombieHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void SetAnimationSpeed(float speed)
    {
        SetMovementSpeed(speed);
    }

    private void CacheAnimatorParameters()
    {
        if (animator == null)
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == speedParameter && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasSpeedParameter = true;
            }
            else if (parameter.name == runningParameter && parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasRunningParameter = true;
            }
            else if (parameter.name == attackTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                hasAttackTrigger = true;
            }
            else if (parameter.name == deathTrigger && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                hasDeathTrigger = true;
            }
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.onDeath.RemoveListener(HandleDeath);
            health.onHealthChanged.RemoveListener(HandleHealthChanged);
        }
    }
}
