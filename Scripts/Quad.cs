using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class Quad {

    private static Dictionary<string, Vector2> _uvs = new() {
        {"uv00", new Vector2(0, 0)},
        {"uv01", new Vector2(0, 1)},
        {"uv10", new Vector2(1, 0)},
        {"uv11", new Vector2(1, 1)}
    };

    private static Dictionary<string, Vector3> _vertices = new() {
        { "p0", new Vector3(-0.5f, -0.5f, 0.5f) },
        { "p1", new Vector3(0.5f, -0.5f, 0.5f) },
        { "p2", new Vector3(0.5f, -0.5f, -0.5f) },
        { "p3", new Vector3(-0.5f, -0.5f, -0.5f) },
        { "p4", new Vector3(-0.5f, 0.5f, 0.5f) },
        { "p5", new Vector3(0.5f, 0.5f, 0.5f) },
        { "p6", new Vector3(0.5f, 0.5f, -0.5f) },
        { "p7", new Vector3(-0.5f, 0.5f, -0.5f) }
    };

    private Dictionary<Block.BlockSide, (Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles)> _sides = new() {
            {
                Block.BlockSide.FRONT, 
                (new []{_vertices["p4"], _vertices["p5"], _vertices["p1"], _vertices["p0"]}, 
                new [] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward }, 
                new []{_uvs["uv11"], _uvs["uv01"], _uvs["uv00"], _uvs["uv00"]},
                new []{3, 1, 0, 3, 2, 1})
            },
            {
                Block.BlockSide.BACK,
                (new []{_vertices["p6"], _vertices["p7"], _vertices["p3"], _vertices["p2"]}, 
                new []{Vector3.back, Vector3.back, Vector3.back, Vector3.back},
                new []{_uvs["uv11"], _uvs["uv01"], _uvs["uv00"], _uvs["uv00"]},
                new []{3, 1, 0, 3, 2, 1})
            },
            {
                Block.BlockSide.LEFT,
                (new []{_vertices["p5"], _vertices["p6"], _vertices["p2"], _vertices["p1"]},
                new []{Vector3.left, Vector3.left, Vector3.left, Vector3.left},
                new [] {_uvs["uv11"], _uvs["uv01"], _uvs["uv00"], _uvs["uv00"]},
                new [] {3, 1, 0, 3, 2, 1})
            },
            {
                Block.BlockSide.RIGHT,
                (new []{_vertices["p7"], _vertices["p4"], _vertices["p0"], _vertices["p3"]},
                new []{Vector3.right, Vector3.right, Vector3.right, Vector3.right},
                new [] {_uvs["uv11"], _uvs["uv01"], _uvs["uv00"], _uvs["uv00"]},
                new [] {3, 1, 0, 3, 2, 1})
            },
            {
                Block.BlockSide.TOP,
                (new []{_vertices["p7"], _vertices["p6"], _vertices["p5"], _vertices["p4"]},
                new []{Vector3.up, Vector3.up, Vector3.up, Vector3.up},
                new [] {_uvs["uv11"], _uvs["uv01"], _uvs["uv00"], _uvs["uv00"]},
                new [] {3, 1, 0, 3, 2, 1})
            },
            {
                Block.BlockSide.BOTTOM,
                (new []{_vertices["p0"], _vertices["p1"], _vertices["p2"], _vertices["p3"]},
                new []{Vector3.down, Vector3.down, Vector3.down, Vector3.down},
                new [] {_uvs["uv11"], _uvs["uv01"], _uvs["uv00"], _uvs["uv00"]},
                new [] {3, 1, 0, 3, 2, 1})
            }
        };
    
    public Mesh BuildMesh(Block.BlockSide side, Vector3 offset) {
        Mesh mesh = new Mesh { name = "Custom Quad" };
       
        mesh.vertices = _sides[side].vertices.Select(vertex => vertex + offset).ToArray();
        mesh.normals = _sides[side].normals;
        mesh.uv = _sides[side].uvs;
        mesh.triangles = _sides[side].triangles;

        mesh.RecalculateBounds();
        
        return mesh;
    }

}
