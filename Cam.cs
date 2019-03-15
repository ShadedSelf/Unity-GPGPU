using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Cam : MonoBehaviour
{
	public Camera cam;
	public Camera cim;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (Camera.current != null && cam == null)
		{
			cam = Camera.current;
		}
		else
		{
			cim.transform.position = cam.transform.position;
			cim.transform.rotation = cam.transform.rotation;
		}
	}
}
