using System;
using System.Collections.Generic;
using System.Linq;
using Terrain.Voxel;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Terrain.Mesh
{
    public abstract class Marching : IMarching
    {

        public float Surface { get; set; }
        private float[] Cube { get; set; }
        protected int[] WindingOrder { get; private set; }

        public Marching(float surface = 0.5f)
        {
            Surface = surface;
            Cube = new float[8];
            WindingOrder = new[] { 0, 1, 2 };
        }

        public Tuple<List<Vector3>, List<int>> Generate(TerrainManager terrain,
            int width,
            int height,
            int depth,
            Vector3 offset)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();

            if (Surface > 0.0f)
            {
                WindingOrder[0] = 0;
                WindingOrder[1] = 1;
                WindingOrder[2] = 2;
            }
            else
            {
                WindingOrder[0] = 2;
                WindingOrder[1] = 1;
                WindingOrder[2] = 0;
            }

            int x, y, z, i;
            int ix, iy, iz;
            for (x = 0; x < width - 1; x++)
            {
                for (y = 0; y < height - 1; y++)
                {
                    for (z = 0; z < depth - 1; z++)
                    {
                        //Get the values in the 8 neighbours which make up a cube
                        for (i = 0; i < 8; i++)
                        {
                            ix = x + VertexOffset[i, 0];
                            iy = y + VertexOffset[i, 1];
                            iz = z + VertexOffset[i, 2];
                            var voxel = terrain.GetVoxelDataInPosition(offset, new Vector3(ix, iy, iz));
                            // var voxel = voxels.Find(p => p.X == ix && p.Y == iy && p.Z == iz);
                            Cube[i] = voxel.Value == -1 ? 0 : voxel.Value;
                        }

                        //Perform algorithm
                        March(x, y, z, Cube, verts, tris);
                    }
                }
            }
            return new Tuple<List<Vector3>, List<int>>(verts, tris);
        }


        /// <summary>
        /// MarchCube performs the Marching algorithm on a single cube
        /// </summary>
        protected abstract void March(float x,
            float y,
            float z,
            float[] cube,
            IList<Vector3> vertList,
            IList<int> indexList);

        /// <summary>
        /// GetOffset finds the approximate point of intersection of the surface
        /// between two points with the values v1 and v2
        /// </summary>
        protected virtual float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }

        /// <summary>
        /// VertexOffset lists the positions, relative to vertex0, 
        /// of each of the 8 vertices of a cube.
        /// vertexOffset[8][3]
        /// </summary>
        protected static readonly int[,] VertexOffset = new int[,]
        {
            { 0, 0, 0 }, { 1, 0, 0 }, { 1, 1, 0 }, { 0, 1, 0 },
            { 0, 0, 1 }, { 1, 0, 1 }, { 1, 1, 1 }, { 0, 1, 1 }
        };
    }
}
