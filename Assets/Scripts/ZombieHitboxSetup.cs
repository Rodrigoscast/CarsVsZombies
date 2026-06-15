using UnityEngine;

[DisallowMultipleComponent]
public class ZombieHitboxSetup : MonoBehaviour
{
    [Header("Car Hit Detection")]
    [SerializeField] private bool makeCollidersTrigger = true;
    [SerializeField] private bool makeRigidbodyKinematic = true;

    private void Awake()
    {
        if (makeRigidbodyKinematic)
        {
            Rigidbody zombieRigidbody = GetComponent<Rigidbody>();
            if (zombieRigidbody != null)
            {
                zombieRigidbody.isKinematic = true;
                zombieRigidbody.useGravity = false;
            }
        }

        if (!makeCollidersTrigger)
        {
            return;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider zombieCollider in colliders)
        {
            zombieCollider.isTrigger = true;
        }
    }
}
