using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Utils;

public class Chunk : MonoBehaviour {
    [SerializeField] private Material _atlas;

    [SerializeField] private int _width = 16;
    [SerializeField] private int _depth = 16;
    [SerializeField] private int _height = 16;

    private Block[,,] _blocks;
    private BlockDetails[] _blocksData;
    private Vector3Int _location;
    private MeshRenderer _renderer;

    public BlockDetails[] BlockData => _blocksData;
    public int Width => _width;
    public int Height => _height;
    public int Depth => _depth;
    public Vector3 Location => _location;
    public MeshRenderer Renderer => _renderer;
    
    
    
    private void BuildChunk(World parent) {
        var blockCount = _width * _height * _depth;
        _blocksData = new BlockDetails[blockCount];
        var airBlock = BlockDetails.GetItemByID(0);
        for (int i = 0; i < blockCount; i++) {
            int x = i % _width + _location.x;
            int y = (i / _width) % _height + _location.y;
            int z = i / (_width * _height) + _location.z;
            var block = parent.GenerateBlockByCoordinate(new Vector3Int(x, y, z));
            var caveProb = MeshUtils.fBM3D(x, y, z, parent.CaveSettings.Scale, parent.CaveSettings.HeightScale,
                parent.CaveSettings.Octaves, parent.CaveSettings.HeightOffset);
            if (ReferenceEquals(block, null) || caveProb >= parent.CaveSettings.Probability && block.Name != PropertyConstant.BOTTOM_BLOCK) block = airBlock;
            _blocksData[i] = block ? block : airBlock;
        }
    }

    

    public void CreateChunk(Vector3Int dimension, Vector3Int position, World parent) {
        _location = position;
        _width = dimension.x;
        _height = dimension.y;
        _depth = dimension.z;
        
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        _renderer = gameObject.AddComponent<MeshRenderer>();
        MeshCollider mc = gameObject.AddComponent<MeshCollider>();

        _renderer.material = _atlas;
        _blocks = new Block[_width, _height, _depth];
        
        BuildChunk(parent);

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