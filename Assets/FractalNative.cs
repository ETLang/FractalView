using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class FractalNative
{
    #region Interop

    [DllImport("FractalNative")]
    public static extern void CopyFractalTo([Out, MarshalAs(UnmanagedType.LPArray)] float[] data);

    [DllImport("FractalNative")]
    public static extern void GenerateFractal(
        double cr,
        double ci,
        double mj,
        int maxIter,
        [MarshalAs(UnmanagedType.I1)] bool modabs,
        [MarshalAs(UnmanagedType.I1)] bool useMagnitude,
        double minx,
        double maxx,
        double miny,
        double maxy,
        int width,
        int height,
        int ss);

    #endregion
}
