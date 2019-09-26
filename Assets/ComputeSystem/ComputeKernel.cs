using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class ComputeKernel
{
	public ComputeShader shader		{ get; private set; }
	public string name				{ get; private set; }
	public int index				{ get; private set; }

	public Vector3Int threads		{ get; private set; }
	public Vector3Int groupSizes	{ get; private set; }

	public ComputeKernel(string name, ComputeShader shader, Vector3Int threads)
	{
		this.shader		= shader;
		this.name		= name;
		this.index		= shader.FindKernel(name);
		this.threads	= threads;
		
		shader.GetKernelThreadGroupSizes(index, out uint x, out uint y, out uint z);
		this.groupSizes = new Vector3Int((int)x, (int)y, (int)z);
	}

	//-- Set: ------------
	public void SetBuffer(string bufferName, ComputeBuffer buffer)	=> shader.SetBuffer(index, bufferName, buffer);
	public void SetTexture(string textureName, Texture texture)		=> shader.SetTexture(index, textureName, texture);

	//-- Dispatch: -------- 
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
