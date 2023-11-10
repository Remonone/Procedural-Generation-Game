using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace DefaultNamespace {
    [CreateAssetMenu(fileName = "Block", menuName = "Blocks/Create New Block", order = 0)]
    public class BlockDetails : ScriptableObject {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private List<Side> _sides  = new(6);
        [SerializeField] private bool _isSolid;

        private static Dictionary<int, BlockDetails> _blockStore;

        public int ID => _id;
        public string Name => _name;
        public List<Side> Sides => _sides;
        public bool IsSolid => _isSolid;

        public static BlockDetails GetItemByID(int id) {
            if (_blockStore == null) {
                _blockStore = new Dictionary<int, BlockDetails>();
                LoadStore();
            }
            if (!_blockStore.ContainsKey(id)) return null;
            return _blockStore[id];
        }

        private static void LoadStore() {
            var itemList = Resources.LoadAll<BlockDetails>("Blocks");
            foreach (var block in itemList) {
                if (_blockStore.ContainsKey(block.ID)) {
                    Debug.LogError(string.Format("There's a duplicate for blcoks: {0} and {1}", _blockStore[block.ID], block));
                    continue;
                }
                _blockStore[block.ID] = block;
            }
        }
    }

    [Serializable]
    public sealed class Side {
        public MeshUtils.BlockSide side;
        public List<Vector2> uv = new List<Vector2>(4);
    }
}
