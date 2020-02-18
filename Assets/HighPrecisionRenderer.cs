using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct HighPrecisionRenderer : IJobParallelFor
{
    [ReadOnly]
    public double2 offset;

    [ReadOnly]
    public double mj;

    [ReadOnly]
    public int maxIter;

    [ReadOnly]
    public bool modabs;

    [ReadOnly]
    public double2 minima;

    [ReadOnly]
    public double2 maxima;

    [ReadOnly]
    public double2 size;

    [WriteOnly]
    public NativeArray<Color> dataOut; // width * height * 4

    public void Execute(int i)
    {
        double2 range = maxima - minima;
        double2 index = new double2(i % size.x, i / size.x);
        double2 uv = index / size;
        var pos = minima + range * uv;
        double2 C = pos + (offset - pos) * mj;
        double2 N = offset + (pos - offset) * mj;
        double2 N2 = N * N;
        int iter;
        for (iter = 0; iter < maxIter && (N2.x + N2.y) < 65536; iter++)
        {
            if (modabs)
            {

                N.x = Math.Abs(N.x);
                N.y = Math.Abs(N.y);
            }
            var newNx = N2.x - N2.y - C.x;
            N.y = 2 * N.x * N.y - C.y;
            N.x = newNx;
            N2 = N * N;
        }
        dataOut[i] = new Color((float)N.x, (float)N.y, iter, 0);
    }
}
