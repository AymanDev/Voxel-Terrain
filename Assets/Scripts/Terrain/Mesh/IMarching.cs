using System;
using System.Collections.Generic;
using Terrain.Voxel;
using UnityEngine;

namespace Terrain.Mesh
{
    public interface IMarching
    {
        float Surface { get; set; }

        Tuple<List<Vector3>, List<int>> Generate(TerrainManager terrain,
            int width,
            int height,
            int depth,
            Vector3 offset);
    }
}
