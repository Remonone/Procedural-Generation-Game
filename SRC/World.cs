using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Utils;

public class World : MonoBehaviour {

    [SerializeField] private GameObject _chunkPrefab;

    private static Vector3 _worldDimensions = new(10, 10, 10);
    private static Vector3 _chunkDimensions = new(16, 16, 16);
    private static List<BlockDetails> _blocks = new();


    [SerializeField] private PerlinSettings _surfaceSettings;
    [SerializeField] private PerlinSettings _stoneSettings;

    public PerlinSettings SurfaceSettings => _surfaceSettings;
    public PerlinSettings StoneSettings => _stoneSettings;
    public List<BlockDetails> Blocks => _blocks;


    private void Start() {
        BlockDetails.GetItemByID(0); // Should change
        StartCoroutine(BuildWorld());
    }

    IEnumerator BuildWorld() {
        for (int x = 0; x < _worldDimensions.x; x++) {
            for (int y = 0; y < _worldDimensions.y; y++) {
                for (int z = 0; z < _worldDimensions.z; z++) {
                    var chunk = Instantiate(_chunkPrefab);
                    var pos = new Vector3(_chunkDimensions.x * x, _chunkDimensions.y * y, _chunkDimensions.z * z);
                    chunk.GetComponent<Chunk>().CreateChunk(_chunkDimensions, pos, this);
                    yield return null;
                }
            }
        }
    }

    public static void RegisterBlock(BlockDetails details) {
        _blocks.Add(details);
    }

    public List<BlockDetails> BlocksByLayerIntersection(Vector3Int coordinate) { // BUG
        List<BlockDetails> toReturn = (from block in _blocks 
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