using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform carTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;
	Vector3 initialCameraPosition;
	Vector3 initialCarPosition;
	Vector3 absoluteInitCameraPosition;

	void Start(){
		initialCameraPosition = gameObject.transform.position;
		initialCarPosition = carTransform.position;
		absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
	}

	void FixedUpdate()
	{
		// Posição desejada baseada na rotação atual do carro
		Vector3 targetPos =
			carTransform.position +
			carTransform.rotation * absoluteInitCameraPosition;

		transform.position = Vector3.Lerp(
			transform.position,
			targetPos,
			followSpeed * Time.deltaTime
		);

		// Olhar para o carro
		Vector3 lookPoint = carTransform.position + Vector3.up * 1.5f;

		Quaternion targetRotation = Quaternion.LookRotation(
			lookPoint - transform.position
		);

		transform.rotation = Quaternion.Lerp(
			transform.rotation,
			targetRotation,
			lookSpeed * Time.deltaTime
		);
	}

}
