    using Unity.Burst;
    using Unity.Collections;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.Rendering;

    [BurstCompile]
    public struct ProcessMeshDataJob : IJobParallelFor {
        [ReadOnly] public Mesh.MeshDataArray meshData;
        public Mesh.MeshData outputMesh;

        public NativeArray<int> vertexStart;
        public NativeArray<int> triangleStart;
        public void Execute(int index) {
            var data = meshData[index];
            var vCount = data.vertexCount;
            var vStart = vertexStart[index];

            var vertices = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetVertices(vertices.Reinterpret<Vector3>());

            var normals = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetNormals(normals.Reinterpret<Vector3>());

            var uvs = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetUVs(0, uvs.Reinterpret<Vector3>()); // TEST with Vector2

            var outputVertices = outputMesh.GetVertexData<Vector3>();
            var outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
            var outputUVs = outputMesh.GetVertexData<Vector3>(stream: 2);

            for (int i = 0; i < vCount; i++) {
                outputVertices[i + vStart] = vertices[i];
                outputNormals[i + vStart] = normals[i];
                outputUVs[i + vStart] = uvs[i];
            }

            vertices.Dispose();
            normals.Dispose();
            uvs.Dispose();

            var tStart = triangleStart[index];
            var tCount = data.GetSubMesh(0).indexCount;
            var outputTriangles = outputMesh.GetIndexData<int>();
            if (data.indexFormat == IndexFormat.UInt16) {
                var triangles = data.GetIndexData<ushort>();
                for (int i = 0; i < tCount; ++i) {
                    int idx = triangles[i];
                    outputTriangles[i + tStart] = vStart + idx;
                }
            } else {
                var triangles = data.GetIndexData<int>();
                for (int i = 0; i < tCount; ++i) {
                    int idx = triangles[i];
                    outputTriangles[i + tStart] = vStart + idx;
                }
            }
        }
    }

