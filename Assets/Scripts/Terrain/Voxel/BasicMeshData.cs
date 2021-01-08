using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Terrain.Voxel {
    public static class BasicMeshData {
        public static class CubeMeshData {
            private const float CubeSize = 1f;

            private static readonly Vector3[] Vertices = {
                    new Vector3(CubeSize, CubeSize, CubeSize),
                    new Vector3(-CubeSize, CubeSize, CubeSize),
                    new Vector3(-CubeSize, -CubeSize, CubeSize),
                    new Vector3(CubeSize, -CubeSize, CubeSize),
                    new Vector3(-CubeSize, CubeSize, -CubeSize),
                    new Vector3(CubeSize, CubeSize, -CubeSize),
                    new Vector3(CubeSize, -CubeSize, -CubeSize),
                    new Vector3(-CubeSize, -CubeSize, -CubeSize),
            };

            private static readonly int[][] FaceTriangles = {
                    new[] {0, 1, 2, 3},
                    new[] {5, 0, 3, 6},
                    new[] {4, 5, 6, 7},
                    new[] {1, 4, 7, 2},
                    new[] {5, 4, 1, 0},
                    new[] {3, 2, 7, 6},
            };

            public static IEnumerable<Vector3> FaceVertices(int side, Vector3 offset) {
                var faceVerts = new Vector3[4];
                for (var i = 0; i < faceVerts.Length; i++) {
                    faceVerts[i] = Vertices[FaceTriangles[side][i]] + offset;
                }

                return faceVerts;
            }

            public static IEnumerable<Vector3> FaceVertices(Direction dir, Vector3 offset) {
                return FaceVertices((int) dir, offset);
            }

            public static float3[] FaceVertices(int side, float3 offset) {
                var faceVerts = new float3[4];
                for (var i = 0; i < faceVerts.Length; i++) {
                    var vert = new float3(Vertices[FaceTriangles[side][i]]);
                    faceVerts[i] = vert + offset;
                }

                return faceVerts;
            }

            public static float3[] FaceVertices(Direction dir, int3 offset) {
                return FaceVertices((int) dir, offset);
            }
        }
    }
}