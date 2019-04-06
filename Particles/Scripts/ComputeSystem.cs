using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeSystem
{
	public ComputeShader _shader;
	private Dictionary<string, ComputeKernel> _kernels = new Dictionary<string, ComputeKernel>();
	private Dictionary<string, ComputeBuffer> _buffers = new Dictionary<string, ComputeBuffer>();
	private bool _profile;

	public ComputeSystem(ComputeShader shader, bool profile)
	{
		_shader = shader;
		_profile = profile;
	}

	public ComputeShader shader
	{
		get { return _shader; }
	}

	//-- Setup: --
	public void AddKernel(string name, Vector3Int threads)		{	_kernels.Add(name, new ComputeKernel(name, _shader, threads));	}
	public void AddBuffer(string name, int count, int stride)	{	_buffers.Add(name, new ComputeBuffer(count, stride));			}

	public void SetAllBuffers()
	{
		foreach (var kernel in _kernels.Values)
			foreach (var buffer in _buffers)
				kernel.SetBuffer(buffer.Value, buffer.Key);
	}

	public void SetBufferAtKernel(string bufferName, string kernelName)	{	_kernels[kernelName].SetBuffer(_buffers[bufferName], bufferName);	}
	public void SetBufferData(string bufferName, System.Array data)		{	_buffers[bufferName].SetData(data);									}

	//-- Getup: --
	public ComputeBuffer GetBuffer(string bufferName) { return _buffers[bufferName]; }

	//-- Dispatch: --
	public void Dispatch(string kernelName)								{	_kernels[kernelName].Dispacth();							}
	public void RecordDispatch(string kernelName, CommandBuffer cmdBuf) {	_kernels[kernelName].RecordDispatch(cmdBuf, _profile);		}

	//-- Cleanup: --
	public void Cleanup()
	{
		foreach (var buffer in _buffers)
			buffer.Value.Release();
		_kernels.Clear();
		_buffers.Clear();
	}
}
