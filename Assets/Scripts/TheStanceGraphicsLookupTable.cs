using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle Logic/The Stance Graphics Lookup Table (TM)")]

public class TheStanceGraphicsLookupTable : ScriptableObject
{
    public List<Color> stanceColors;
    public List<Sprite> stanceImages;

    public Color GetColor(BattleLogic.Stance stance)
    {
        return stanceColors[(int)stance];
    }
    public Sprite GetImage(BattleLogic.Stance stance)
    {
        return stanceImages[(int)stance];
    }
}
