using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FractalView
{
    public class LiveRenderer
    {
        private const double CPUThresholdEdge = 1e-5;
        private static readonly int CPUThresholdLOD = (int)Math.Log(1 / CPUThresholdEdge, 2) - 1;

        public LiveRenderer(Fractal fractal, FractalBufferPool pool)
        {
            if (fractal == null)
                throw new ArgumentNullException("fractal");
            if (pool == null)
                throw new ArgumentNullException("pool");

            _fractal = fractal;
            _pool = pool;
            tileUpdatesPerFrame = 10;

            _fractal.Colorizer.Changed += Colorizer_Changed;
            _fractal.FractalChanged += OnFractalChanged;
        }

        public void Render(Camera cam, double detailBias)
        {
            if (cam == null)
                throw new ArgumentNullException("cam");

            if (cam == null) return;


            // Compute current LOD
            var lod = detailBias - Math.Log(_fractal.ViewScale, 2);
            var lowLOD = (int)Math.Floor(lod);
            var lodBlend = (float)(lod - lowLOD);

            // Adjust tile updates count based on framerate
            if (Time.unscaledDeltaTime > 1.0f / 30.0f)
                tileUpdatesPerFrame = Math.Max(1, tileUpdatesPerFrame - 1);
            if (tileUpdatesLastFrame >= tileUpdatesPerFrame && Time.unscaledDeltaTime < 1.0f / 30.0f)
                tileUpdatesPerFrame += 0.25f;

            var isGPU = (lowLOD < CPUThresholdLOD);
            if (!isGPU && wasGPU)
                tileUpdatesPerFrame = 1;
            wasGPU = isGPU;

            var desiredTileKeys = FindNeededTiles(cam, lowLOD);
            tileUpdatesLastFrame = 0;
            foreach (var tile in _tileCache.Bs)
                if (tile.TryFinishRenderFractal())
                    tileUpdatesLastFrame++;

            var newTileKeys = SelectNewTileKeys(desiredTileKeys).ToArray();
            var tilesToUpdate = (int)Math.Min(tileUpdatesPerFrame, newTileKeys.Length);
            for (int i = 0; i < tilesToUpdate; i++)
            {
                var tile = AllocateTile(newTileKeys[i]);
                tile.RenderFractal();
            }
            JobHandle.ScheduleBatchedJobs();
            FreeOldTiles(desiredTileKeys);

            // Colorize
            foreach (var tile in _tileCache.Bs)
            {
                if (!tile.IsColorized)
                    tile.Colorize();
            }

            { // Final Composite
                if (_tileCache.Count == 0)
                    return;

                var minLOD = _tileCache.As.Min(key => key.LOD);
                var maxLOD = lowLOD + 1;
                for (int i = minLOD; i <= lowLOD; i++)
                {
                    foreach (var tile in _tileCache)
                        if (tile.Key.LOD == i)
                            tile.Value.RenderComposition(1);
                }

                foreach (var tile in _tileCache)
                    if (tile.Key.LOD == maxLOD)
                        tile.Value.RenderComposition(lodBlend);
            }
        }

        public void PurgeCache()
        {
            FreeOldTiles(null);
        }

        #region Private

        private void OnFractalChanged()
        {
            PurgeCache();
        }

        private void Colorizer_Changed()
        {
            foreach (var tile in _tileCache.Bs)
                tile.IsColorized = false;
        }

        private FractalTile AllocateTile(TileKey key)
        {
            double edge;

            if (key.LOD < 0)
                edge = 1.0 * ((long)1 << -key.LOD);
            else
                edge = 1.0 / ((long)1 << key.LOD);

            var pos = new double2(key.X, key.Y);
            var minima = new double2(key.X * edge, key.Y * edge);
            var maxima = new double2((key.X + 1) * edge, (key.Y + 1) * edge);

            var tile = new FractalTile(_fractal, _pool.Get(), minima, maxima, edge < 1e-5);
            _tileCache[key] = tile;
            return tile;
        }

        private void FreeTile(FractalTile tile)
        {
            _tileCache.Remove(tile);
            _pool.Free(tile.Data);
        }

        private IEnumerable<TileKey> SelectNewTileKeys(HashSet<TileKey> keys)
        {
            foreach (var key in keys)
                if (!_tileCache.Contains(key))
                    yield return key;
        }

        private void FreeOldTiles(HashSet<TileKey> keys)
        {
            List<TileKey> remove = new List<TileKey>();

            foreach (var kvp in _tileCache)
                if (keys == null || !keys.Contains(kvp.Key))
                {
                    if (!kvp.Value.AwaitingCPURender)
                        remove.Add(kvp.Key);
                    if (kvp.Value.Data != null)
                        _pool.Free(kvp.Value.Data);
                    kvp.Value.Data = null;
                }

            foreach (var key in remove)
                _tileCache.Remove(key);
        }

        private HashSet<TileKey> FindNeededTiles(Camera cam, int lowLOD)
        {
            var bb = Help.GetViewBoundingBox(cam);

            var tiles = new HashSet<TileKey>();

            FindTilesOfLOD(tiles, bb, lowLOD);
            FindTilesOfLOD(tiles, bb, lowLOD + 1);
            return tiles;
        }

        private void FindTilesOfLOD(HashSet<TileKey> tiles, Bounds bb, int lod)
        {
            double edge;
            if (lod < 0)
                edge = 1.0 * (double)((long)1 << -lod);
            else
                edge = 1.0 / (double)((long)1 << lod);

            var minima = new double2(bb.min.x, bb.min.y);
            var maxima = new double2(bb.max.x, bb.max.y);
            var tileScale = _fractal.ViewScale / edge;
            minima *= tileScale;
            maxima *= tileScale;
            minima += _fractal.ViewCenter / edge;
            maxima += _fractal.ViewCenter / edge;

            var maxX = (long)Math.Ceiling(maxima.x);
            var maxY = (long)Math.Ceiling(maxima.y);
            for (long x = (long)Math.Floor(minima.x) - 1; x < maxX + 1; x++)
                for (long y = (long)Math.Floor(minima.y) - 1; y < maxY + 1; y++)
                    tiles.Add(new TileKey(lod, x, y));
        }

        private struct TileKey
        {
            public int LOD;
            public long X;
            public long Y;

            public TileKey(int lod, long x, long y)
            {
                LOD = lod;
                X = x;
                Y = y;
            }

            public override int GetHashCode()
            {
                var xh = X.GetHashCode();
                var yh = Y.GetHashCode();
                var lodh = LOD.GetHashCode();
                return xh ^ (yh << 10) ^ (lodh << 20);
            }

            public override bool Equals(object obj)
            {
                var rhs = (TileKey)obj;
                return rhs.X == X && rhs.Y == Y && rhs.LOD == LOD;
            }
        }

        private Fractal _fractal;
        private FractalBufferPool _pool;
        private Relation<TileKey, FractalTile> _tileCache = new Relation<TileKey, FractalTile>();
        public float tileUpdatesPerFrame;
        public int tileUpdatesLastFrame;
        private bool wasGPU = true;

        #endregion
    }
}
