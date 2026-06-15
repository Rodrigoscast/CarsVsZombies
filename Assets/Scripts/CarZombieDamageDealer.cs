using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarZombieDamageDealer : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float minimumDamage = 15f;
    [SerializeField] private float damagePerSpeed = 2.5f;
    [SerializeField] private float minimumImpactSpeed = 3f;
    [SerializeField] private float executeHealthThreshold = 20f;
    [SerializeField] private float hitCooldown = 0.25f;

    [Header("Knockback")]
    [SerializeField] private float minKnockbackDistance = 2.5f;
    [SerializeField] private float maxKnockbackDistance = 12f;
    [SerializeField] private float maxKnockbackSpeed = 35f;
    [SerializeField] private float minKnockupHeight = 0.4f;
    [SerializeField] private float maxKnockupHeight = 3.5f;
    [SerializeField] private float knockbackDuration = 0.25f;
    [SerializeField] private bool ignorePhysicalCollisionWithZombies;
    [SerializeField] private float physicalCollisionIgnoreDuration = 0.2f;
    [SerializeField] private bool preserveCarMomentumAfterZombieHit = true;

    [Header("Target")]
    [SerializeField] private string zombieTag = "Zombie";
    [SerializeField] private bool requireZombieTag;

    private Rigidbody carRigidbody;
    private Collider[] carColliders;
    private Vector3 lastCarVelocity;
    private readonly Dictionary<Damageable, float> nextAllowedHitTimes = new Dictionary<Damageable, float>();

    private void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carColliders = GetComponentsInChildren<Collider>();
    }

    private void FixedUpdate()
    {
        lastCarVelocity = carRigidbody.linearVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryDamageZombie(collision.collider, collision.relativeVelocity.magnitude);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamageZombie(collision.collider, collision.relativeVelocity.magnitude);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamageZombie(other, carRigidbody.linearVelocity.magnitude);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamageZombie(other, carRigidbody.linearVelocity.magnitude);
    }

    private void TryDamageZombie(Collider other, float impactSpeed)
    {
        if (impactSpeed < minimumImpactSpeed)
        {
            return;
        }

        Damageable zombieHealth = other.GetComponentInParent<Damageable>();
        ZombieController zombie = other.GetComponentInParent<ZombieController>();

        if (zombieHealth == null || zombie == null)
        {
            return;
        }

        if (requireZombieTag && !MatchesZombieTag(other, zombie))
        {
            return;
        }

        if (nextAllowedHitTimes.TryGetValue(zombieHealth, out float nextAllowedHitTime) && Time.time < nextAllowedHitTime)
        {
            return;
        }

        if (ignorePhysicalCollisionWithZombies)
        {
            IgnorePhysicalCollisionWith(zombie);
        }

        float damage = minimumDamage + impactSpeed * damagePerSpeed;

        if (zombieHealth.CurrentHealth <= executeHealthThreshold)
        {
            zombieHealth.Kill();
        }
        else
        {
            zombieHealth.TakeDamage(damage);
        }

        ZombieNavMeshMover mover = zombie.GetComponent<ZombieNavMeshMover>();
        if (mover != null)
        {
            float speedPercent = Mathf.InverseLerp(minimumImpactSpeed, maxKnockbackSpeed, impactSpeed);
            float knockbackDistance = Mathf.Lerp(minKnockbackDistance, maxKnockbackDistance, speedPercent);
            float knockupHeight = Mathf.Lerp(minKnockupHeight, maxKnockupHeight, speedPercent);
            Vector3 hitDirection = carRigidbody.linearVelocity.sqrMagnitude > 0.01f ? carRigidbody.linearVelocity.normalized : transform.forward;

            mover.KnockBack(transform.position, hitDirection, knockbackDistance, knockupHeight, knockbackDuration);
        }

        PreserveCarMomentum();
        nextAllowedHitTimes[zombieHealth] = Time.time + hitCooldown;
    }

    private bool MatchesZombieTag(Collider other, ZombieController zombie)
    {
        if (string.IsNullOrEmpty(zombieTag))
        {
            return true;
        }

        return zombie.CompareTag(zombieTag) || other.CompareTag(zombieTag);
    }

    private void IgnorePhysicalCollisionWith(ZombieController zombie)
    {
        Collider[] zombieColliders = zombie.GetComponentsInChildren<Collider>();

        foreach (Collider carCollider in carColliders)
        {
            if (carCollider == null)
            {
                continue;
            }

            foreach (Collider zombieCollider in zombieColliders)
            {
                if (zombieCollider != null)
                {
                    Physics.IgnoreCollision(carCollider, zombieCollider, true);
                    StartCoroutine(RestoreCollision(carCollider, zombieCollider, physicalCollisionIgnoreDuration));
                }
            }
        }
    }

    private IEnumerator RestoreCollision(Collider carCollider, Collider zombieCollider, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (carCollider != null && zombieCollider != null)
        {
            Physics.IgnoreCollision(carCollider, zombieCollider, false);
        }
    }

    private void PreserveCarMomentum()
    {
        if (!preserveCarMomentumAfterZombieHit || lastCarVelocity.sqrMagnitude <= carRigidbody.linearVelocity.sqrMagnitude)
        {
            return;
        }

        carRigidbody.linearVelocity = lastCarVelocity;
    }
}
