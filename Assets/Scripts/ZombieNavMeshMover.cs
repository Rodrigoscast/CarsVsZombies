using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieNavMeshMover : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private BrainCapsule brainCapsule;
    [SerializeField] private string capsuleTag = "BrainCapsule";

    [Header("Movement")]
    [SerializeField] private float repathInterval = 0.25f;
    [SerializeField] private float stopDistance = 1.4f;
    [SerializeField] private float navMeshSearchRadius = 3f;
    [SerializeField] private float routeVariationRadius = 5f;
    [SerializeField] private float routeVariationInterval = 2f;
    [SerializeField] private float directApproachDistance = 8f;
    [SerializeField] private bool faceNexusWhileAttacking = true;

    private NavMeshAgent agent;
    private Damageable health;
    private ZombieController zombieController;
    private float nextRepathTime;
    private float nextRouteVariationTime;
    private float baseSpeed;
    private int originalAvoidancePriority;
    private Vector3 currentDestination;
    private bool isBeingKnockedBack;
    private bool isLockedInAttack;
    private Coroutine knockbackRoutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Damageable>();
        zombieController = GetComponent<ZombieController>();
        baseSpeed = agent.speed;
        originalAvoidancePriority = agent.avoidancePriority;
    }

    private void Start()
    {
        FindTargetIfNeeded();
        if (zombieController != null)
        {
            stopDistance = Mathf.Max(stopDistance, zombieController.AttackRange * 0.9f);
        }

        agent.stoppingDistance = stopDistance;
        PlaceOnNavMeshIfNeeded();
    }

    private void Update()
    {
        if (isBeingKnockedBack)
        {
            zombieController?.SetMovementSpeed(0f);
            return;
        }

        if (health != null && health.IsDead)
        {
            StopAgent();
            return;
        }

        if (brainCapsule == null || brainCapsule.IsDestroyed)
        {
            StopAgent();
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (zombieController != null && zombieController.IsInAttackRange())
        {
            LockInAttack();
            FaceNexus();
            return;
        }

        UnlockAttack();

        if (Time.time < nextRepathTime)
        {
            return;
        }

        nextRepathTime = Time.time + repathInterval;
        agent.isStopped = false;
        agent.SetDestination(GetCurrentDestination());
        zombieController?.SetMovementSpeed(agent.velocity.magnitude);
    }

    public void SetTarget(BrainCapsule target)
    {
        brainCapsule = target;
    }

    public void ConfigureForWave(float speedMultiplier)
    {
        agent.speed = baseSpeed * Mathf.Max(0.1f, speedMultiplier);
    }

    public void KnockBack(Vector3 hitSource, Vector3 hitDirection, float distance, float height, float duration)
    {
        if (!agent.enabled || !agent.isOnNavMesh || distance <= 0f || duration <= 0f)
        {
            return;
        }

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        knockbackRoutine = StartCoroutine(KnockBackRoutine(hitSource, hitDirection, distance, height, duration));
    }

    private void FindTargetIfNeeded()
    {
        if (brainCapsule != null)
        {
            return;
        }

        GameObject targetObject = GameObject.FindGameObjectWithTag(capsuleTag);
        if (targetObject != null)
        {
            brainCapsule = targetObject.GetComponent<BrainCapsule>();
        }
    }

    private void PlaceOnNavMeshIfNeeded()
    {
        if (!agent.enabled || agent.isOnNavMesh)
        {
            return;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning("ZombieNavMeshMover nao encontrou NavMesh perto do ponto de spawn.", this);
        }
    }

    private Vector3 GetCurrentDestination()
    {
        if (brainCapsule == null)
        {
            return transform.position;
        }

        float distanceToNexus = Vector3.Distance(transform.position, brainCapsule.transform.position);
        if (distanceToNexus <= directApproachDistance)
        {
            return brainCapsule.transform.position;
        }

        if (Time.time >= nextRouteVariationTime || currentDestination == Vector3.zero)
        {
            currentDestination = GetVariedDestination();
            nextRouteVariationTime = Time.time + routeVariationInterval;
        }

        return currentDestination;
    }

    private Vector3 GetVariedDestination()
    {
        Vector3 center = brainCapsule.transform.position;

        if (routeVariationRadius <= 0f)
        {
            return center;
        }

        Vector2 randomOffset = Random.insideUnitCircle * routeVariationRadius;
        Vector3 destination = center + new Vector3(randomOffset.x, 0f, randomOffset.y);

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, routeVariationRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center;
    }

    private void FaceNexus()
    {
        if (!faceNexusWhileAttacking || brainCapsule == null)
        {
            return;
        }

        Vector3 direction = brainCapsule.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private IEnumerator KnockBackRoutine(Vector3 hitSource, Vector3 hitDirection, float distance, float height, float duration)
    {
        isBeingKnockedBack = true;

        Vector3 direction = hitDirection;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            direction = transform.position - hitSource;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude <= 0.01f)
        {
            direction = -transform.forward;
        }

        direction.Normalize();

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + direction * distance;
        float elapsed = 0f;
        bool agentWasEnabled = agent.enabled;

        if (agentWasEnabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float arc = Mathf.Sin(progress * Mathf.PI) * height;
            transform.position = Vector3.Lerp(startPosition, endPosition, progress) + Vector3.up * arc;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;

        if (agentWasEnabled)
        {
            agent.enabled = true;

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        isBeingKnockedBack = false;
        isLockedInAttack = false;
        nextRepathTime = 0f;
        currentDestination = Vector3.zero;
        knockbackRoutine = null;
    }

    private void StopAgent()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        zombieController?.SetMovementSpeed(0f);
    }

    private void LockInAttack()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (!isLockedInAttack)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.avoidancePriority = 0;
            isLockedInAttack = true;
        }

        agent.velocity = Vector3.zero;
        zombieController?.SetMovementSpeed(0f);
    }

    private void UnlockAttack()
    {
        if (!isLockedInAttack)
        {
            return;
        }

        if (agent.enabled)
        {
            agent.avoidancePriority = originalAvoidancePriority;
        }

        isLockedInAttack = false;
        nextRepathTime = 0f;
        currentDestination = Vector3.zero;
    }
}
