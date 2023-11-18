using UnityEngine;
using Utils;

[ExecuteAlways]
public class PerlinGrapher : MonoBehaviour {

    private readonly Vector3 _dimensions = new(10, 10, 10);
    [SerializeField] private float heightScale = 2f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float scale = .5f;

    [SerializeField] private int _octaves = 1;
    [SerializeField] private float _heightOffset = 1;
    [Range(0.0f, 10.0f)]
    [SerializeField] private float DrawCutOff = 1f;

    void CreateCubes() {
        for (int x = 0; x < _dimensions.x; x++) {
            for (int y = 0; y < _dimensions.y; y++) {
                for (int z = 0; z < _dimensions.z; z++) {
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.name = "perlin_cube";
                    obj.transform.parent = transform;
                    obj.transform.position = new Vector3(x, y, z);
                }
            }
        }
    }

    void Graph() {
        MeshRenderer[] cubes = GetComponentsInChildren<MeshRenderer>();
        if(cubes.Length == 0) CreateCubes();

        if (cubes.Length == 0) return;
        for (int x = 0; x < _dimensions.x; x++) {
            for (int y = 0; y < _dimensions.y; y++) {
                for (int z = 0; z < _dimensions.z; z++) {
                    var p3D = MeshUtils.fBM3D(x, y, z, scale, heightScale, _octaves, _heightOffset);
                    cubes[x + (int)_dimensions.x * (y + (int)_dimensions.y * z)].enabled = p3D < DrawCutOff;
                }
            }
        }
    }

    private void OnValidate() {
        Graph();
    }
}