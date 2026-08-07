using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleLogic; // B)

public class EncountersOhNo : DungeonBlockAnchorObject
{
    public List<CharAttr> enemies;
    // Start is called before the first frame update
    public void Init(EncounterTable.Entry entry)
    {
        enemies = entry.enemies;
    }
}
