using System.Collections.Generic;
using UnityEngine;
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;

namespace Utils {
    public static class MeshUtils {
        public static Mesh MergeMeshes(params Mesh[] meshes) {
            Mesh mesh = new Mesh();
            Dictionary<VertexData, int> pointsOrder = new Dictionary<VertexData, int>();
            HashSet<VertexData> pointsHash = new HashSet<VertexData>();
            List<int> triangles = new List<int>();
            int pIndex = 0;

            for (int i = 0; i < meshes.Length; i++) {
                if(meshes[i] == null) continue;
                for (int j = 0; j < meshes[i].vertices.Length; j++) {
                    Vector3 v = meshes[i].vertices[j];
                    Vector3 n = meshes[i].normals[j];
                    Vector2 u = meshes[i].uv[j];
                    VertexData p = new VertexData(v, n, u);
                    if (pointsHash.Contains(p)) continue;
                    pointsOrder.Add(p, pIndex);
                    pointsHash.Add(p);

                    pIndex++;
                }

                for (int t = 0; t < meshes[i].triangles.Length; t++) {
                    int trianglePoint = meshes[i].triangles[t];
                    
                    Vector3 v = meshes[i].vertices[trianglePoint];
                    Vector3 n = meshes[i].normals[trianglePoint];
                    Vector2 u = meshes[i].uv[trianglePoint];
                    VertexData p = new VertexData(v, n, u);

                    pointsOrder.TryGetValue(p, out var index);
                    triangles.Add(index);
                }
                meshes[i] = null;
            }
            ExtractArrays(pointsOrder, mesh);
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();

            return mesh;
        }

        public static void ExtractArrays(Dictionary<VertexData, int> list, Mesh mesh) {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            foreach (var v in list.Keys) {
                vertices.Add(v.Item1);
                normals.Add(v.Item2);
                uvs.Add(v.Item3);
            }

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
        }
        
        public enum BlockSide {
            BOTTOM = 0,
            TOP = 1,
            LEFT = 2,
            RIGHT = 3,
            FRONT = 4,
            BACK = 5
        }

        public static Dictionary<string, Vector3> Vertices = new() {
            { "p0", new Vector3(-0.5f, -0.5f, 0.5f) },
            { "p1", new Vector3(0.5f, -0.5f, 0.5f) },
            { "p2", new Vector3(0.5f, -0.5f, -0.5f) },
            { "p3", new Vector3(-0.5f, -0.5f, -0.5f) },
            { "p4", new Vector3(-0.5f, 0.5f, 0.5f) },
            { "p5", new Vector3(0.5f, 0.5f, 0.5f) },
            { "p6", new Vector3(0.5f, 0.5f, -0.5f) },
            { "p7", new Vector3(-0.5f, 0.5f, -0.5f) }
        };
        
    }
}
