using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public Camera portalCamera;
	public GameObject portalCube;

	void Awake ()
	{
		StaticManager.pseudoMainCam = portalCamera;
		StaticManager.portalCube = portalCube;
		StaticManager.former = new GameObject("Former");
	}
	
}
