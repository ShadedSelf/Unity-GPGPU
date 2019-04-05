using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeSystem
{
	public ComputeShader shader;
	private Dictionary<string, ComputeKernel> kernels = new Dictionary<string, ComputeKernel>();
	private Dictionary<string, ComputeBuffer> buffers = new Dictionary<string, ComputeBuffer>();
	private bool profile;

	public ComputeSystem(ComputeShader shader, bool profile)
	{
		this.shader = shader;
		this.profile = profile;
	}

	public Dictionary<string, ComputeBuffer> Buffers
	{
		get
		{
			return buffers;
		}
	}

	//-- Setup: --
	public void AddKernel(string name, Vector3Int threads)
	{
		kernels.Add(name, new ComputeKernel(name, shader, threads));
	}

	public void AddBuffer(string name, int count, int stride)
	{
		buffers.Add(name, new ComputeBuffer(count, stride));
	}

	public void SetAllBuffers()
	{
		foreach (var kernel in kernels.Values)
			foreach (var buffer in buffers)
				kernel.SetBuffer(buffer.Value, buffer.Key);
	}

	public void SetBufferAtKernel(string bufferName, string kernelName)
	{
		kernels[kernelName].SetBuffer(buffers[bufferName], bufferName);
	}

	public void SetBufferData(string bufferName, System.Array data)
	{
		buffers[bufferName].SetData(data);
	}

	//-- Getup: --
	public ComputeBuffer GetBuffer(string bufferName)
	{
		return buffers[bufferName];
	}

	//-- Dispatch: --
	public void Dispatch(string kernelName)
	{
		kernels[kernelName].Dispacth();
	}

	public void RecordDispatch(string kernelName, CommandBuffer cmdBuf)
	{
		kernels[kernelName].RecordDispatch(cmdBuf, profile);
	}

	//-- Cleanup: --
	public void Cleanup()
	{
		foreach (var buffer in buffers)
			buffer.Value.Release();
	}
}
