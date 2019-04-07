using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeSystem
{
	private Dictionary<string, ComputeKernel> _kernels = new Dictionary<string, ComputeKernel>();
	private Dictionary<string, ComputeBuffer> _buffers = new Dictionary<string, ComputeBuffer>();
	
	private ComputeShader _shader;
	private bool _profile;

	public ComputeSystem(ComputeShader shader, bool profile = false)
	{
		_shader		= shader;
		_profile	= profile;
	}

	public ComputeShader shader
	{
		get { return _shader; }
	}

	//-- Add: --
	public void AddKernel(string name, Vector3Int threads)		{	_kernels.Add(name, new ComputeKernel(name, _shader, threads));	}
	public void AddBuffer(string name, int count, int stride)	{	_buffers.Add(name, new ComputeBuffer(count, stride));			}
	// public void AddRenderTexture()

	//-- Set: --
	public void SetAllBuffers()
	{
		foreach (var kernel in _kernels.Values)
			foreach (var buffer in _buffers)
				kernel.SetBuffer(buffer.Key, buffer.Value);
	}

	public void SetBufferAtKernel(string bufferName, string kernelName)	{	_kernels[kernelName].SetBuffer(bufferName, _buffers[bufferName]);	} // Not used, done automatically by unity
	public void SetBufferData(string bufferName, System.Array data)		{	_buffers[bufferName].SetData(data);									}

	//-- Get: --
	public ComputeBuffer GetBuffer(string bufferName) { return _buffers[bufferName]; }

	//-- Dispatch: --
	public void Dispatch(string kernelName)								{	_kernels[kernelName].Dispacth();							}
	public void RecordDispatch(string kernelName, CommandBuffer cmdBuf) {	_kernels[kernelName].RecordDispatch(cmdBuf, _profile);		}

	//-- Clean: --
	public void Cleanup()
	{
		foreach (var buffer in _buffers)
			buffer.Value.Release();
			
		_kernels.Clear();
		_buffers.Clear();
	}
}
