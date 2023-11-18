using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class Block {

    private readonly Chunk _parentChunk;
    public Mesh Mesh { get; }

    private static readonly List<Vector3> Neighbors = new() { // Try to get rid from dependency here...
        Vector3.down, Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };
    
    
    public Block(Vector3 offset, BlockDetails block, Chunk chunk) {
        if (block.ID == 0) return;
        List<Quad> quads = new();
        _parentChunk = chunk;
        var localPosition = offset - chunk.Location;
        var defaultSide = block.Sides[0];
        for (var i = 0; i < 6; i++) {
            var operateSide = block.Sides.SingleOrDefault(details => details.side == (MeshUtils.BlockSide)i);
            operateSide ??= defaultSide;
            var neighbor = Neighbors[i];
            if(IsSolidBlock(localPosition + neighbor)) continue;
            
            quads.Add(new Quad((MeshUtils.BlockSide)i, offset, operateSide));
        }

        if (quads.Count < 1) return;

        Mesh = MeshUtils.MergeMeshes(quads.Select(quad => quad.Mesh).ToArray());
    }

    private bool IsSolidBlock(Vector3 pos) {
        var x = (int)pos.x;
        var y = (int)pos.y;
        var z = (int)pos.z;
        if (x < 0 || x >= _parentChunk.Width || y < 0 || y >= _parentChunk.Height || z < 0 ||
            z >= _parentChunk.Depth) return false;
        return BlockDetails.GetItemByID(_parentChunk.BlockData[x + _parentChunk.Width * (y + _parentChunk.Depth * z)]).IsSolid;
    }

}
