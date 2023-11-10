using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Utils;

public class Quad {

    private Mesh _mesh;

    public Mesh Mesh => _mesh;

    public Quad(MeshUtils.BlockSide side, Vector3 offset, Side details) {
        _mesh = new Mesh { name = "Custom Quad" };
        _mesh.vertices = _sides[side].vertices.Select(vertex => vertex + offset).ToArray();
        _mesh.normals = _sides[side].normals;
        _mesh.uv = new []{details.uv[3], details.uv[2], details.uv[0], details.uv[1]};
        _mesh.triangles = _sides[side].triangles;

        _mesh.RecalculateBounds();
        
    }
    
    
    private static Dictionary<MeshUtils.BlockSide, (Vector3[] vertices, Vector3[] normals, int[] triangles)> _sides = new() {
            {
                MeshUtils.BlockSide.FRONT, 
                (new []{MeshUtils.Vertices["p4"], MeshUtils.Vertices["p5"], MeshUtils.Vertices["p1"], MeshUtils.Vertices["p0"]}, 
                new [] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward }, 
                new []{3, 1, 0, 3, 2, 1})
            },
            {
                MeshUtils.BlockSide.BACK,
                (new []{MeshUtils.Vertices["p6"], MeshUtils.Vertices["p7"], MeshUtils.Vertices["p3"], MeshUtils.Vertices["p2"]}, 
                new []{Vector3.back, Vector3.back, Vector3.back, Vector3.back},
                new []{3, 1, 0, 3, 2, 1})
            },
            {
                MeshUtils.BlockSide.LEFT,
                (new []{MeshUtils.Vertices["p5"], MeshUtils.Vertices["p6"], MeshUtils.Vertices["p2"], MeshUtils.Vertices["p1"]},
                new []{Vector3.left, Vector3.left, Vector3.left, Vector3.left},
                new [] {3, 1, 0, 3, 2, 1})
            },
            {
                MeshUtils.BlockSide.RIGHT,
                (new []{MeshUtils.Vertices["p7"], MeshUtils.Vertices["p4"], MeshUtils.Vertices["p0"], MeshUtils.Vertices["p3"]},
                new []{Vector3.right, Vector3.right, Vector3.right, Vector3.right},
                new [] {3, 1, 0, 3, 2, 1})
            },
            {
                MeshUtils.BlockSide.TOP,
                (new []{MeshUtils.Vertices["p7"], MeshUtils.Vertices["p6"], MeshUtils.Vertices["p5"], MeshUtils.Vertices["p4"]},
                new []{Vector3.up, Vector3.up, Vector3.up, Vector3.up},
                new [] {3, 1, 0, 3, 2, 1})
            },
            {
                MeshUtils.BlockSide.BOTTOM,
                (new []{MeshUtils.Vertices["p0"], MeshUtils.Vertices["p1"], MeshUtils.Vertices["p2"], MeshUtils.Vertices["p3"]},
                new []{Vector3.down, Vector3.down, Vector3.down, Vector3.down},
                new [] {3, 1, 0, 3, 2, 1})
            }
        };

}
