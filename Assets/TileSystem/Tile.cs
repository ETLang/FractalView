using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Tiling
{
    public class Tile<ResourceType, CacheType> where ResourceType : ITileData, new() where CacheType : ITileData, new()
    {
        public TileKey Key;
        public ResourceType Resources;
        public CacheType Cache;
        public TheEffect Effect;
        public bool IsColorized;
        public Texture Fractal;
        public double Edge;
        public double2 Position;
        public bool AwaitingCPURender;
        public bool DoneRendering;
        public JobHandle CPURenderJob;
        public HighPrecisionRenderer CPURenderData;
        int JobAge;

        public Tile(TheEffect effect, TileKey key, ResourceType resources)
        {
            Effect = effect;
            Key = key;
            Resources = resources;
            IsColorized = false;
            //Fractal = resources.GPUFractal;// (Edge < 1e-7) ? (Texture)resources.CPUFractal : resources.GPUFractal;

            if (key.LOD < 0)
                Edge = 1.0 * (double)((long)1 << -key.LOD);
            else
                Edge = 1.0 / (double)((long)1 << key.LOD);
            Position = new double2(Key.X, Key.Y) * Edge;

            //_filter = new Vector4(RFloat(), RFloat(), RFloat(), 1);
            _filter = Vector4.one;
        }

        //public void RenderFractal()
        //{
        //    IsColorized = false;
        //    //RenderFractalCPUDouble();
        //    //return;


        //    if (Edge < 1e-7)
        //        RenderFractalCPUDouble();
        //    else
        //        RenderFractalGPUSingle();
        //}

        //public bool TryFinishRenderFractal()
        //{
        //    //return TryFinishRenderFractalCPU();

        //    if (Edge < 1e-7)
        //        return TryFinishRenderFractalCPU();
        //    else
        //        return true;
        //}

        //public void RenderFractalCPUDouble()
        //{
        //    double2 window = Edge;

        //    if (AwaitingCPURender) return;

        //    AwaitingCPURender = true;

        //    CPURenderData = new HighPrecisionRenderer();
        //    CPURenderData.size = new double2(TileResolution, TileResolution);
        //    CPURenderData.minima = Position;
        //    CPURenderData.maxima = Position + window;

        //    CPURenderData.minima.x *= -1;
        //    CPURenderData.maxima.x *= -1;

        //    CPURenderData.maxIter = Effect.maxIterations;
        //    CPURenderData.mj = Effect.MandelbrotToJulia;
        //    CPURenderData.modabs = Effect.burningShip;
        //    CPURenderData.offset = new double2(Effect.Cx, Effect.Cy);
        //    CPURenderData.dataOut = new Unity.Collections.NativeArray<Color>(TileResolution * TileResolution, Unity.Collections.Allocator.Persistent);
        //    CPURenderJob = CPURenderData.Schedule(CPURenderData.dataOut.Length, TileResolution);
        //}

        //public bool TryFinishRenderFractalCPU()
        //{
        //    if (!AwaitingCPURender) return false;
        //    if (DoneRendering) return false;

        //    JobAge++;

        //    if (JobAge < 4 && !CPURenderJob.IsCompleted)
        //        return false;

        //    AwaitingCPURender = false;
        //    DoneRendering = true;
        //    CPURenderJob.Complete();

        //    if (Resources != null)
        //    {
        //        Resources.CPUFractal.LoadRawTextureData(CPURenderData.dataOut);
        //        Resources.CPUFractal.Apply();
        //    }

        //    CPURenderData.dataOut.Dispose();
        //    return true;
        //}

        //public void RenderFractalGPUSingle()
        //{
        //    var mat = Resources.FractalMat;

        //    if (Effect.burningShip)
        //        mat.EnableKeyword("MOD_ABS");
        //    else
        //        mat.DisableKeyword("MOD_ABS");

        //    if (Effect.displayVelocity)
        //        mat.EnableKeyword("VIZ_VEL");
        //    else
        //        mat.DisableKeyword("VIZ_VEL");

        //    if (Effect.displayMagnitude)
        //        mat.EnableKeyword("VIZ_MAG");
        //    else
        //        mat.DisableKeyword("VIZ_MAG");

        //    mat.SetInt(_MaxIterationsId, Effect.maxIterations);
        //    mat.SetVector(_CId, new Vector2(Effect.Cx, Effect.Cy));
        //    mat.SetFloat(_MandelBrotJuliaBlendId, Effect.MandelbrotToJulia);
        //    mat.SetVector(_FractalWindowPositionId, new Vector2((float)Position.x, (float)Position.y));
        //    mat.SetFloat(_FractalWindowSizeId, (float)Edge);

        //    Graphics.Blit(null, Resources.GPUFractal, mat);
        //}

        //public void Colorize()
        //{
        //    if (AwaitingCPURender) return;
        //    if (Resources == null) return;

        //    IsColorized = true;
        //    Graphics.Blit(Fractal, Resources.Colorized, Effect._colorizerMat);
        //}

        //public void RenderToScreen(double viewScale, double2 viewCenter, float alpha)
        //{
        //    if (AwaitingCPURender) return;
        //    if (Resources == null) return;

        //    var pos = new double2(Key.X, Key.Y);

        //    var quadScale = (Edge / viewScale);
        //    var quadPos = (pos * quadScale) - viewCenter * Effect.LOD0EdgeLength / viewScale;
        //    var quadTransform = Matrix4x4.TRS(new Vector3((float)quadPos.x, (float)quadPos.y, alpha == 1 ? 10 : 0), Quaternion.identity, new Vector3((float)quadScale, (float)quadScale, 1));
        //    _filter.w = alpha;
        //    Resources.BlitMat.SetVector(_FilterId, _filter);
        //    Graphics.DrawMesh(Effect._Quad, quadTransform, Resources.BlitMat, 0);// alpha == 1 ? 0 : 1);
        //}

        Vector4 _filter;
        static System.Random _r = new System.Random();
        static float RFloat() { return (float)_r.NextDouble(); }
    }
}