using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeKernel
{
	public string name;
	public int index;

	private Vector3Int threads;
	private Vector3Int groupSizes;
	private ComputeShader shader;

	public ComputeKernel(string name, ComputeShader shader, Vector3Int threads)
	{
		this.name = name;
		this.shader = shader;
		this.threads = threads;

		index = shader.FindKernel(name);
		uint x, y, z;
		shader.GetKernelThreadGroupSizes(index, out x, out y, out z);
		groupSizes = new Vector3Int((int)x, (int)y, (int)z);
	}

	public void SetBuffer(ComputeBuffer buffer, string name)
	{
		shader.SetBuffer(index, name, buffer);
	}

	public void Dispacth()
	{
		shader.Dispatch(index, 
			threads.x / groupSizes.x, 
			threads.y / groupSizes.y, 
			threads.z / groupSizes.z);
	}

	public void RecordDispatch(CommandBuffer cmdBuff, bool profile)
	{
		if (profile) { cmdBuff.BeginSample(name); }
		
		cmdBuff.DispatchCompute(shader, index, 
			threads.x / groupSizes.x, 
			threads.y / groupSizes.y, 
			threads.z / groupSizes.z);
			
		if (profile) { cmdBuff.EndSample(name); }
	}
}
