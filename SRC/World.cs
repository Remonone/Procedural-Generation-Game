using System;
using System.Collections;
using UnityEngine;

public class World : MonoBehaviour {

    [SerializeField] private GameObject _chunkPrefab;

    private static Vector3 _worldDimensions = new Vector3(10, 10, 10);
    private static Vector3 _chunkDimensions = new Vector3(16, 16, 16);

    private void Start() {
        StartCoroutine(BuildWorld());
    }

    IEnumerator BuildWorld() {
        for (int x = 0; x < _worldDimensions.x; x++) {
            for (int y = 0; y < _worldDimensions.y; y++) {
                for (int z = 0; z < _worldDimensions.z; z++) {
                    var chunk = Instantiate(_chunkPrefab);
                    var pos = new Vector3(_chunkDimensions.x * x, _chunkDimensions.y * y, _chunkDimensions.z * z);
                    chunk.GetComponent<Chunk>().CreateChunk(_chunkDimensions, pos);
                    yield return null;
                }
            }
        }
    }
}