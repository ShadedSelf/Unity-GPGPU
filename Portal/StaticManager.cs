using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticManager
{

	public static Camera pseudoMainCam;
	public static Weapon weapon;
	public static Vector3[] roomNormals = new Vector3[6] { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
	public static GameObject portalCube;
	public static GameObject former;

	public static void SetLayerRecursively(this GameObject go, int layerNumber)
	{
		foreach (var trans in go.GetComponentsInChildren<Transform>(true))
			trans.gameObject.layer = layerNumber;
	}

}
