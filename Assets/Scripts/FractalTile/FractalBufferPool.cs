using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FractalView
{
    public class FractalBufferPool
    {
        public int TileResolution;
        public Shader FractalShader;
        public Shader BlitShader;
        public Texture2D DitherTexture;

        public int PreallocatedBuffersAvailable { get { return _freePool.Count; } }

        public FractalBufferPool(int tileResolution = 256)
        {
            TileResolution = tileResolution;
            FractalShader = Shader.Find("Hidden/FractalShader");
            BlitShader = Shader.Find("Hidden/BlitShader");

            DitherTexture = new Texture2D(tileResolution, tileResolution, TextureFormat.RFloat, false);

            var ditherValues = new float[tileResolution * tileResolution];

            for (int i = 0; i < tileResolution * tileResolution; i++)
                ditherValues[i] = UnityEngine.Random.value;

            DitherTexture.SetPixelData(ditherValues, 0);
            DitherTexture.Apply();
        }

        public FractalBuffer Get()
        {
            if (_freePool.Count == 0)
                return new FractalBuffer(this);
            else
                return _freePool.Pop();
        }

        public void Free(FractalBuffer buffer)
        {
            _freePool.Push(buffer);
        }

        Stack<FractalBuffer> _freePool = new Stack<FractalBuffer>();
    }
}
