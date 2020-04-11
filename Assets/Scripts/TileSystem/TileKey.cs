
namespace Tiling
{
    public struct TileKey
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
}