using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Sort
{
	public const string KERNEL_SORT = "Sort";
	public const string KERNEL_INIT = "InitKeys";

	public const string PROP_BLOCK = "block";
	public const string PROP_DIM = "dim";
	public const string PROP_COUNT = "count";

	public const string BUF_KEYS = "keys";

	int _kernelSort, _kernelInit;

	public ComputeShader compute;
	public int count;

    uint[] key_data;

    public void Set()
	{
		_kernelSort = compute.FindKernel(KERNEL_SORT);
    }

    /*public void SortMe()
    {
        var count = _keys.count;
        int x, y, z;
        CalcNums(count, out x, out y, out z);

        compute.SetInt(PROP_COUNT, count);
        for (var dim = 2; dim <= count; dim <<= 1)
        {
            compute.SetInt(PROP_DIM, dim);
            for (var block = dim >> 1; block > 0; block >>= 1)
            {
                compute.SetInt(PROP_BLOCK, block);
                compute.SetBuffer(_kernelSort, BUF_KEYS, _keys);
                //compute.SetBuffer(_kernelSort, BUF_VALUES, values);
                compute.Dispatch(_kernelSort, x, y, z);
            }
        }
    }*/

    public void SetSortBuffer(CommandBuffer commandBuffer)
	{

		//var count = _keys.count;
		int x, y, z;
		CalcNums(count, out x, out y, out z);

		compute.SetInt(PROP_COUNT, count);

		commandBuffer.BeginSample("Sort");

        for (var dim = 2; dim <= count; dim <<= 1)
		{
			commandBuffer.SetComputeIntParam(compute, PROP_DIM, dim);
			for (var block = dim >> 1; block > 0; block >>= 1)
			{
				commandBuffer.SetComputeIntParam(compute, PROP_BLOCK, block);
				commandBuffer.DispatchCompute(compute, _kernelSort, x, y, z);
			}
		}

		commandBuffer.EndSample("Sort");
	}

	public const int GROUP_SIZE = 512;
	public const int MAX_DIM_GROUPS = 1024;
	public const int MAX_DIM_THREADS = (GROUP_SIZE * MAX_DIM_GROUPS);
	void CalcNums(int length, out int x, out int y, out int z)
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
