using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;


[CreateAssetMenu(fileName = "Block", menuName = "Blocks/Create New Block", order = 0)]
public class BlockDetails : ScriptableObject {
    [SerializeField] private int _id;
    [SerializeField] private string _name;
    [SerializeField] private List<Side> _sides  = new(6);
    [SerializeField] private bool _isSolid;
    [SerializeField] private GenerationLevel _levels;
    [SerializeField] private Sprite _sprite;

    private static Dictionary<int, BlockDetails> _blockStore;
    private static Dictionary<int, BlockDataChunk> _dataChunks;

    public int ID => _id;
    public string Name => _name;
    public List<Side> Sides => _sides;
    public bool IsSolid => _isSolid;
    public GenerationLevel Layers => _levels;
    public static BlockDataChunk[] DataChunks => _dataChunks.Values.ToArray();
    public Sprite Sprite => _sprite;

    public static BlockDetails GetItemByID(int id) {
        if (_blockStore == null) {
            _blockStore = new Dictionary<int, BlockDetails>();
            _dataChunks = new Dictionary<int, BlockDataChunk>();
            LoadStore();
        }
        if (!_blockStore.ContainsKey(id)) return null;
        return _blockStore[id];
    }

    private static void LoadStore() {
        var itemList = Resources.LoadAll<BlockDetails>("Blocks");
        foreach (var block in itemList) {
            if (_blockStore.ContainsKey(block.ID)) {
                Debug.LogError($"There's a duplicate for blocks: {_blockStore[block.ID]} and {block}");
                continue;
            }
            _blockStore[block.ID] = block;

            InitBlock(block);
        }
    }

    private static void InitBlock(BlockDetails details) {
        World.RegisterBlock(details.ID, details);
        _dataChunks.Add(details.ID,
            new BlockDataChunk {
                bottom = details._levels.lowLevel,
                top = details._levels.topLevel, id = details._id
            });
    }

    public struct BlockDataChunk {
        public int id;
        public PerlinSettings top;
        public PerlinSettings bottom;
    }
}

[Serializable]
public sealed class Side {
    public MeshUtils.BlockSide side;
    public List<Vector2> uv = new(4);
}

[Serializable]
public sealed class GenerationLevel {
    public PerlinSettings topLevel;
    public PerlinSettings lowLevel;
}