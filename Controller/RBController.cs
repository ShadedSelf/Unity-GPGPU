using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class RBController : MonoBehaviour
{

	public MouseLooker mouseLook;
	public Transform pivot;
	public LayerMask mask;
	public int maxJumps = 1;
	public bool grounded = false;
	public bool walled = false;
	public bool stucked = false;

	private int currJumps;
	private Camera cam;
	private Rigidbody rb;
	void Start()
	{
		currJumps = maxJumps;
		cam = GetComponentInChildren<Camera>();
		InitThing();
		rb = GetComponent<Rigidbody>();
		downForceHolder = downForce;
	}

	[HideInInspector] public Vector3 currentDownNormal = Vector3.down;
	private Vector3 moveDirection;
	void Update()
	{
		moveDirection = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
			moveDirection -= Vector3.Cross(cam.transform.right, currentDownNormal);
		if (Input.GetKey(KeyCode.S))
			moveDirection += Vector3.Cross(cam.transform.right, currentDownNormal);
		if (Input.GetKey(KeyCode.D))
			moveDirection += cam.transform.right;
		if (Input.GetKey(KeyCode.A))
			moveDirection -= cam.transform.right;
		if (Input.GetKeyDown(KeyCode.Space) && currJumps > 0)
		{
			t = 0;
			downForce = downForceHolder;
			currJumps -= 1;
		}

		if (Input.GetKey(KeyCode.LeftShift) && walled)
			stucked = true;
		else
			stucked = false;
	
		if (Physics.BoxCast(transform.position, new Vector3(.499f, .0001f, .499f) * transform.localScale.y, -transform.up, Quaternion.identity, 1f * transform.localScale.y, mask))
		{
			if (!grounded)
			{
				currJumps = maxJumps;
			}
			grounded = true;
		}
		else
			grounded = false;

		for (int i = 0; i < 4; i++)
		{
			if (Physics.BoxCast(transform.position, new Vector3(.499f, .999f, .0001f) * transform.localScale.y, RandomExtensions.sixNormals[2 + i], Quaternion.LookRotation(RandomExtensions.sixNormals[2 + i], transform.up), .5f * transform.localScale.y, mask))
			{
				walled = true;
				break;
			}
			walled = false;
		}
	}

	public float speed = 12;
	public float jumpForce = 10;
	public float downForce = 10;
	[HideInInspector]public float downForceHolder;
	private float up = 0;
	private float t = 1;
	void FixedUpdate()
	{
		mouseLook.LookRotation(pivot, cam.transform);

		if (t < 1)
		{
			up = Mathf.Lerp(-downForce - jumpForce, 0, t);
			t += 1 * Time.deltaTime;
		}
		if (t >= 1 && !grounded && downForce < downForceHolder * 4 && !stucked)
			downForce *= 1.02f;
		else if ((grounded || stucked) && downForce != downForceHolder)
			downForce = downForceHolder;

		float curr = downForce + up;
		rb.velocity = moveDirection.normalized * speed;
		if (!stucked)
			rb.velocity += currentDownNormal * curr;
	}

	public void InitThing()
	{
		mouseLook.Init(pivot, cam.transform);
	}
}
