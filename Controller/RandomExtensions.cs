using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class RandomExtensions
{
	public static Vector3[] sixNormals = new Vector3[6] { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };


	public static bool BetwenTheLine(this float value)
	{
		if (value >= 0 && value <= 1)
			return true;
		else
			return false;
	}
	
	public static Vector3 LerpezaiseScreenPos(this Vector3 pos)
	{
		pos.x = pos.x / Screen.width;
		pos.y = pos.y / Screen.height;
		return pos;
	}

	public static Vector3 UnLerpezaiseScreenPos(this Vector3 pos)
	{
		pos.x = Mathf.Lerp(0, Screen.width, pos.x);
		pos.y = Mathf.Lerp(0, Screen.height, pos.y);
		return pos;
	}

	public static Vector3 CubesToOne(float x, float y, float z, int gridSize, float scaleMult = 1)
	{
		return new Vector3((x / gridSize - .5f + (.5f / gridSize)) * scaleMult, (y / gridSize - .5f + (.5f / gridSize)) * scaleMult, (z / gridSize - .5f + (.5f / gridSize)) * scaleMult);
	}

	public static bool AlmostFloat(this float number1, float number2, float precision = 0.0001f)
	{
		return Mathf.Abs(number1 - number2) < precision;
	}

	public static bool AlmostVector(this Vector3 v1, Vector3 v2, float precision = 0.0001f)
	{
		if (v1.x.AlmostFloat(v2.x, precision) && v1.y.AlmostFloat(v2.y, precision) && v1.z.AlmostFloat(v2.z, precision))
			return true;
		else
			return false;
	}

	public static IEnumerator ValueTo(Action<float> value, float curr, float target, float speed)
	{
		while (!curr.AlmostFloat(target, 0.001f))
		{
			curr = Mathf.MoveTowards(curr, target, speed * Time.deltaTime);
			value(curr);
			yield return null;
		}
		value(target);
	}

	public static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign, float offSet)
	{
		Vector3 offsetPos = pos + normal * offSet;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint(offsetPos);
		Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
	}

	public static float NormalizeFloat(this float value, float max, float min)
	{
		return (value - min) / (max - min);
	}

	public static int Rounder(this int value, int min, int max)
	{
		if (value > max)
			return min + value - max - 1;
		return value;
	}
}
