using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mogo.GameData
{
    public partial class SkillData : GameData<SkillData>

    {
        public static readonly string fileName = "xml/SkillData";

        //是否需要聚气阶段
        public bool chargePower { get; set; }

        //单位聚气时间
        public float chargePowerUnitTime { get; set; }

        //单位聚气体积增长
        public float chargePowerUnitVolume { get; set; }

        //最大体积
        public float chargePowerMaxVolume { get; set; }

        //技能聚气速率
        public float skillChargePowerRate { get; set; }

        //技能聚气释放百分比
        public float skillReleasePercent { get; set; }

        public float chargePowerUnitConsumePower { get; set; }

        //是否能够追踪目标
        public int traceTarget { get; set; }

        //移动速度
        public int moveSpeed { get; set; }

        public int skillDamage { get; set; }

        //技能预设
        public string skillPrefab { get; set; }

        //直接释放的技能消耗
        public float ConsumeEnergy { get; set; }

        public int hitDistance { get; set; }

        public int continueDamage { get; set; }

        public float continueDamageRadius { get; set; }

        public List<string> skillSound { get; set; }

        public uint continueAttackTime { get; set; }

        //持续伤害的特效显示间隔
        public uint continueFxTimeDelta { get; set; }

        //持续伤害的特效显示次数
        public int continueFxTimes { get; set; }

        //持续伤害计算时间间隔
        public uint continueDamageTimeDelta { get; set; }

        //持续伤害的次数
        public int continueDamageTimes { get; set; }

        //持续伤害的技能对象的销毁定时，上面两者的总时长不应该大于这个值
        public uint continueDamageDestroyTime { get; set; }
        
        //技能对象的初始大小
        public float initSize { get; set; }

        public int isHpUp { get; set; }

        public int skillRange { get; set; }

        public string explodeFx { get; set; }

        public SkillData()
        {
            skillRange = 1000;
        }
    }
}
