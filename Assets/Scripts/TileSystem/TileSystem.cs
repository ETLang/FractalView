using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiling
{
    public interface ITileData
    {
        void Initialize(TileKey key);
    }

    public class TileSystem<ResourceType, CacheType> where ResourceType : ITileData, new() where CacheType : ITileData, new()
    {
        Stack<ResourceType> _freeTilePool = new Stack<ResourceType>();
        Dictionary<TileKey, Tile<ResourceType, CacheType>> _tileCache = new Dictionary<TileKey, Tile<ResourceType, CacheType>>();
    }
}