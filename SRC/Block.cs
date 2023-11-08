using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class Block {

    private Mesh _mesh;
    private Chunk _parentChunk;

    public Mesh Mesh => _mesh;

    private static List<(int, int, int)> _neighbors = new() { // Try to get rid from dependency here...
        (0, -1, 0), (0, 1, 0), (1, 0, 0), (-1, 0, 0), (0, 0, 1), (0, 0, -1)
    };
    
    
    public Block(Vector3 offset, MeshUtils.BlockType type, Chunk chunk) {
        if (type is MeshUtils.BlockType.AIR) return;
        List<Quad> quads = new();
        _parentChunk = chunk;
        for (var i = 0; i < 6; i++) {
            var neigbors = _neighbors[i];
            if(IsTransparentNeighbor((int)offset.x + neigbors.Item1, (int)offset.y + neigbors.Item2, (int)offset.z + neigbors.Item3))
                quads.Add(new Quad((MeshUtils.BlockSide)i, offset, type));
        }

        if (quads.Count < 1) return;

        _mesh = MeshUtils.MergeMeshes(quads.Select(quad => quad.Mesh).ToArray());
    }

    public bool IsTransparentNeighbor(int x, int y, int z) {
        if (x < 0 || x >= _parentChunk.Width || y < 0 || y >= _parentChunk.Height || z < 0 ||
            z >= _parentChunk.Depth) return true;
        
        var block = _parentChunk.BlockData[x + _parentChunk.Width * (y + _parentChunk.Depth * z)];
        return block is MeshUtils.BlockType.AIR or MeshUtils.BlockType.WATER;
    }

}
