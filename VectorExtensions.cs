using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 MultVector(this Vector3 me, Vector3 you)
	{
		return new Vector3(me.x * you.x, me.y * you.y, me.z * you.z);
	}

	public static Vector3 AbsVector(this Vector3 me)
	{
		return new Vector3(Mathf.Abs(me.x), Mathf.Abs(me.y), Mathf.Abs(me.z));
	}

	public static Vector3 SignVector(this Vector3 me)
	{
		return new Vector3(Mathf.Sign(me.x), Mathf.Sign(me.y), Mathf.Sign(me.z));
	}

	public static Vector3 ClampVector(this Vector3 me, float min = 0, float max = 1)
	{
		return new Vector3(Mathf.Clamp(me.x, min, max), Mathf.Clamp(me.y, min, max), Mathf.Clamp(me.z, min, max));
	}

	public static Vector3 AlignNormal(this Vector3 normal)
	{
		Vector3 absNormal = normal.AbsVector();
		float max = Mathf.Max(Mathf.Max(absNormal.x, absNormal.y), absNormal.z);
		Vector3 alignedNormal = new Vector3((absNormal.x >= max) ? 1 : 0, (absNormal.y >= max) ? 1 : 0, (absNormal.z >= max) ? 1 : 0);
		return alignedNormal = alignedNormal.MultVector(normal.SignVector());
	}
}
