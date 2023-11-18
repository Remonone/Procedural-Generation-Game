﻿using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
    [SerializeField] private Material _atlas;

    [SerializeField] private int _width = 16;
    [SerializeField] private int _depth = 16;
    [SerializeField] private int _height = 16;

    private Block[,,] _blocks;
    private int[] _blocksData;
    private Vector3Int _location;
    private MeshRenderer _renderer;

    public int[] BlockData => _blocksData;
    public int Width => _width;
    public int Height => _height;
    public int Depth => _depth;
    public Vector3 Location => _location;
    public MeshRenderer Renderer => _renderer;

    private JobHandle _jobHandle;
    private CalculateBlockTypes _cbs;


    private void BuildChunk(World parent) {
        var blockCount = _width * _height * _depth;
        _blocksData = new int[blockCount];
        var blocks = new NativeArray<int>(_blocksData, Allocator.Persistent);
        var data = new NativeArray<BlockDetails.BlockDataChunk>(BlockDetails.DataChunks, Allocator.Persistent);
        _cbs = new CalculateBlockTypes {
            cData = blocks, 
            width = _width, 
            height = _height, 
            location = _location,
            caveSettings = parent.CaveSettings,
            dataChunks = data
        };
        _jobHandle = _cbs.Schedule(_blocksData.Length, 64);
        _jobHandle.Complete();
        blocks.CopyTo(_blocksData);
        blocks.Dispose();
        data.Dispose();

    }

    

    public void CreateChunk(Vector3Int dimension, Vector3Int position, World parent) {
        _location = position;
        _width = dimension.x;
        _height = dimension.y;
        _depth = dimension.z;
        
        _blocks = new Block[_width, _height, _depth];
        BuildChunk(parent);

        RenderChunk();
    }

    private void RenderChunk() {
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        _renderer = gameObject.AddComponent<MeshRenderer>();
        _renderer.material = _atlas;
        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        var inputMeshes = new List<Mesh>();
        int vertexStart = 0;
        int triangleStart = 0;
        int meshCount = _width * _height * _depth;
        int m = 0;
        var jobs = new ProcessMeshDataJob {
            vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
            triangleStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
        };
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                for (int z = 0; z < _depth; z++) {
                    _blocks[x, y, z] = new Block(new Vector3(x, y, z) + _location, BlockDetails.GetItemByID(_blocksData[x + _width * (y + _depth * z)]), this);
                    if (ReferenceEquals(_blocks[x, y, z].Mesh, null)) continue;
                    inputMeshes.Add(_blocks[x, y, z].Mesh);
                    var vCount = _blocks[x, y, z].Mesh.vertexCount;
                    var triCount = (int)_blocks[x, y, z].Mesh.GetIndexCount(0);
                    jobs.vertexStart[m] = vertexStart;
                    jobs.triangleStart[m] = triangleStart;
                    vertexStart += vCount;
                    triangleStart += triCount;
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
        var newMesh = new Mesh {
            name = "Chunk_" + _location.x + "_" + _location.y + "_" + _location.z
        };
        var sm = new SubMeshDescriptor(0, triangleStart) {
            firstVertex = 0,
            vertexCount = vertexStart
        };

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

    public void SetBlocks((Vector3Int position, int id)[] blocks) {
        var mf = gameObject.GetComponent<MeshFilter>();
        var mr = gameObject.GetComponent<MeshRenderer>();
        var mc = gameObject.GetComponent<MeshCollider>();
        DestroyImmediate(mf);
        DestroyImmediate(mr);
        DestroyImmediate(mc);
        foreach(var block in blocks) {
            int index = block.position.x + _width * (block.position.y + _depth * block.position.z);
            _blocksData[index] = block.id;
        }
        
        RenderChunk();
    }
}