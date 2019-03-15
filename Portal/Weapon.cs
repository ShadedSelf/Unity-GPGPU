using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

	public GameObject mainHolder;
	public GameObject noiseCubeHolder;
	public GameObject[] noiseQuads;
	public GameObject[] cornerCubes;

	public Vector3 screenPosition = new Vector3(.875f, .125f, 4f);
	public bool stickScreenPosition = false;

	void LateUpdate()
	{
		if (stickScreenPosition)
			transform.position = StaticManager.pseudoMainCam.ScreenToWorldPoint(screenPosition.UnLerpezaiseScreenPos());
	}

	private Coroutine screenPositionCoroutine = null;
	public void SetScreenPos(Vector3 screenPositionToSet, float speed = 1)
	{
		if (screenPositionCoroutine != null)
			StopCoroutine(screenPositionCoroutine);
		screenPositionCoroutine = StartCoroutine(ScreenPositioner(screenPositionToSet, speed));
	}

	private Coroutine positionCoroutine = null;
	public void SetPosition(Vector3 positionToSet, float speed)
	{
		if (positionCoroutine != null)
			StopCoroutine(positionCoroutine);
		positionCoroutine = StartCoroutine(Positioner(positionToSet, speed));
	}

	private Coroutine scaleCoroutine = null;
	public void SetScale(Vector3 scaleToSet, float speed = 1)
	{
		if (scaleCoroutine != null)
			StopCoroutine(scaleCoroutine);
		scaleCoroutine = StartCoroutine(Scaler(scaleToSet, speed));
	}

	IEnumerator ScreenPositioner(Vector3 screenPositionToSet, float speed)
	{
		float t = 0;
		Vector3 curr = screenPosition;
		Vector3 middle;
		while (t <= 1)
		{
			t += speed * Time.deltaTime;
			middle.x = Mathf.SmoothStep(curr.x, screenPositionToSet.x, t);
			middle.y = Mathf.SmoothStep(curr.y, screenPositionToSet.y, t);
			middle.z = Mathf.SmoothStep(curr.z, screenPositionToSet.z, t);
			screenPosition = middle;
			yield return null;
		}
		screenPosition = screenPositionToSet;
	}

	IEnumerator Positioner(Vector3 positionToSet, float speed)
	{
		float t = 0;
		Vector3 curr = transform.position;
		Vector3 middle;
		while (t <= 1)
		{
			t += speed * Time.deltaTime;
			middle.x = Mathf.SmoothStep(curr.x, positionToSet.x, t);
			middle.y = Mathf.SmoothStep(curr.y, positionToSet.y, t);
			middle.z = Mathf.SmoothStep(curr.z, positionToSet.z, t);
			transform.position = middle;
			yield return null;
		}
		transform.position = positionToSet;
	}

	IEnumerator Scaler(Vector3 scaleToSet, float speed = 1f)
	{
		float t = 0;
		Vector3 curr = noiseCubeHolder.transform.localScale;
		Vector3 middle;
		while (t <= 1)
		{
			t += speed * Time.deltaTime;
			middle.x = Mathf.SmoothStep(curr.x, scaleToSet.x, t);
			middle.y = Mathf.SmoothStep(curr.y, scaleToSet.y, t);
			middle.z = Mathf.SmoothStep(curr.z, scaleToSet.z, t);
			noiseCubeHolder.transform.localScale = middle;
			CornerPos();
			yield return null;
		}
		noiseCubeHolder.transform.localScale = scaleToSet;
		CornerPos();
	}

	void CornerPos()
	{
		int j = 0;
		for (int x = 0; x < 2; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				for (int z = 0; z < 2; z++)
				{
					cornerCubes[j].transform.localPosition = new Vector3(x * noiseCubeHolder.transform.localScale.x - (noiseCubeHolder.transform.localScale.x / 2), y * noiseCubeHolder.transform.localScale.y - (noiseCubeHolder.transform.localScale.y / 2), z * noiseCubeHolder.transform.localScale.z - (noiseCubeHolder.transform.localScale.z / 2));
					j++;
				}
			}
		}
	}
}
