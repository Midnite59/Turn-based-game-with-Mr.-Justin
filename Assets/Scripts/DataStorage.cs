using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataStorage
{
    [Flags]
    public enum Zones {
        None = 0, // 0
        Up = 1 << 0, // 1 
        Up2 = 1 << 1, // 10
        Down = 1 << 2,  // 100
        Down2 = 1 << 3, // 1000
        Left = 1 << 4, // ...
        Left2 = 1 << 5,
        Left3 = 1 << 6,
        Right = 1 << 7,
    };
    [Flags]
    public enum CharacterFlags
    {
        None = 0, // 0
        Him = 1 << 0, // 1 
        Her = 1 << 1, // 10
        ThatOne = 1 << 2,  // 100
        RegalTom = 1 << 3, // 1000
        Norman = 1 << 4, // ...
        ObscureReferenceGuy = 1 << 5,
        NotImportant = 1 << 6,
        ThatOneEdgyDudeWhoDiesIn1stAct = 1 << 7,
    };

    [Serializable]
    public class PermData
    {
        public RunData currentRun;
        public Zones unlockedZones;
        public CharacterFlags characterFlags;
    }
    [Serializable]
    public class CharData
    {
        public CharAttr attr;
        public int exp;
        public int hp;
    }
    [Serializable]
    public class RunData
    {
        public List<CharData> characters;
    }
}
