using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FRAC : MonoBehaviour
{
	[Header("Shit")]
	public RenderTexture RT;
	public ComputeShader CS;
	public int texSize = 1024;
	[Header("Shittings")]
	public int maxSteps = 256;
	public float drawDist = 100;
	public float epsi = .000001f;
	[Header("Plane")]
	public float fwMult = 1;
	public float projectionMult = 2;
	[Header("Frac")]
	public float scale;
	public Transform preRotVec;
	public float preRotAngle;
	public Transform postRotVec;
	public float postRotAngle;
	public Vector3 offset;
	public float sphereRadius;
	public int iterations;
	public int colIterations;
	[Header("Random")]
	public Vector3 r0;
	public Light li;

	int deframe;
	void OnEnable()
	{
		texSize = Mathf.Max(Mathf.NextPowerOfTwo(texSize), 32);
		RT = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		RT.enableRandomWrite = true;
		RT.Create();

		CS.SetTexture(0, "Result", RT);

		deframe = Time.frameCount;
	}

	Vector3 eye = new Vector3(0, 0, -1);
	Vector3 up = new Vector3(0, 1, 0);
	Vector3 right = new Vector3(1, 0, 0);
	Vector3 fw = new Vector3(0, 0, 1);
	void Update()
	{
		if (Camera.current != Camera.main && Camera.current != null)
		{
			if (eye != Camera.current.transform.position)
			{
				deframe = Time.frameCount;
			}
			eye = Camera.current.transform.position;
			up = Camera.current.transform.up;
			right = Camera.current.transform.right;
			fw = Camera.current.transform.forward;
		}
		CS.SetVector("eye", eye);
		CS.SetVector("up", up);
		CS.SetVector("right", right);
		CS.SetVector("fw", fw);

		CS.SetVector("_LightDir", li.transform.forward.normalized);

		CS.SetVector("_PreRotVec", preRotVec.transform.forward);
		CS.SetVector("_PostRotVec", postRotVec.transform.forward);
		CS.SetVector("_OffSet", offset);

		CS.SetInt("maxSteps", maxSteps);
		CS.SetFloat("drawDist", drawDist);
		CS.SetFloat("epSI", epsi);

		CS.SetFloat("fwMult", fwMult);
		CS.SetFloat("projectionMult", projectionMult);

		CS.SetFloat("_Scale", scale);
		CS.SetFloat("_PreRotAngle", preRotAngle);
		CS.SetFloat("_PostRotAngle", postRotAngle);
		CS.SetFloat("_Rad", sphereRadius);

		CS.SetInt("texSize", texSize);
		CS.SetInt("_Iterations", iterations);
		CS.SetInt("_ColorIterations", colIterations);

		CS.SetFloat("_Time", Time.time);
		CS.SetInt("_Frame", Time.frameCount - deframe);

		CS.SetVector("_R0", r0);

		Folds();

		CS.Dispatch(0, texSize / 32, texSize / 32, 1);
	}

	void Folds()
	{
		Vector4[] f = new Vector4[6];

		float PHI = 1.618033988749895f;

		Vector3 n1 = (new Vector3(-PHI, PHI - 1.0f, 1.0f)).normalized;
		Vector3 n2 = (new Vector3(1.0f, -PHI, PHI + 1.0f)).normalized;

		f[0] = new Vector4(1, 1, 1, 1);
		f[1] = new Vector4(n2.x, n2.y, n2.z, 0);
		f[2] = new Vector4(1, 0, 1, 1);
		f[3] = new Vector4(n1.x, n1.y, n1.z, 0);
		f[4] = new Vector4(n2.x, n2.y, n2.z, 0);
		f[5] = new Vector4(0, 0, 1, 1);

		CS.SetVectorArray("afolds", f);
		CS.SetInt("numFolds", 6);
	}

	void OnGUI()
	{
		GUI.DrawTexture(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)), RT, ScaleMode.ScaleAndCrop, false);
	}
}
