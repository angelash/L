using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mogo.GameData
{
    public partial class GlobalData : GameData<GlobalData>
    {
        public static readonly string fileName = "xml/GlobalData";
        public Dictionary<int, int> InitBuilding { get; protected set; }
        public Dictionary<int, int> InitSoldier { get; protected set; }
        public int BattleTime { get; protected set; }
        public float CombatFrequeny { get; protected set; }
        public float BuildingReversionFrequency { get; protected set; }
        public int BuildingReversionEnergy { get; protected set; }
        public float AttackStarFrequeny { get; protected set; }
        public float AttackStarEnergy { get; protected set; }
        public int GameTime { get; protected set; }
    }
}