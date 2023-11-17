using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Profiling;
using Utils;

public class World : MonoBehaviour {

    [SerializeField] private GameObject _chunkPrefab;
    [SerializeField] private PerlinSettings _caveSettings;
    [SerializeField] private GameObject _player;
    [SerializeField] private Camera _loadingCamera;
    [SerializeField] private int _renderDistance = 4;


    private static Vector3Int _worldDimensions = new(5, 5, 5);
    private static Vector3Int _chunkDimensions = new(16, 16, 16);
    
    private static Dictionary<int, BlockDetails> _blocks = new();
    private static WaitForSeconds _wfs = new(.3f);
    
    public PerlinSettings CaveSettings => _caveSettings;
    
    private HashSet<Vector3Int> _chunkChecker = new();
    private HashSet<Vector2Int> _chunkColumns = new();
    private Dictionary<Vector3Int, Chunk> _chunks = new();

    private Vector3Int _lastBuildPosition;

    private Queue<IEnumerator> _buildQueue = new();

    private List<Vector2Int> _spiralList = new() {
        new Vector2Int(_chunkDimensions.x, 0), 
        new Vector2Int(0, _chunkDimensions.z), 
        new Vector2Int(-_chunkDimensions.x, 0), 
        new Vector2Int(0, -_chunkDimensions.z)
    };

    private IEnumerator BuildCoordinator() {
        while (true) {
            while (_buildQueue.Count > 0)
                yield return StartCoroutine(_buildQueue.Dequeue());
            yield return null;
        }
    }

    private IEnumerator UpdateWorld() {
        while (true) {
            var playerPos = _player.transform.position;
            if ((_lastBuildPosition - playerPos).magnitude > _chunkDimensions.x) {
                _lastBuildPosition = Vector3Int.CeilToInt(playerPos);
                int posX = (int)(playerPos.x / _chunkDimensions.x) * _chunkDimensions.x;
                int posZ = (int)(playerPos.z / _chunkDimensions.z) * _chunkDimensions.z;
                _buildQueue.Enqueue(BuildRecursiveWorld(posX, posZ, _renderDistance));
                _buildQueue.Enqueue(HideColumns(posX, posZ));
            }
            yield return _wfs;
        }
    }


    private IEnumerator BuildRecursiveWorld(int x, int z, int r) {
        int nextrad = r - 1;
        if(r <= 0) yield break;
        foreach (var el in _spiralList) {
            BuildChunkColumn(x + el.x, z + el.y);
            _buildQueue.Enqueue(BuildRecursiveWorld(x + el.x, z + el.y, nextrad));
            yield return null;
        }
    }

    public void HideChunkColumn(int x, int z) {
        for (int y = 0; y < _worldDimensions.y; y++) {
            Vector3Int pos = new(x, y * _chunkDimensions.y, z);
            if (_chunkChecker.Contains(pos)) {
                _chunks[pos].Renderer.enabled = false;
            }
        }
    }

    private IEnumerator HideColumns(int x, int z) {
        Vector2Int playerPos = new Vector2Int(x, z);
        foreach (Vector2Int cc in _chunkColumns) {
            if((cc - playerPos).magnitude >= _renderDistance * _chunkDimensions.x)
                HideChunkColumn(cc.x, cc.y);
        }

        yield return null;
    }

    private void Start() {
        BlockDetails.GetItemByID(0);
        StartCoroutine(BuildWorld());
    }

    private void BuildChunkColumn(int x, int z) {
        for (int y = 0; y < _worldDimensions.y; y++) {
            var pos = new Vector3Int(x, _chunkDimensions.y * y, z);
            if (!_chunks.ContainsKey(pos)) {
                var chunk = Instantiate(_chunkPrefab);
                var ch = chunk.GetComponent<Chunk>();
                ch.CreateChunk(_chunkDimensions, pos, this);
                _chunkChecker.Add(pos);
                _chunkColumns.Add(new Vector2Int(x, z));
                _chunks.Add(pos, ch); 
            }
            else {
                _chunks[pos].Renderer.enabled = true;
            }
            
        }
    }

    private IEnumerator BuildWorld() {
        for (int x = 0; x < _worldDimensions.x; x++) {
            for (int z = 0; z < _worldDimensions.z; z++) {
                BuildChunkColumn(_chunkDimensions.x * x, _chunkDimensions.z * z);
                yield return null;
            }
        }
        int xpos = _worldDimensions.x * _chunkDimensions.x / 2;
        int zpos = _worldDimensions.z * _chunkDimensions.z / 2;
        int ypos = GetHighestPointByPosition(xpos, zpos);
        _loadingCamera.gameObject.SetActive(false);
        _player.transform.position = new Vector3(xpos, ypos, zpos);
        _player.SetActive(true);

        _lastBuildPosition = Vector3Int.CeilToInt(_player.transform.position);

        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());
    }

    public static void RegisterBlock(int id, BlockDetails details) {
        _blocks.Add(id, details);
    }
    
    public BlockDetails GenerateBlockByCoordinate(Vector3Int coord) {
        var blocks = BlocksByLayerIntersection(coord);
        var block = blocks.FirstOrDefault(
            block => UnityEngine.Random.Range(0f, 1f) <= block.Layers.lowLevel.Probability);
        block ??= BlockDetails.GetItemByID(0);
        return block;
    }
    
    public int GetHighestPointByPosition(int x, int z) {
        BlockDetails block;
        for (int y = PropertyConstant.HEIGHT_LIMIT; y > 0; y--) {
            block = GenerateBlockByCoordinate(new Vector3Int(x, y, z));
            if (block != null) return y + 1;
        }

        return PropertyConstant.HEIGHT_LIMIT;
    }

    
    public List<BlockDetails> BlocksByLayerIntersection(Vector3Int coordinate) {
        List<BlockDetails> toReturn = (from block in _blocks.Values 
            let low = block.Layers.lowLevel 
            let high = block.Layers.topLevel 
            let lowValue = MeshUtils.fBM(coordinate.x, coordinate.z, low.Scale, low.HeightScale, low.Octaves, low.HeightOffset) 
            let topValue = MeshUtils.fBM(coordinate.x, coordinate.z, high.Scale, high.HeightScale, high.Octaves, high.HeightOffset) 
            where lowValue <= coordinate.y && coordinate.y <= topValue select block).ToList();
        return toReturn;
    }

    
    
}

[Serializable]
public struct PerlinSettings {
    public float HeightScale;
    public float Scale;
    public int Octaves;
    public float HeightOffset;
    public float Probability;

    public PerlinSettings(float heightScale, float scale, int octaves, float heightOffset, float probability) {
        HeightScale = heightScale;
        Scale = scale;
        Octaves = octaves;
        HeightOffset = heightOffset;
        Probability = probability;
    }
}