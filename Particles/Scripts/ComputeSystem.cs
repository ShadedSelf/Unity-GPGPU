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
		ComputeKernel kernel;
		if (kernels.TryGetValue(kernelName, out kernel))
		{
			ComputeBuffer buffer;
			if (buffers.TryGetValue(bufferName, out buffer))
				kernel.SetBuffer(buffer, bufferName);
		}
	}

	public void SetBufferData(string bufferName, System.Array data)
	{
		ComputeBuffer buffer;
		if (buffers.TryGetValue(bufferName, out buffer))
			buffer.SetData(data);
	}

	//-- Getup: --
	public ComputeBuffer GetBuffer(string bufferName)
	{
		ComputeBuffer buffer;
		if (buffers.TryGetValue(bufferName, out buffer))
			return buffer;
		return null;
	}

	//-- Dispatch: --
	public void Dispatch(string kernelName)
	{
		ComputeKernel kernel;
		if (kernels.TryGetValue(kernelName, out kernel))
			kernel.Dispacth();
	}

	public void RecordDispatch(string kernelName, CommandBuffer cmdBuf)
	{
			ComputeKernel kernel;
		if (kernels.TryGetValue(kernelName, out kernel))
			kernel.RecordDispatch(cmdBuf, profile);
	}

	//-- Cleanup: --
	public void Cleanup()
	{
		foreach (var buffer in buffers)
			buffer.Value.Release();
	}
}
