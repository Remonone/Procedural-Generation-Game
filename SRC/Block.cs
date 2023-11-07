using System;
using System.Linq;
using UnityEngine;
using Utils;

public class Block {

    private Mesh _mesh;

    public Mesh Mesh => _mesh;
    
    
    public Block(Vector3 offset) {
        Quad[] quads = new Quad[6];
        for (var i = 0; i < 6; i++) {
            quads[i] = new Quad((MeshUtils.BlockSide)i, offset);
        }

        _mesh = MeshUtils.MergeMeshes(quads.Select(quad => quad.Mesh).ToArray());
    }
}
