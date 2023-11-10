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
    
    [Header("Perlin Settings")] 
    [SerializeField] private float _heightScale = 10;
    [SerializeField] private float _scale = 0.001f;
    [SerializeField] private int _octaves = 8;
    [SerializeField] private float _heightOffset = -33;
    
    private Block[,,] _blocks;
    private MeshUtils.BlockType[] _blocksData;
    private Vector3 _location;

    public MeshUtils.BlockType[] BlockData => _blocksData;
    public int Width => _width;
    public int Height => _height;
    public int Depth => _depth;
    public Vector3 Location => _location;

    
    
    private void BuildChunk() {
        var blockCount = _width * _height * _depth;
        _blocksData = new MeshUtils.BlockType[blockCount];
        for (int i = 0; i < blockCount; i++) {
            int x = i % _width + (int)_location.x;
            int y = (i / _width) % _height + (int)_location.y;
            int z = i / (_width * _height) + (int)_location.z;
            _blocksData[i] = MeshUtils.fBM(x, z, _scale, _heightScale, _octaves, -_heightOffset) > y ? MeshUtils.BlockType.DIRT : MeshUtils.BlockType.AIR;
        }
    }


    public void CreateChunk(Vector3 dimension, Vector3 position) {
        _location = position;
        _width = (int)dimension.x;
        _height = (int)dimension.y;
        _depth = (int)dimension.z;
        
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        MeshCollider mc = gameObject.AddComponent<MeshCollider>();

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
        
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                for (int z = 0; z < _depth; z++) {
                    _blocks[x, y, z] = new Block(new Vector3(x, y, z) + _location, _blocksData[x + _width * (y + _depth * z)], this);
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
        newMesh.name = "Chunk_" + _location.x + "_" + _location.y + "_" + _location.z; 
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
        mc.sharedMesh = newMesh;
    }
}