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
    public class PublishRenderer
    {
        public static void Render(Fractal fractal, Camera cam, FractalBufferPool pool, int ssLOD)
        {
            var target = cam.targetTexture ?? cam.activeTexture;
            var w = cam.pixelWidth;
            var h = cam.pixelHeight;

            var oldTarget = RenderTexture.active;

            if (target != null)
                RenderTexture.active = target;

            var ar = w / (float)h;
            var ortho = Matrix4x4.Ortho(-2 * ar, 2 * ar, -2, 2, -1, 1);
            //var camOrthe = cam.projectionMatrix;

            //GL.Clear(false, true, Color.blue);
            GL.PushMatrix();
            //GL.Viewport(new Rect(0, 0, 300, 300));
            //GL.Clear(false, true, Color.red);
            //GL.LoadProjectionMatrix(test == null ? cam.projectionMatrix : test.projectionMatrix);
            GL.LoadProjectionMatrix(Matrix4x4.Ortho(-2*ar, 2*ar, -2, 2, -1, 1));
            //GL.LoadPixelMatrix();
           // GL.LoadIdentity();
           // GL.LoadOrtho();
            //GL.LoadPrjectionMatrix(cam.projectionMatrix);
            //Help.RenderRectNow(target, new Material(Shader.Find("Hidden/BlitShader")), Matrix4x4.identity);
            //GL.PopMatrix();

            // Free cached tile resources before calling
            var tileJobChunk = Math.Max(3, pool.PreallocatedBuffersAvailable);
            
            // copy the colorizer
            var pubColorizer = new Colorizer(fractal.Colorizer);

            //pubColorizer.FlipVertical = true;

            RenderTextureDescriptor rtd = new RenderTextureDescriptor(1, 1, RenderTextureFormat.BGRA32);

            if (target != null)
            {
                pubColorizer.ConvertSRGB = !target.sRGB;
                rtd = target.descriptor;

                if (target.format == RenderTextureFormat.ARGB32)
                    pubColorizer.SwapRedBlue = true;
            }

            // Compute the grid dimensions
            int tileCols = (1 << ssLOD) * (w - 1) / pool.TileResolution + 1;
            int tileRows = (1 << ssLOD) * (h - 1) / pool.TileResolution + 1;

            // Compute the fractal window and edge length
            var cambb = Help.GetViewBoundingBox(ortho);
            var minima = fractal.ViewCenter + new double2(cambb.min.x, cambb.min.y) * fractal.ViewScale; 
            var maxima = fractal.ViewCenter + new double2(cambb.max.x, cambb.max.y) * fractal.ViewScale;
            var span = maxima - minima;
            var edge = span.x * pool.TileResolution / (w * (1 << ssLOD));

            // Flip the fractal over so the result is compatible with TIFF
            //var tmp = minima.y;
            //minima.y = maxima.y;
            //maxima.y = tmp;
            var edgeIncrement = new double2(edge, edge);

            // TODO: incorporate rotation

            // Allocate downsampling buffers
            RenderTexture[] downsamplers = null;
            if (ssLOD > 0)
            {
                downsamplers = new RenderTexture[ssLOD - 1];
                for (int i = 1; i < ssLOD; i++)
                {
                    var desc = rtd;
                    desc.width = desc.height = pool.TileResolution >> i;
                    downsamplers[i - 1] = new RenderTexture(desc);
                    downsamplers[i - 1].Create();
                }
            }

            List<double2> toRender = new List<double2>();

            for (int x = 0; x < tileCols; x++)
                for (int y = 0; y < tileRows; y++)
                    toRender.Add(new double2(x, y));

            int toRenderIndex = 0;
            var one2 = new double2(1, 1);
            List<FractalTile> tiles = new List<FractalTile>();
            while (toRenderIndex < toRender.Count)
            {
                int nextJobChunk = Math.Min(tileJobChunk, toRender.Count - toRenderIndex);

                // Initialize tiles
                for(int i = 0;i < nextJobChunk;i++)
                {
                    var index = toRender[i+ toRenderIndex];
                    var tileMin = minima + index * edgeIncrement;
                    var tileMax = minima + (index + one2) * edgeIncrement;

                    var tile = new FractalTile(fractal, pool.Get(), tileMin, tileMax, edge < 1e-5);
                    tiles.Add(tile);
                }

                for (int i = 0; i < tiles.Count; i++)
                    tiles[i].RenderFractal();

                JobHandle.ScheduleBatchedJobs();

                for (int i = 0; i < tiles.Count; i++)
                {
                    tiles[i].FinishRenderFractal();
                    tiles[i].Colorize(pubColorizer);

                    // Downsample 
                    Texture downSrc = tiles[i].Data.Colorized;

                    if (downsamplers != null)
                    {
                        for (int downsIndex = 0; downsIndex != downsamplers.Length; downsIndex++)
                        {
                            var downsTarget = downsamplers[downsIndex];
                            Help.BlitNow(downSrc, downsTarget);
                            //Help.Mark(downsTarget, UnityEngine.Random.ColorHSV());
                            downSrc = downsTarget;
                        }
                    }

                    // Render to target
                    tiles[i].RenderComposition(1, target, downSrc, true);
                }

                for (int i = 0; i < tiles.Count; i++)
                    pool.Free(tiles[i].Data);

                tiles.Clear();
                toRenderIndex += nextJobChunk;
            }

            if (target != null)
                RenderTexture.active = oldTarget;

            GL.PopMatrix();
        }
    }
}
