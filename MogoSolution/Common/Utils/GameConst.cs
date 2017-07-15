using UnityEngine;
using System.Collections;

public class GameConst
{
    public static float EnergyUpRate { get; set; }
    public static float MaxEnergy = 10000;
    public static int MaxHp = 100;

    public static class Event
    {
        public static readonly string HpChanged = "HpChanged";
        public static readonly string EnergyChanged = "EnergyChanged";
        public static readonly string IsFlyChanged = "IsFlyChanged";
        public static readonly string IsTargetLockChanged = "IsTargetLockChanged";
        public static readonly string CurSkillChanged = "CurSkillChanged";
        public static readonly string TeamOneScoreChanged = "TeamOneScoreChanged";
        public static readonly string TeamTwoScoreChanged = "TeamTwoScoreChanged";
    }
}
