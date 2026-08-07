using BattleLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EncounterTable", menuName = "Dungeon Generation/EncounterTable")]
public class EncounterTable : ScriptableObject
{
    [Serializable]
    public class Entry 
    {
        public List<CharAttr> enemies;
        public float weight;
        public float weightPerSegment;

        public float getInContext(DungeonGenerator dungeonGenerator)
        {
            int segment = dungeonGenerator.segment;
            float finalWeight = Mathf.Max(weight + (weightPerSegment * segment), 0);
            return finalWeight;
        }

    }
    public List<Entry> entries;

    public Entry GetNextEncounter(BattleRandom randomIN, out BattleRandom randomOUT, DungeonGenerator dungeonGenerator, List<Entry> entries = null)
    {
        entries ??= this.entries;
        randomOUT = randomIN.NextRandom(0, entries.Sum(b => b.getInContext(dungeonGenerator)), out float value);
        int index = 0;
        while (value > 0)
        {
            value -= entries[index].getInContext(dungeonGenerator);
            if (value <= 0)
            {
                return entries[index];
            }
            index++;
        }
        if (entries.Count > 0)
        {
            return entries.First();
        }
        throw new ArgumentOutOfRangeException("Nothing in entries (after failsafe). Entries: " + string.Join("\n", entries.Select(entry => string.Join(", ", entry.enemies.Select(e => e.name)))));
    }
}
