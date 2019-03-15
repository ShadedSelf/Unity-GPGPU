using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityModifier : MonoBehaviour {


	public float gravity;
	public Vector3 gravityNormal;
	public bool setCurrentGravity = false;
	public bool restoreGravity = false;

	private Rigidbody rb;
	private float defeaultGravity;

	void Start ()
	{
		gravity = -Physics.gravity.y;
		defeaultGravity = gravity;
		gravityNormal = Vector3.down;
		rb = GetComponent<Rigidbody>();
		rb.useGravity = false;
	}

	void Update()
	{
		if (setCurrentGravity)
		{
			setCurrentGravity = false;
			Physics.gravity = gravityNormal * gravity;
		}

		if (restoreGravity)
		{
			restoreGravity = false;
			Physics.gravity = gravityNormal * defeaultGravity;
		}
	}

	void FixedUpdate ()
	{
		rb.AddForce(gravityNormal * gravity, ForceMode.Acceleration);
	}
}
