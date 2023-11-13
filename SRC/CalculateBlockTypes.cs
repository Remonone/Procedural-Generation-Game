using DefaultNamespace;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utils;

[BurstCompile]
public struct CalculateBlockTypes : IJobParallelFor {
        public NativeArray<int> cData;
        public int width;
        public int height;
        public Vector3Int location;
        public PerlinSettings caveSettings;
        public Unity.Mathematics.Random random;
        [NativeDisableParallelForRestriction] public NativeArray<BlockDetails.BlockDataChunk> dataChunks;

 
        public void Execute(int i) {
                int x = i % width + location.x;
                int y = (i / width) % height + location.y;
                int z = i / (width * height) + location.z; 
                random = new Unity.Mathematics.Random(2);
                int ID = 0;
                foreach (var block in dataChunks) {
                        PerlinSettings bottom = block.Bottom;
                        PerlinSettings top = block.Top;
                        float lowValue = MeshUtils.fBM(x, z, bottom.Scale, bottom.HeightScale, bottom.Octaves, bottom.HeightOffset);
                        float topValue = MeshUtils.fBM(x, z, top.Scale, top.HeightScale, top.Octaves, top.HeightOffset);
                        if (lowValue <= y && y <= topValue && random.NextFloat(0f, 1f) <= bottom.Probability) {
                                ID = block.ID;
                                break;
                        }
                }
                
                var prob = MeshUtils.fBM3D(x, y, z, caveSettings.Scale, caveSettings.HeightScale, caveSettings.Octaves, caveSettings.HeightOffset);
                if (prob >= caveSettings.Probability && ID != PropertyConstant.BOTTOM_BLOCK_ID) ID = 0;
                cData[i] = ID;
        }

}