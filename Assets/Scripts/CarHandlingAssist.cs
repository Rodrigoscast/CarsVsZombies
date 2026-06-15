using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarHandlingAssist : MonoBehaviour
{
    [Header("Grip")]
    [SerializeField] private float lateralGrip = 4f;
    [SerializeField] private float maxLateralGripSpeed = 45f;

    [Header("Stability")]
    [SerializeField] private float downforce = 45f;
    [SerializeField] private float angularStability = 2.5f;
    [SerializeField] private float maxAngularVelocity = 2.5f;

    [Header("Body")]
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.35f, 0f);

    private Rigidbody carRigidbody;

    private void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        carRigidbody.maxAngularVelocity = maxAngularVelocity;
        StartCoroutine(ApplyCenterOfMassAfterCarSetup());
    }

    private void FixedUpdate()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(carRigidbody.linearVelocity);
        float speed = carRigidbody.linearVelocity.magnitude;

        float gripPercent = Mathf.InverseLerp(0f, maxLateralGripSpeed, speed);
        localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, lateralGrip * gripPercent * Time.fixedDeltaTime);
        carRigidbody.linearVelocity = transform.TransformDirection(localVelocity);

        carRigidbody.AddForce(-transform.up * downforce * speed, ForceMode.Force);

        Vector3 localAngularVelocity = transform.InverseTransformDirection(carRigidbody.angularVelocity);
        localAngularVelocity.x = Mathf.Lerp(localAngularVelocity.x, 0f, angularStability * Time.fixedDeltaTime);
        localAngularVelocity.z = Mathf.Lerp(localAngularVelocity.z, 0f, angularStability * Time.fixedDeltaTime);
        carRigidbody.angularVelocity = transform.TransformDirection(localAngularVelocity);
    }

    private IEnumerator ApplyCenterOfMassAfterCarSetup()
    {
        yield return null;
        carRigidbody.centerOfMass += centerOfMassOffset;
    }
}
