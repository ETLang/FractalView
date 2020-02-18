using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FractalView
{
    public class FractalTile
    {
        public Fractal Fractal;
        public FractalBuffer Data;
        public bool IsColorized;
        public Texture FractalTex;
        public double2 WindowMinima;
        public double2 WindowMaxima;
        public bool PreferCPU;
        public bool AwaitingCPURender;
        public bool DoneRendering;
        public JobHandle CPURenderJob;
        public HighPrecisionRenderer CPURenderData;
        int JobAge;

        static FractalTile()
        {
            _Quad = new Mesh();
            _Quad.vertices = new Vector3[]
            {
                new Vector3(0,1,0),
                new Vector3(1,1,0),
                new Vector3(0,0,0),
                new Vector3(1,0,0)
            };
            _Quad.SetIndices(new int[] { 0, 1, 2, 2, 1, 3 }, MeshTopology.Triangles, 0);
            _Quad.RecalculateBounds();
            _Quad.UploadMeshData(true);
        }

        public FractalTile(Fractal fractal, FractalBuffer resources, double2 windowMin, double2 windowMax, bool preferCPU = false)
        {
            Fractal = fractal;
            Data = resources;
            IsColorized = false;
            WindowMinima = windowMin;
            WindowMaxima = windowMax;
            PreferCPU = preferCPU || (windowMax.x - windowMin.x < 1e-5);
            FractalTex = PreferCPU ? (Texture)resources.CPUFractal : resources.GPUFractal;

            //if (key.LOD < 0)
            //    Edge = Effect.LOD0EdgeLength * (double)((long)1 << -key.LOD);
            //else
            //    Edge = Effect.LOD0EdgeLength / (double)((long)1 << key.LOD);
            //Position = new double2(Key.X, Key.Y) * Edge;

            //_filter = new Vector4(RFloat(), RFloat(), RFloat(), 1);
            _filter = Vector4.one;
        }

        public void RenderFractal()
        {
            IsColorized = false;
            //RenderFractalCPUDouble();
            //return;


            if (PreferCPU)
                RenderFractalCPUDouble();
            else
                RenderFractalGPUSingle();
        }

        public bool TryFinishRenderFractal()
        {
            //return TryFinishRenderFractalCPU();

            if (PreferCPU)
                return TryFinishRenderFractalCPU();
            else
                return true;
        }

        public void FinishRenderFractal()
        {
            if (PreferCPU)
                FinishRenderFractalCPU();
        }

        public void RenderFractalCPUDouble()
        {
            if (AwaitingCPURender) return;

            AwaitingCPURender = true;

            var tileResolution = Data.CPUFractal.width;

            CPURenderData = new HighPrecisionRenderer();
            CPURenderData.size = new double2(tileResolution, tileResolution);
            CPURenderData.minima = WindowMinima;
            CPURenderData.maxima = WindowMaxima;

            CPURenderData.minima.x *= -1;
            CPURenderData.maxima.x *= -1;

            CPURenderData.maxIter = Fractal.MaxIterations;
            CPURenderData.mj = Fractal.Mandulia;
            CPURenderData.modabs = Fractal.AbsMod;
            CPURenderData.offset = Fractal.C;
            CPURenderData.dataOut = new Unity.Collections.NativeArray<Color>(tileResolution * tileResolution, Unity.Collections.Allocator.Persistent);
            CPURenderJob = CPURenderData.Schedule(CPURenderData.dataOut.Length, tileResolution);
        }

        public bool TryFinishRenderFractalCPU()
        {
            if (!AwaitingCPURender) return false;
            if (DoneRendering) return false;

            JobAge++;

            if (JobAge < 4 && !CPURenderJob.IsCompleted)
                return false;

            AwaitingCPURender = false;
            DoneRendering = true;
            CPURenderJob.Complete();

            if (Data != null)
            {
                Data.CPUFractal.LoadRawTextureData(CPURenderData.dataOut);
                Data.CPUFractal.Apply();
            }

            CPURenderData.dataOut.Dispose();
            return true;
        }

        public void FinishRenderFractalCPU()
        {
            if (!AwaitingCPURender) return;
            if (DoneRendering) return;

            AwaitingCPURender = false;
            DoneRendering = true;
            CPURenderJob.Complete();

            if (Data != null)
            {
                Data.CPUFractal.LoadRawTextureData(CPURenderData.dataOut);
                Data.CPUFractal.Apply();
            }

            CPURenderData.dataOut.Dispose();
        }

        public void RenderFractalGPUSingle()
        {
            Data.BindFractalProperties(Fractal, WindowMinima, WindowMaxima);
            Help.BlitNow(null, Data.GPUFractal, Data.FractalMat);
            DoneRendering = true;
        }

        public void Colorize(Colorizer altColorizer = null)
        {
            if (AwaitingCPURender) return;
            if (Data == null) return;

            IsColorized = true;
            var colorizer = altColorizer ?? Fractal.Colorizer;
            colorizer.Colorize(FractalTex, Data.Colorized);
            //Graphics.Blit(FractalTex, Data.Colorized, colorizer.Material);
        }

        public void RenderComposition(float alpha, RenderTexture target = null, Texture altImage = null, bool flipY = false)
        {
            if (AwaitingCPURender) return;
            if (Data == null) return;
            if (_Quad == null) return;

            GL.modelview = Matrix4x4.identity;
            //var pos = new double2(Key.X, Key.Y);

            var quadScale = (WindowMaxima.x - WindowMinima.x) / Fractal.ViewScale;
            var quadPos = (WindowMinima - Fractal.ViewCenter) / Fractal.ViewScale;
            //var quadPos = (pos * quadScale) - viewCenter * Effect.LOD0EdgeLength / viewScale;
            var quadTransform = Matrix4x4.TRS(new Vector3((float)quadPos.x, (float)quadPos.y, 0/*alpha == 1 ? 0 : 1*/), Quaternion.identity, new Vector3((float)quadScale, (float)quadScale, 1));

            if (flipY)
                quadTransform = Matrix4x4.Scale(new Vector3(1, -1, 1)) * quadTransform;

            _filter.w = alpha;
            Data.BlitMat.SetVector(_FilterId, _filter);

            if (altImage != null)
                Data.BlitMat.mainTexture = altImage;

            //Graphics.DrawMesh(_Quad, quadTransform, Data.BlitMat, 0, cam);// alpha == 1 ? 0 : 1);

            Data.BlitMat.SetPass(0);

            RenderTexture oldRT = null;

            if (target != null)
            {
                oldRT = RenderTexture.active;
                Graphics.SetRenderTarget(target);
            }

            Graphics.DrawMeshNow(_Quad, quadTransform);// alpha == 1 ? 0 : 1);

            if (target != null)
                RenderTexture.active = oldRT;

            if (altImage != null)
                Data.BlitMat.mainTexture = Data.Colorized;
        }   

        Vector4 _filter;

        static Mesh _Quad;
        static int _FilterId = Shader.PropertyToID("_Filter");

        static Texture2D _DitherTex;

        static System.Random _r = new System.Random();
        static float RFloat() { return (float)_r.NextDouble(); }
    }
}
