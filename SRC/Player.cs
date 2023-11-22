using UI.Documents;
using UnityEngine;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;

public class Player : MonoBehaviour {
    [SerializeField] private float _hitDistance;
    [SerializeField] private InputActionMap _map;
    [SerializeField] private SelectionHUD _hud;

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
            if (RayToPoint(out var hit)) {
                var hitBlock = hit.point - hit.normal / 2f;
                var chunk = hit.collider.gameObject.GetComponent<Chunk>();
                ChangeBlock(hitBlock, chunk, 0);
            }
        }

        if (_map["Action"].WasPerformedThisFrame()) {
            if (RayToPoint(out var hit)) {
                var hitBlock = hit.point + hit.normal / 2.0f;
                var chunk = hit.collider.gameObject.GetComponent<Chunk>();
                ChangeBlock(hitBlock, chunk, _hud.ActiveBlock.ID);
            }
        }

        if (_map["Scroll"].WasPerformedThisFrame()) {
            float yAxis = _map["Scroll"].ReadValue<Vector2>().y;
            _hud.ShiftActiveSlot(yAxis > 0);
        }
    }

    private bool RayToPoint(out RaycastHit hit) {
        Ray ray = Camera.main.ScreenPointToRay(_map["Position"].ReadValue<Vector2>());
        return Physics.Raycast(ray, out hit, _hitDistance);
    }

    private void ChangeBlock(Vector3 position, Chunk chunk, int id) {
        int blockX = (int)(Mathf.Round(position.x) - chunk.Location.x);
        int blockY = (int)(Mathf.Round(position.y) - chunk.Location.y);
        int blockZ = (int)(Mathf.Round(position.z) - chunk.Location.z);
        var blocks = new(Vector3Int position, int id)[1];
        blocks[0].position = new Vector3Int(blockX, blockY, blockZ);
        blocks[0].id = id;
        chunk.SetBlocks(blocks);
    }
}
