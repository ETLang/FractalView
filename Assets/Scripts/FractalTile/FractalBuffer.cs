using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace FractalView
{
    public class FractalBuffer
    {
        public Texture2D CPUFractal;
        public RenderTexture GPUFractal;
        public RenderTexture Colorized;
        public Material FractalMat;
        public Material BlitMat;

        public FractalBuffer(FractalBufferPool factory)
        {
            var desc = new RenderTextureDescriptor(factory.TileResolution, factory.TileResolution, RenderTextureFormat.ARGBFloat);
            GPUFractal = new RenderTexture(desc);
            GPUFractal.filterMode = FilterMode.Point;
            GPUFractal.Create();

            CPUFractal = new Texture2D(factory.TileResolution, factory.TileResolution, TextureFormat.RGBAFloat, false);
            CPUFractal.filterMode = FilterMode.Point;

            desc = new RenderTextureDescriptor(factory.TileResolution, factory.TileResolution, RenderTextureFormat.BGRA32);
            Colorized = new RenderTexture(desc);
            Colorized.Create();

            FractalMat = new Material(factory.FractalShader);
            BlitMat = new Material(factory.BlitShader);
            BlitMat.mainTexture = Colorized;
        }

        public void BindFractalProperties(Fractal fractal, double2 minima, double2 maxima)
        {
            var mat = FractalMat;

            if (fractal.AbsMod)
                mat.EnableKeyword("MOD_ABS");
            else
                mat.DisableKeyword("MOD_ABS");

            mat.SetInt(_MaxIterationsId, fractal.MaxIterations);
            mat.SetVector(_CId, new Vector2((float)fractal.C.x, (float)fractal.C.y));
            mat.SetFloat(_MandelBrotJuliaBlendId, (float)fractal.Mandulia);

            // TODO convert to minima/maxima in shader
            mat.SetVector(_FractalWindowPositionId, new Vector2((float)minima.x, (float)minima.y));
            mat.SetFloat(_FractalWindowSizeId, (float)(maxima.x - minima.x));

            // TODO add rotation to shader
        }

        static int _MaxIterationsId = Shader.PropertyToID("_MaxIterations");
        static int _CId = Shader.PropertyToID("_C");
        static int _MandelBrotJuliaBlendId = Shader.PropertyToID("_MandelBrotJuliaBlend");
        static int _FractalWindowPositionId = Shader.PropertyToID("_FractalWindowPosition");
        static int _FractalWindowSizeId = Shader.PropertyToID("_FractalWindowSize");
    }
}
