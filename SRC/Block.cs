using System;
using System.Linq;
using UnityEngine;
using Utils;

namespace DefaultNamespace {
    public class Block : MonoBehaviour {
        
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Material _atlas;
        
        [Serializable]
        public enum BlockSide {
            BOTTOM = 0,
            TOP = 1,
            LEFT = 2,
            RIGHT = 3,
            FRONT = 4,
            BACK = 5
        };
        
        private void Start() {
            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
            mr.material = _atlas;

            Quad[] quads = new Quad[6];
            for (var i = 0; i < 6; i++) {
                quads[i] = new Quad((BlockSide)i, _offset);
            }

            var mesh = MeshUtils.MergeMeshes(quads.Select(quad => quad.Mesh).ToArray());
            mf.mesh = mesh;
        }
    }
}
