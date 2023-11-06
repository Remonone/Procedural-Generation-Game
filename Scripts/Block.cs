using System;
using UnityEngine;

namespace DefaultNamespace {
    public class Block : MonoBehaviour {
        
        [SerializeField] private BlockSide side;
        [SerializeField] private Vector3 _offset;
        
        [Serializable]
        public enum BlockSide {
            BOTTOM,
            TOP,
            LEFT,
            RIGHT,
            FRONT,
            BACK
        };
        
        private void Start() {
            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();

            Quad q = new Quad();
            mf.mesh = q.BuildMesh(side, _offset);
        }
    }
}
