using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;

public class Player : MonoBehaviour {
    [SerializeField] private float _hitDistance;
    [SerializeField] private InputActionMap _map;

    private void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable() {
        _map.Enable();
    }

    private void OnDisable() {
        _map.Disable();
    }

    private void Update() {
        if (_map["Attack"].WasPerformedThisFrame()) {
            Ray ray = Camera.main.ScreenPointToRay(_map["Position"].ReadValue<Vector2>());
            if (Physics.Raycast(ray, out var hit, _hitDistance)) {
                var hitBlock = hit.point - hit.normal / 2f;
                var chunk = hit.collider.gameObject.GetComponent<Chunk>();
                int blockX = (int)(Mathf.Round(hitBlock.x) - chunk.Location.x);
                int blockY = (int)(Mathf.Round(hitBlock.y) - chunk.Location.y);
                int blockZ = (int)(Mathf.Round(hitBlock.z) - chunk.Location.z);
                var blocks = new(Vector3Int position, int id)[1];
                blocks[0].position = new Vector3Int(blockX, blockY, blockZ);
                blocks[0].id = 0;
                chunk.SetBlocks(blocks);
            }
        }
    }
}
