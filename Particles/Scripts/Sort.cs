using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Sorter
{
	const string BLOCK = "block";
	const string DIM   = "dim";
	const string COUNT = "count";

	private ComputeShader computeShader;
	private int count;
	public int sortKernel;

	public Sorter(int count)
	{
		this.count = count;
		computeShader = Finder.FindObject<ComputeShader>("SortCS");
		sortKernel = computeShader.FindKernel("Sort");
	}

    public void SetSortBuffer(CommandBuffer commandBuffer)
	{
		int x, y, z;
		CalcNums(count, out x, out y, out z);

		commandBuffer.BeginSample("Sorting");

		computeShader.SetInt(COUNT, count);
        for (int dim = 2; dim <= count; dim <<= 1)
		{
			commandBuffer.SetComputeIntParam(computeShader, DIM, dim);
			for (int block = dim >> 1; block > 0; block >>= 1)
			{
				commandBuffer.SetComputeIntParam(computeShader, BLOCK, block);
				commandBuffer.DispatchCompute(computeShader, sortKernel, x, y, z);
			}
		}

		commandBuffer.EndSample("Sorting");
	}

	const int GROUP_SIZE = 512;
	const int MAX_DIM_GROUPS = 1024;
	const int MAX_DIM_THREADS = (GROUP_SIZE * MAX_DIM_GROUPS);

	private void CalcNums(int length, out int x, out int y, out int z)
	{
		if (length <= MAX_DIM_THREADS)
		{
			x = (length - 1) / GROUP_SIZE + 1;
			y = z = 1;
		}
		else
		{
			x = MAX_DIM_GROUPS;
			y = (length - 1) / MAX_DIM_THREADS + 1;
			z = 1;
		}
	}
}
