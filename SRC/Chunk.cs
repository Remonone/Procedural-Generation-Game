using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Chunk : MonoBehaviour {
    [SerializeField] private Material _atlas;

    [SerializeField] private int _width = 16;
    [SerializeField] private int _depth = 16;
    [SerializeField] private int _height = 42;

    private Block[,,] _blocks;

    private MeshUtils.BlockType[] _blocksData;

    public MeshUtils.BlockType[] BlockData => _blocksData;
    public int Width => _width;
    public int Height => _height;
    public int Depth => _depth;
    
    private void BuildChunk() {
        var blockCount = _width * _height * _depth;
        _blocksData = new MeshUtils.BlockType[blockCount];
        for (int i = 0; i < blockCount; i++) {
            int x = i % _width;
            int y = (i / _width) % _height;
            int z = i / (_width * _height);
            _blocksData[i] = MeshUtils.fBM(x, z, 0.001f, 10, 8, -33) > y ? MeshUtils.BlockType.DIRT : MeshUtils.BlockType.AIR;
        }
    }

    private void Start() {
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

        mr.material = _atlas;
        _blocks = new Block[_width, _height, _depth];
        
        BuildChunk();

        var inputMeshes = new List<Mesh>();
        int vertexStart = 0;
        int triangleStart = 0;
        int meshCount = _width * _height * _depth;
        int m = 0;
        var jobs = new ProcessMeshDataJob();
        // TEMP -> TEMPJOB
        jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        jobs.triangleStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        
        for (int x = 0; x < _depth; x++) {
            for (int y = 0; y < _height; y++) {
                for (int z = 0; z < _width; z++) {
                    _blocks[x, y, z] = new Block(new Vector3(x, y, z), _blocksData[x + _width * (y + _depth * z)], this);
                    if (_blocks[x, y, z].Mesh == null) continue;
                    inputMeshes.Add(_blocks[x, y, z].Mesh);
                    var vcount = _blocks[x, y, z].Mesh.vertexCount;
                    var icount = (int)_blocks[x, y, z].Mesh.GetIndexCount(0);
                    jobs.vertexStart[m] = vertexStart;
                    jobs.triangleStart[m] = triangleStart;
                    vertexStart += vcount;
                    triangleStart += icount;
                    m++;
                }
            }
        }

        jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        var outputMeshData = Mesh.AllocateWritableMeshData(1);
        jobs.outputMesh = outputMeshData[0];
        jobs.outputMesh.SetIndexBufferParams(triangleStart, IndexFormat.UInt32);
        jobs.outputMesh.SetVertexBufferParams(vertexStart, 
            new VertexAttributeDescriptor(VertexAttribute.Position), 
            new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2));
        var handle = jobs.Schedule(inputMeshes.Count, 4);
        var newMesh = new Mesh();
        newMesh.name = "Chunk";
        var sm = new SubMeshDescriptor(0, triangleStart, MeshTopology.Triangles);
        sm.firstVertex = 0;
        sm.vertexCount = vertexStart;
        
        handle.Complete();

        jobs.outputMesh.subMeshCount = 1;
        jobs.outputMesh.SetSubMesh(0, sm);
        
        Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] {newMesh});
        jobs.meshData.Dispose();
        jobs.vertexStart.Dispose();
        jobs.triangleStart.Dispose();
        newMesh.RecalculateBounds();

        mf.mesh = newMesh;
    }
}