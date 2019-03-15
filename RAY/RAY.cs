using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RAY : MonoBehaviour
{
    public Vector2 angle;
    [Header("Shit")]
	public RenderTexture RT;
	public ComputeShader CS;
	public Material mat;
	public int texSize = 1024;
	[Header("Shittings")]
	public int maxSteps = 256;
	public float drawDist = 100;
	public float epsi = .000001f;
	[Header("Plane")]
	public float fwMult = 1;
	public float projectionMult = 2;
	[Header("Shadows")]
	[Range(-1, 1)]
	public float _L;
	public float minT = .01f;
	public float maxT = 50;
	public float kk = 32;
	public Transform li;
    [Header("Me")]
    public bool set = false;
    public Params par;
    public Texture2D noise;

    [System.Serializable]
    public struct Params
    {
        public Vector3 pos;
        public Vector3 size;
        public Vector3 cross;
        public float hole;
    }

    void OnEnable()
	{
		texSize = Mathf.Max(Mathf.NextPowerOfTwo(texSize), 32);
		RT = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		RT.enableRandomWrite = true;
		RT.Create();

		CS.SetTexture(0, "Result", RT);
        CS.SetTexture(0, "Noise", noise);
		mat.SetTexture("_MainTex", RT);
	}

	Vector3 eye = new Vector3(0, 0, -1);
	Vector3 up = new Vector3(0, 1, 0);
	Vector3 right = new Vector3(1, 0, 0);
	Vector3 fw = new Vector3(0, 0, 1);
    int deframe = 0;
	void Update()
	{
		if (Camera.current != Camera.main && Camera.current != null)
		{
			eye = Camera.current.transform.position;
			up = Camera.current.transform.up;
			right = Camera.current.transform.right;
			fw = Camera.current.transform.forward;
		}
        if (set)
        {
            //set = false;
            deframe = Time.frameCount;
        }
            CS.SetVector("eye", eye);
		CS.SetVector("up", up);
		CS.SetVector("right", right);
		CS.SetVector("fw", fw);

		CS.SetVector("angle", angle);
		CS.SetVector("_LightDir", li.transform.forward.normalized);
		CS.SetVector("_SelfDir", transform.forward.normalized);

		CS.SetInt("maxSteps", maxSteps);
		CS.SetFloat("drawDist", drawDist);
		CS.SetFloat("epSI", epsi);

		CS.SetFloat("fwMult", fwMult);
		CS.SetFloat("projectionMult", projectionMult);

		CS.SetFloat("_L", _L);
		CS.SetFloat("minT", minT);
		CS.SetFloat("maxT", maxT);
		CS.SetFloat("kk", kk);

		CS.SetInt("texSize", texSize);

        CS.SetFloat("_Time", Time.time);
        CS.SetInt("_Frame", Time.frameCount - deframe);

        CS.SetVector("_Pos", par.pos);
        CS.SetVector("_Size", par.size);
        CS.SetFloat("_HolePC", par.hole);
        CS.SetVector("_CrossPC", par.cross);

        CS.SetFloat("_BoxPC", 100);

        cu = pointWarp(time, Time.time / 5f, kk, 2.5f);
        div = Vector3.Dot(cu.normalized, li.transform.forward);
        time += div * .1f;
        CS.SetVector("pWarp", cu);
        Turn();

        CS.Dispatch(0, texSize / 32, texSize / 32, 1);

        /*if(set)
        {
            set = false;
            StartCoroutine(Set(new Vector3((Random.value - .5f) * 5, (Random.value - .5f) * 5, (Random.value - .5f) * 5), 
                new Vector3(Random.value * 2, Random.value * 2, Random.value * 2) + Vector3.one * .1f));
        }*/
	}
    Vector3 cu;
    float time;
    float div;


    Vector3 pos;
    Vector3 p;
    Vector3 vel;
    int loops;
    void Turn()
    {
        vel = (p - pos) / Time.deltaTime;
        vel += li.forward * 1 * Time.deltaTime;
        pos = p;
        p = pos + vel * Time.deltaTime;

        Vector3 grad = p;
        Vector3 dp = grad.normalized * ((grad.magnitude) - 3.5f);
        dp += (grad - (li.forward * 3.5f)) / 20f;
        p -= dp;

        float prevT = time;
        time = Mathf.Atan2(pos.z, pos.x);
        if ((prevT > 1 && time < -1) || (prevT < -1 && time > 1))
            loops -= (int)Mathf.Sign(time);

        cu = pointWarp(time + (Mathf.PI * loops * 2), 0, kk, 2.5f);
        CS.SetVector("pWarp", cu);
        //CS.SetFloat("warp",)
    }

    Vector3 posDelta;
    Vector3 sizeDelta;
    IEnumerator Set(Vector3 pos, Vector3 size)
    {
        par.pos = Vector3.SmoothDamp(par.pos, pos, ref posDelta, .1f);
        par.pos = Vector3.SmoothDamp(par.pos, pos, ref posDelta, .1f); par.size = Vector3.SmoothDamp(par.size, size, ref sizeDelta, .1f);

        par.hole = Mathf.Max((size.x + size.y + size.z) / 3 * .25f, .25f);

        size = new Vector3(Mathf.Max(size.x, par.hole * 2), Mathf.Max(size.y, par.hole * 2), Mathf.Max(size.z, par.hole * 2));

        while (posDelta.magnitude > .001f && sizeDelta.magnitude > .001f)
        {
            par.pos = Vector3.SmoothDamp(par.pos, pos, ref posDelta, .1f);
            par.size = Vector3.SmoothDamp(par.size, size, ref sizeDelta, .1f);
            par.cross = /*par.size **/Vector3.one * .1f;

            yield return null;
        }
    }

    Vector3 pointWarp(float angle, float inWarp, float reps, float radius)
    {
        Vector3 roter = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;
        Vector3 pointer = Vector3.up;
        Vector3 cp = Vector3.Cross(roter, pointer);
        pointer = rot(cp, inWarp + angle * reps).MultiplyPoint3x4(pointer);
        return (pointer + roter * radius);
    }

    Matrix4x4 rot(Vector3 axis, float angle)
    {
        axis = axis.normalized;
        float s = Mathf.Sin(angle);
        float c = Mathf.Cos(angle);
        float oc = 1f - c;

        Matrix4x4 a = new Matrix4x4(
            new Vector4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0),
            new Vector4(oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0),
            new Vector4(oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0),
            Vector4.zero
            );
        return new Matrix4x4(a.GetRow(0), a.GetRow(1), a.GetRow(2), a.GetRow(3));
    }

    void OnGUI()
	{
		GUI.DrawTexture(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height)), RT,ScaleMode.ScaleAndCrop, false);
	}
}
