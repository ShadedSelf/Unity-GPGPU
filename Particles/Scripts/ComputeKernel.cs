using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeKernel
{
	private string _name;
	private int _index;

	private Vector3Int _threads;
	private Vector3Int _groupSizes;
	private ComputeShader _shader;

	public ComputeKernel(string name, ComputeShader shader, Vector3Int threads)
	{
		_name = name;
		_shader = shader;
		_threads = threads;
		_index = shader.FindKernel(name);
		
		uint x, y, z;
		shader.GetKernelThreadGroupSizes(_index, out x, out y, out z);
		_groupSizes = new Vector3Int((int)x, (int)y, (int)z);
	}

	public void SetBuffer(ComputeBuffer buffer, string name)
	{
		_shader.SetBuffer(_index, name, buffer);
	}

	public void Dispacth()
	{
		_shader.Dispatch(_index, 
			_threads.x / _groupSizes.x, 
			_threads.y / _groupSizes.y, 
			_threads.z / _groupSizes.z);
	}

	public void RecordDispatch(CommandBuffer cmdBuff, bool profile)
	{
		if (profile) { cmdBuff.BeginSample(_name); }
		
		cmdBuff.DispatchCompute(_shader, _index, 
			_threads.x / _groupSizes.x, 
			_threads.y / _groupSizes.y, 
			_threads.z / _groupSizes.z);
			
		if (profile) { cmdBuff.EndSample(_name); }
	}
}
