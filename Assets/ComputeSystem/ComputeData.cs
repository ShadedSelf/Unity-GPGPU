using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeData
{
	public Dictionary<string, ComputeBuffer> buffers	{ get; private set; } = new Dictionary<string, ComputeBuffer>();
	public Dictionary<string, RenderTexture> textures	{ get; private set; } = new Dictionary<string, RenderTexture>();

	//-- Add: -----------------
	public void AddBuffer(string name, int count, int stride)				=> buffers.Add(name, new ComputeBuffer(count, stride));
	public void AddBuffer(string name, ComputeBuffer buffer)				=> buffers.Add(name, buffer);

	public void AddRenderTexture(string name, RenderTextureDescriptor desc)	=> textures.Add(name, new RenderTexture(desc));
	public void AddRenderTexture(string name, RenderTexture rt)				=> textures.Add(name, rt);

	//-- Clean: ---------------
	public void Cleanup()
	{
		foreach (var buffer in buffers)		{ buffer.Value.Release();	}
		foreach (var texture in textures)	{ texture.Value.Release();	}
		
		buffers.Clear();
		textures.Clear();
	}
}

// public enum GpuResourceType
// {
// 	Buffer,
// 	RenderTexture,
// }

// public class GpuResource
// {
// 	public GpuResourceType type	{ get; private set; }
// 	public string name			{ get; private set; }
// 	public bool isFreed			{ get; private set; } = false;

// 	private object resource;

// 	private GpuResource(object resource, GpuResourceType type)
// 	{
// 		 this.resource = resource;
// 		 this.type = type;
// 	}
// 	public GpuResource(ComputeBuffer resource)	: this(resource, GpuResourceType.Buffer)		{ }
// 	public GpuResource(RenderTexture resource)	: this(resource, GpuResourceType.RenderTexture)	{ }

// 	public void Bind(ComputeShader shader, int kernelIndex)
// 	{
// 		switch (type)
// 		{
// 			case (GpuResourceType.Buffer)			: shader.SetBuffer(kernelIndex, name, (ComputeBuffer)resource); break;
// 			case (GpuResourceType.RenderTexture)	: shader.SetTexture(kernelIndex, name, (RenderTexture)resource); break;
// 		}
// 	}

// 	public void Release()
// 	{
// 		switch (type)
// 		{
// 			case (GpuResourceType.Buffer)			: ((ComputeBuffer)resource).Release(); break;
// 			case (GpuResourceType.RenderTexture)	: ((RenderTexture)resource).Release(); break;
// 		}
// 		isFreed = true;
// 	}
// }