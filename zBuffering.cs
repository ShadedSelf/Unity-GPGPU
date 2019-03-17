using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class zBuffering : MonoBehaviour
{

	[Header("^^")]
	public int particleCount = 100000;
	public int iterations = 1;
	public float radius = .1f;
	[Header("Compute Shaders")]
	public ComputeShader particleCS;
	public ComputeShader sortCS;
	[Header("Drawing")]
	public Mesh instanceMesh;
	public Material instanceMaterial;
	[Header("World")]
	[Range(1, 4)]
	public float dTMult = 3;
	public float maxVelocity = .5f;
	public int gridSize = 5;
	public Vector3 worldSize;
	public Vector3 gravity;
	[Header("Parts")]
	public float rad = .3f;
	public float lambad = 1;
	public float rest = 100;
	public float mK = .001f;
	public float mQ = .1f;
	public float mE = 4;
	[Header("Colliders")]
	public GameObject[] cubes;

	ComputeBuffer collisionBuffer, gridBuffer, argsBuffer, cubesBuffer, neisBuffer, pos, p, tmp;
	CommandBuffer commandBuffer;
	uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
	int particleKernel, sortKernel, limitKernel, clearKernel, collisionKernel, findKernel, lambKernel;
	Sort sorter;

    [System.Serializable]
    private struct CollisionData
	{
		public uint cell;
		public uint particleID;
	}

    [System.Serializable]
    public struct Grid
	{
		public uint start;
		public uint end;
	}

	private struct Cubes
	{
		public Vector3 position;
		public Vector3 scale;
		public Vector3 velocity;
	}

	public struct Nei
	{
		public uint num;
		public uint[] neis;
	}

    void OnEnable()
	{
		//gridSize = (int)(worldSize.x / radius);

		particleCount = Mathf.ClosestPowerOfTwo(particleCount);
		particleKernel = particleCS.FindKernel("Particler");
		limitKernel = particleCS.FindKernel("TrueLimits");
		clearKernel = particleCS.FindKernel("Clear");
		collisionKernel = particleCS.FindKernel("Collision");
		findKernel = particleCS.FindKernel("Find");
		// lambKernel = particleCS.FindKernel("Lamb");
		lambad = 5;
		sortKernel = sortCS.FindKernel("Sort");

		CreateBuffers();

		sorter = new Sort();
		sorter.compute = sortCS;
		sorter.count = particleCount;
		sorter.Set();

		SetCommands();
	}

	Vector3 internalVelocity;
	Vector3 lastPos;
	void Update()
	{
		particleCS.SetFloat("deltaTime", Time.deltaTime * dTMult / (float)iterations);
		particleCS.SetFloat("time", Time.time);
		particleCS.SetFloat("particleRad", rad);
		particleCS.SetFloat("rest", rest);
		particleCS.SetFloat("maxVelocity", maxVelocity);
		particleCS.SetFloat("radius", radius);
		particleCS.SetFloat("mass", lambad);
		particleCS.SetFloat("mQ", mQ);
		particleCS.SetFloat("mK", mK);
		particleCS.SetFloat("mE", mE);

		particleCS.SetInt("gridSize", Mathf.Abs(gridSize));
		particleCS.SetInt("particleCount", particleCount);
        particleCS.SetInts("gridSize", new int[3] { gridSize, gridSize, gridSize });

        particleCS.SetVector("gravity", gravity);
		particleCS.SetVector("worldSize", worldSize);

		instanceMaterial.SetVector("_WorldPos", new Vector4(transform.position.x, transform.position.y, transform.position.z));
		instanceMaterial.SetFloat("radius", radius);

		DynamicBuffers();
		Graphics.ExecuteCommandBuffer(commandBuffer);
		Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    Vector3[] cubesTemp;
	void DynamicBuffers()
	{
		particleCS.SetMatrix("rot", Matrix4x4.Rotate(cubes[0].transform.rotation).inverse);
		if (cubes.Length > 0)
		{
			Cubes[] cubesData = new Cubes[cubes.Length];
			for (int i = 0; i < cubes.Length; i++)
			{
				cubesData[i].velocity = cubes[i].transform.position - cubesTemp[i];
				cubesData[i].position = cubes[i].transform.position - transform.position;
				cubesData[i].scale = cubes[i].transform.lossyScale / 2;
				cubesTemp[i] = cubes[i].transform.position;
			}
			cubesBuffer.SetData(cubesData);
		}
	}

	void SetCommands()
	{
		commandBuffer = new CommandBuffer();
		commandBuffer.name = "Particle System";

		commandBuffer.BeginSample("Particle");
		commandBuffer.DispatchCompute(particleCS, particleKernel, particleCount / 32, 1, 1);
		commandBuffer.EndSample("Particle");

		sorter.SetSortBuffer(commandBuffer);

        commandBuffer.BeginSample("Limit");
		commandBuffer.DispatchCompute(particleCS, limitKernel, particleCount / 32, 1, 1);
		commandBuffer.EndSample("Limit");

		commandBuffer.BeginSample("Find");
		commandBuffer.DispatchCompute(particleCS, findKernel, particleCount / 32, 1, 1);
		commandBuffer.EndSample("Find");

		for (int i = 0; i < iterations; i++)
		{
			commandBuffer.BeginSample("Lamb");
			commandBuffer.DispatchCompute(particleCS, lambKernel, particleCount / 32, 1, 1);
			commandBuffer.EndSample("Lamb");

			commandBuffer.BeginSample("Collision");
			commandBuffer.DispatchCompute(particleCS, collisionKernel, particleCount / 32, 1, 1);
			commandBuffer.EndSample("Collision");

			commandBuffer.BeginSample("Test");
        	commandBuffer.DispatchCompute(particleCS, 6, particleCount / 32, 1, 1);
        	commandBuffer.EndSample("Test");
		}

		commandBuffer.BeginSample("Clear");
		commandBuffer.DispatchCompute(particleCS, clearKernel, Mathf.NextPowerOfTwo(gridSize * gridSize * gridSize) / 128, 1, 1);
		commandBuffer.EndSample("Clear");
	}

	void CreateBuffers()
	{
		argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
		uint numIndices = instanceMesh.GetIndexCount(0);
		args[0] = numIndices;
		args[1] = (uint)particleCount;
		argsBuffer.SetData(args);

        pos = new ComputeBuffer(particleCount, sizeof(float) * 4);
        Vector4[] particles = new Vector4[particleCount];
        for (int i = 0; i < particleCount; i++)

            particles[i] = Random.insideUnitSphere * worldSize.x;

        pos.SetData(particles);
        particleCS.SetBuffer(particleKernel, "pos", pos);
        particleCS.SetBuffer(6, "pos", pos);
        instanceMaterial.SetBuffer("pos", pos);

        p = new ComputeBuffer(particleCount, sizeof(float) * 4);
        p.SetData(particles);
        particleCS.SetBuffer(particleKernel, "p", p);
        particleCS.SetBuffer(collisionKernel, "p", p);
        particleCS.SetBuffer(findKernel, "p", p);
        particleCS.SetBuffer(lambKernel, "p", p);
        particleCS.SetBuffer(6, "p", p);
        instanceMaterial.SetBuffer("p", p);

        tmp = new ComputeBuffer(particleCount, sizeof(float) * 4);
        tmp.SetData(particles);
        particleCS.SetBuffer(particleKernel, "tmp", tmp);
        particleCS.SetBuffer(collisionKernel, "tmp", tmp);
        particleCS.SetBuffer(findKernel, "tmp", tmp);
        particleCS.SetBuffer(lambKernel, "tmp", tmp);
        particleCS.SetBuffer(6, "tmp", tmp);
        instanceMaterial.SetBuffer("tmp", tmp);





        collisionBuffer = new ComputeBuffer(particleCount, sizeof(uint) * 2);
		CollisionData[] colls = new CollisionData[particleCount];
		for (int i = 0; i < particleCount; i++)
			colls[i].particleID = (uint)i;
		collisionBuffer.SetData(colls);
		particleCS.SetBuffer(particleKernel, "collisionBuffer", collisionBuffer);
		particleCS.SetBuffer(limitKernel, "collisionBuffer", collisionBuffer);
		particleCS.SetBuffer(collisionKernel, "collisionBuffer", collisionBuffer);
		particleCS.SetBuffer(findKernel, "collisionBuffer", collisionBuffer);
        particleCS.SetBuffer(6, "collisionBuffer", collisionBuffer);
        particleCS.SetBuffer(6, "collisionBuffer", collisionBuffer);
        sortCS.SetBuffer(sortKernel, "collisionBuffer", collisionBuffer);

		gridBuffer = new ComputeBuffer(Mathf.NextPowerOfTwo(gridSize * gridSize * gridSize), sizeof(uint) * 2);
		particleCS.SetBuffer(collisionKernel, "gridBuffer", gridBuffer);
		particleCS.SetBuffer(limitKernel, "gridBuffer", gridBuffer);
		particleCS.SetBuffer(clearKernel, "gridBuffer", gridBuffer);
		particleCS.SetBuffer(findKernel, "gridBuffer", gridBuffer);

		neisBuffer = new ComputeBuffer(particleCount, sizeof(uint) * 51);
		particleCS.SetBuffer(findKernel, "neisBuffer", neisBuffer);
		particleCS.SetBuffer(collisionKernel, "neisBuffer", neisBuffer);
		particleCS.SetBuffer(lambKernel, "neisBuffer", neisBuffer);
		particleCS.SetBuffer(particleKernel, "neisBuffer", neisBuffer);

        if (cubes.Length > 0)
		{
			cubesBuffer = new ComputeBuffer(cubes.Length, sizeof(float) * 3 * 3);
			particleCS.SetBuffer(collisionKernel, "cubes", cubesBuffer);
			particleCS.SetInt("cubeCount", cubes.Length);
			cubesTemp = new Vector3[cubes.Length];
		}
	}

	void OnDisable()
	{
		collisionBuffer.Release();
		gridBuffer.Release();
		argsBuffer.Release();
		cubesBuffer.Release();
		neisBuffer.Release();
        p.Release();
        pos.Release();
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(transform.position, worldSize * 2 + Vector3.one * radius * 2);
	}
}
