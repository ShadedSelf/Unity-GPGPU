using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class RecursionPort : MonoBehaviour
{


	public GameObject otherPort;
	[Range(0, 100)]
	public int recursions;
	public LayerMask mask = -1;
	public bool mainPortal = false;
	public GameObject functionsGameObject;
	public string[] functions;

	private Camera cam { get; set; }
	private RenderTexture tex;
	private RenderTexture tempTex;
	private bool first;

	void OnEnable()
	{
		gameObject.layer = 8;
		first = true;
		if (GetComponent<MeshCollider>() != null)
		{
			GetComponent<MeshCollider>().convex = true;
			GetComponent<MeshCollider>().isTrigger = true;
		}
		tex = new RenderTexture(Screen.width, Screen.height, 24);
		tempTex = new RenderTexture(Screen.width, Screen.height, 24);
		GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("UV/UV Remap"));
		UpdateCam();
	}

	void LateUpdate()
	{
		CheckProximity();
		if (first)
		{
			first = false;
			StartCoroutine(DrawMesh());
			GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", otherPort.GetComponent<RecursionPort>().tex);
			StaticManager.pseudoMainCam.nearClipPlane = 0.001f;
		}

		if (otherPort.GetComponent<Renderer>().IsVisibleFrom(StaticManager.pseudoMainCam))
		{
			GetCamPos();
		}
	}

	private float temp { get; set; }
	void CheckProximity()
	{
		temp = Vector3.Distance(StaticManager.pseudoMainCam.transform.position, transform.position);
		if (temp < otherPort.GetComponent<RecursionPort>().temp)
			mainPortal = true;
		else
			mainPortal = false;
	}

	public bool obliqueMatrix = false;
	public float cameraOffset = 0.0f;
	public float obliquePlaneOffset = 0.01f;

	private Vector3 camPos;
	private bool resetProjectionMatrix = true;
	public void GetCamPos()
	{
		for (int i = recursions; i >= 0; i--)
		{
			camPos = otherPort.transform.InverseTransformPoint(StaticManager.pseudoMainCam.transform.position);
			camPos.x = -camPos.x;
			camPos.z = (-camPos.z + i * (Vector3.Distance(transform.localPosition, otherPort.transform.localPosition) / transform.localScale.z) - cameraOffset);
			cam.transform.localPosition = camPos;
			cam.transform.localRotation = Quaternion.AngleAxis(180.0f, new Vector3(0, 1, 0)) * Quaternion.LookRotation(otherPort.transform.InverseTransformDirection(StaticManager.pseudoMainCam.transform.forward), otherPort.transform.InverseTransformDirection(StaticManager.pseudoMainCam.transform.up));

			if (obliqueMatrix)
			{
				cam.projectionMatrix = cam.CalculateObliqueMatrix(RandomExtensions.CameraSpacePlane(cam, transform.position, transform.forward, -1, obliquePlaneOffset));
				resetProjectionMatrix = true;
			}
			else if (resetProjectionMatrix)
			{
				cam.ResetProjectionMatrix();
				resetProjectionMatrix = false;
			}
					

			if (recursions == 0)
			{
				cam.Render();
				return;
			}
			cam.targetTexture = tempTex;
			cam.Render();
			Graphics.Blit(tempTex, tex);
			cam.targetTexture = null;
		}
	}

	private float eps = 0.0010011f; //0.0100011f
	void OnTriggerStay(Collider thing)
	{
		if (transform.InverseTransformPoint(thing.transform.position).z >= -0.001001) //-0.010001
		{
			if (thing.GetComponent<RBController>() != null)
			{
				thing.GetComponent<RBController>().currentDownNormal = -otherPort.transform.up;
				thing.GetComponent<RBController>().speed *= 1 / (transform.lossyScale.y / otherPort.transform.lossyScale.y);
				thing.GetComponent<RBController>().jumpForce *= 1 / (transform.lossyScale.y / otherPort.transform.lossyScale.y);
				thing.GetComponent<RBController>().downForce *= 1 / (transform.lossyScale.y / otherPort.transform.lossyScale.y);
				thing.GetComponent<RBController>().downForceHolder *= 1 / (transform.lossyScale.y / otherPort.transform.lossyScale.y);
				var tmp = thing.transform;
				GetPos(ref tmp, eps);
			}
			if (thing.GetComponent<GravityModifier>() != null && transform.InverseTransformPoint(thing.transform.position).z >= -Mathf.Epsilon)
			{
				thing.GetComponent<GravityModifier>().gravityNormal = -otherPort.transform.up;
				thing.GetComponent<GravityModifier>().gravity *= 1 / (transform.lossyScale.y / otherPort.transform.lossyScale.y);
				var tmp = thing.transform;
				GetPos(ref tmp, Mathf.Epsilon);
			}



			if (functionsGameObject != null)
			{
				DoFunctions();
			}
		}
	}

	void GetPos(ref Transform relativePos, float epsislon)
	{
		Vector3 tempPos = transform.InverseTransformPoint(relativePos.position);
		tempPos.x = -tempPos.x;
		tempPos.z = -epsislon;
		relativePos.position = otherPort.transform.TransformPoint(tempPos);
		StaticManager.former.transform.parent = otherPort.transform;
		StaticManager.former.transform.localRotation = Quaternion.AngleAxis(180.0f, new Vector3(0, 1, 0)) * Quaternion.LookRotation(transform.InverseTransformDirection(relativePos.forward), transform.InverseTransformDirection(relativePos.up));
		relativePos.transform.rotation = StaticManager.former.transform.rotation;
		relativePos.localScale = relativePos.lossyScale * 1 / (transform.lossyScale.y / otherPort.transform.lossyScale.y);

		StaticManager.former.transform.forward = relativePos.gameObject.GetComponent<Rigidbody>().velocity.normalized;
		StaticManager.former.transform.localRotation = Quaternion.AngleAxis(180.0f, new Vector3(0, 1, 0)) * Quaternion.LookRotation(transform.InverseTransformDirection(StaticManager.former.transform.forward), transform.InverseTransformDirection(StaticManager.former.transform.up));
		relativePos.gameObject.GetComponent<Rigidbody>().velocity = StaticManager.former.transform.forward * relativePos.gameObject.GetComponent<Rigidbody>().velocity.magnitude * 1 / (transform.lossyScale.y / otherPort.transform.lossyScale.y);
	}

	IEnumerator DrawMesh()
	{
		while (true)
		{
			Vector3 tempPos = transform.InverseTransformPoint(StaticManager.portalCube.transform.position);
			tempPos.x = -tempPos.x;
			tempPos.z = -tempPos.z;
			tempPos = otherPort.transform.TransformPoint(tempPos);
			StaticManager.former.transform.parent = otherPort.transform;
			StaticManager.former.transform.localRotation = Quaternion.AngleAxis(180.0f, new Vector3(0, 1, 0)) * Quaternion.LookRotation(transform.InverseTransformDirection(StaticManager.portalCube.transform.forward), transform.InverseTransformDirection(StaticManager.portalCube.transform.up));
			Matrix4x4 matrix = Matrix4x4.TRS(tempPos, StaticManager.former.transform.rotation, StaticManager.portalCube.transform.lossyScale * 1 / (transform.lossyScale.y / otherPort.transform.lossyScale.y));
			Graphics.DrawMesh(StaticManager.portalCube.GetComponent<MeshFilter>().mesh, matrix, new Material(Shader.Find("Standard")), 0);
			yield return null;
		}
	}

	void DoFunctions()
	{
		if (functions.Length > 0 && mainPortal == true)
		{
			foreach (var func in functions)
			{
				functionsGameObject.SendMessage(func);
			}
		}
		otherPort.GetComponent<RecursionPort>().mainPortal = true;
		mainPortal = false;
	}

	public void UpdateCam()
	{
		if (first)
		{
			var camObject = new GameObject("cam");
			camObject.transform.parent = transform;
			cam = camObject.AddComponent<Camera>();
			cam.enabled = false;
			cam.targetTexture = tex;
		}
		cam.nearClipPlane = StaticManager.pseudoMainCam.nearClipPlane;
		cam.farClipPlane = StaticManager.pseudoMainCam.farClipPlane;
		cam.fieldOfView = StaticManager.pseudoMainCam.fieldOfView;
		cam.backgroundColor = StaticManager.pseudoMainCam.backgroundColor;
		cam.cullingMask = mask;
	}

	void OnDisable()
	{
		Destroy(cam);
		tex.Release();
		tempTex.Release();
	}
}