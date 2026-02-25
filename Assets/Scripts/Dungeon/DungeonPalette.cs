using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using System.Linq;
using BattleLogic;

[CreateAssetMenu(fileName = "DungeonPalette", menuName = "Dungeon Generation/Dungeon Palette")]
public class DungeonPalette : ScriptableObject
{
    [Serializable]
    public class BlockMeta
    {
        public DungeonBlock block;
        public float weight;
        public float weightPerDepth;

        public float getInContext(DungeonGenerator dungeonGenerator, DungeonBlock dungeonBlock) 
        {
            int age = dungeonGenerator.blockAges[dungeonBlock];
            int depth = dungeonGenerator.depth;
            float finalWeight = Mathf.Max(weight + (weightPerDepth * age), 0);
            if (age >= depth - 1) 
            {
                return block.anchors.Count(a => !a.anchorObject.isWall) == 1 ? finalWeight : 0;
            }
            return finalWeight;
        }

    }
    public List<BlockMeta> blockMetas;

    public DungeonBlock GetNextBlock(BattleRandom randomIN, out BattleRandom randomOUT, DungeonGenerator dungeonGenerator, DungeonBlock dungeonBlock, List<BlockMeta> bMetas = null) 
    {
        bMetas ??= blockMetas;
        randomOUT = randomIN.NextRandom(0, bMetas.Sum(b => b.getInContext(dungeonGenerator, dungeonBlock)), out float value);
        int index = 0;
        while (value > 0) 
        {
            value -= bMetas[index].getInContext(dungeonGenerator, dungeonBlock);
            if (value <= 0) 
            {
                return bMetas[index].block;
            }
            index++;
        }
        return bMetas.Last().block;
    }
    public void BakeBlocks() 
    {
        foreach (var bm in blockMetas)
        {
            bm.block.Setup();
        }
        Debug.Log("Blocks backed-I mean baked");
    }
}
