using System.Collections.Generic;

namespace Mogo.Game
{
    // 物品绑定与否
    public class ITEM_TYPE_BIND_FLAG
    {
        public const int ITEM_TYPE_NOBIND = 1; // [非绑定]
        public const int ITEM_TYPE_BIND = 0; // [绑定]
    }

    // 数据表物品分类
    public class ITEM_TYPE_XML
    {
        public const int ITEM_TYPE_NORMALEQUIP = 1; // [普通装备分类]
        public const int ITEM_TYPE_JEWEL = 2; // [宝石分类]
        public const int ITEM_TYPE_MATERIAL = 3; // [材料分类]
        public const int ITEM_TYPE_RUNE = 4; // [符文分类]
        public const int ITEM_TYPE_PEAKEQUIP = 5; // [巅峰装备分类]
    }

    public class UIConfig
    {
        public const int WIDTH = 1280;
    }

    public enum Vocation : byte
    {
        Warrior = 1, // 战士
        Assassin = 2, // 刺客
        Archer = 3, // 弓箭手
        Mage = 4, // 法师
        Others = 5 // 其他
    }

    public enum Gender : byte
    {
        Female = 0, // 女性
        Male = 1 // 男性 
    }

    public class Nation
    {
        public const int LANG = 1; //狼血
        public const int SHI = 2; //狮心
        public const int FENG = 3; //风暴
    }

    public enum ItemCode
    {
        GOLD = 2
    }

    /** 普通装备部位
     * **/

    public enum EquipType
    {
        Head = 1, // 普通装备头部
        Neck = 2, // 普通装备颈部
        Shoulder = 3, // 普通装备肩部
        Cuirass = 4, // 普通装备胸甲
        Belt = 5, // 普通装备腰带
        Glove = 6, // 普通装备手套
        Cuish = 7, // 普通装备腿甲
        Shoes = 8, // 普通装备靴子
        Ring = 9, // 普通装备戒指部位
        Weapon = 10 // 普通装备武器部位
    }

    /** 装备插槽
     * **/

    public enum EquipSlot
    {
        Head = 1, // 头部
        Neck = 2, // 颈部
        Shoulder = 3, // 肩部
        Cuirass = 4, // 胸甲
        Belt = 5, // 腰带
        Glove = 6, // 手套
        Cuish = 7, // 腿甲
        Shoes = 8, // 靴子
        LeftRing = 9, // 左戒子
        RightRing = 10, // 右戒子
        Weapon = 11 // 武器
    }

    public enum WeaponSubType
    {
        none = 0,
        blade = 1, // 大剑
        fist = 2, // 拳套
        dagger = 3, // 匕首
        twinblade = 4, // 月刃
        staff = 5, // 法杖
        fan = 6, // 扇
        bow = 7, // 弓
        gun = 8 // 大炮
    };

    public enum BattleModeType
    {
        ClientGuiding = 1, // 客户端先行的
        ServerGuiding = 2 // 服务器端先行的
    }

    public enum TargetType
    {
        Enemy = 1, // 敌人
        TeamMember = 2, // 队友
        Myself = 4, // 自己
        //Ally = 3 // 友方
    }

    public enum TargetRangeType
    {
        LineRange = 3,
        SectorRange = 0,
        CircleRange = 1,
        SingeTarget = 2,
        ForwardCircleRange = 5,
        WorldRange = 6
    }

    public enum CharacterCode : short
    {
        INVALID_NAME = 14, //名字不合法
        INPUT_NAME = 20, //请输入名字
        NOT_SELECTED = 501, //未选定角色
        CREATE_CHARACTER = 502, //创建角色
        DELETE_CHARACTER = 506, //删除角色确认
        QUIT_GAME_CONFIRM = 601 //确认退出游戏？
    };

    public enum IconOffset
    {
        Avatar = 14000
    }

    /// <summary>
    ///     中文表分段偏移。
    /// </summary>
    public enum LangOffset
    {
        /// <summary>
        ///     帐号管理。
        /// </summary>
        Account = 20999,

        /// <summary>
        ///     角色管理。
        /// </summary>
        Character = 21000,

        /// <summary>
        ///     游戏启动
        /// </summary>
        StartGame = 21200
    }

    public class ActionConstants
    {
        //public static readonly int CITY_IDLE = -1; //主城的站立
        public static readonly int CITY_IDLE = 0; //主城的站立
        public static readonly int COPY_IDLE = 0; //副本的站立
        public static readonly int HIT = 11; //受击
        public static readonly int HIT_AIR = 12; //浮空
        public static readonly int HIT_GROUND = 13; //倒地受击
        public static readonly int KNOCK_DOWN = 14; //击飞
        public static readonly int PUSH = 15; //后退
        public static readonly int STUN = 16;
        public static readonly int DIE = 17; //死亡
        public static readonly int REVIVE = 19; //复活
        public static readonly int DIE_KNOCK_DOWN = 37; //击飞死亡
        public static readonly int DIE_AIR = 38; //浮空死亡
    }

    public class PlayerActionNames
    {
//角色动作ID对照名字表,用于技能结束判断
        public static readonly Dictionary<int, string> names = new Dictionary<int, string>
        {
            {-1, "idle"},
            {0, "ready"},
            {1, "attack_1"},
            {2, "attack_2"},
            {3, "attack_3"},
            {4, "powercharge"},
            {5, "powerattack_1"},
            {6, "powerattack_2"},
            {7, "powerattack_3"},
            {8, "skill_1"},
            {9, "skill_2"},
            {10, "rush"},
            {11, "hit"},
            {12, "hitair"},
            {13, "hitground"},
            {14, "knockdown"},
            {15, "push"},
            {16, "stun"},
            {17, "die"}
        };

        public static readonly string IDLE = "idle";
        public static readonly string READY = "ready";
        public static readonly string ATTACK_1 = "attack_1";
        public static readonly string ATTACK_2 = "attack_2";
        public static readonly string ATTACK_3 = "attack_3";
        public static readonly string POWERCHARGE = "powercharge";
        public static readonly string POWERATTACK_1 = "powerattack_1";
        public static readonly string POWERATTACK_2 = "powerattack_2";
        public static readonly string POWERATTACK_3 = "powerattack_3";
        public static readonly string SKILL_1 = "skill_1";
        public static readonly string SKILL_2 = "skill_2";
        public static readonly string RUSH = "rush";
        public static readonly string HIT = "hit";
        public static readonly string HITAIR = "hitair";
        public static readonly string HITGROUND = "hitground";
        public static readonly string KNOCKDOWN = "knockdown";
        public static readonly string PUSH = "push";
        public static readonly string STUN = "stun";
        public static readonly string DIE = "die";
        public static readonly string DIE_HITAIR = "dir_hitair";
        public static readonly string DIE_KNOCKDOWN = "dir_knockdown";
        public static readonly string GETUP = "getup";
    }

    public class ActionTime
    {
//动作时间，毫秒
        public static readonly uint HIT = 600;
        public static readonly uint HIT_AIR = 3500;
        public static readonly uint KNOCK_DOWN = 3500;
        public static readonly uint PUSH = 1000;
        public static readonly uint HIT_GROUND = 3000;
        public static readonly uint REVIVE = 2500;
    }

    public class StateCfg
    {
        public static readonly int DEATH_STATE = 0; //死亡状态       
        public static readonly int DIZZY_STATE = 1; //眩晕状态       
        public static readonly int POSSESS_STATE = 2; //魅惑状态       
        public static readonly int IMMOBILIZE_STATE = 3; //定身状态       
        public static readonly int SILENT_STATE = 4; //沉默状态       
        public static readonly int STIFF_STATE = 5; //僵直状态       
        public static readonly int FLOAT_STATE = 6; //浮空状态       
        public static readonly int DOWN_STATE = 7; //击倒状态       
        public static readonly int BACK_STATE = 8; //击退状态       
        public static readonly int UP_STATE = 9; //击飞状态       
        public static readonly int IMMUNITY_STATE = 10; //免疫状态       
        public static readonly int NO_HIT_STATE = 11; //无法被击中状态 
        public static readonly int SLOW_DOWN_STATE = 12; //无法被击中状态 
        public static readonly int BATI_STATE = 13; //霸体状态
        //public static readonly int BATI_STATE = 14; //免疫伤害状态 ,一般在战场复活后用的
        public static readonly int HIDE_STATE = 15; //隐身状态 
    }

    public class DummyLookOnParam
    {
        public static readonly float CLOSE_MODE_FLOAT_FACTOR = 0.2f;
        //接近模式距离浮动系数(random(技能castRange*(1-CLOSE_MODE_FLOAT_FACTOR), 技能castRange*(1+CLOSE_MODE_FLOAT_FACTOR)))

        public static readonly float REFER = 1.6f; //接近远离界定距离系数（技能castRange*REFER）
        public static readonly float PER_ANGLE = 40.0f; //绕圈模式每次绕的角度 
        public static readonly float SPEED_FACTOR_MODE_0 = 0.5f;
        public static readonly float SPEED_FACTOR_MODE_1 = 0.4f;
        public static readonly float SPEED_FACTOR_MODE_2_3 = 0.4f;
        public static readonly float SPEED_FACTOR_MODE_5 = 1.6f;
    }

    public enum CliEntityType
    {
        CLI_ENTITY_TYPE_DUMMY = 1,
        CLI_ENTITY_TYPE_JUG = 2,
        CLI_ENTITY_TYPE_DROP = 3,
        CLI_ENTITY_TYPE_SPAWNPOINT = 4,
        CLI_ENTITY_TYPE_PUBLIC_DROP = 5
    }

    public enum AutoFightState : byte
    {
        IDLE = 1,
        PAUSE = 2,
        RUNNING = 3
    }

    public class RandomFB
    {
        public static readonly int RAIDID = 50000;
    }

    public class SpecialMonsterId
    {
        public static readonly int TowerDefCrystalId = 5010;
    }

    public class AISpecialEnum
    {
        public static readonly int PatrolSquareRange = 3; //5米 单位m 正方形半径
        public static readonly int DefaultSee = 5000; //50米 单位cm
        public static readonly float PATROL_SPEED_FACTOR = 0.5f; //巡逻速度衰减
    }

    public enum AIWarnEvent : byte
    {
        DiscoverSomeOne = 1
    }

    public class CopyConditionType
    {
        /// <summary>
        ///     击杀BOSS
        /// </summary>
        public const int BOSS = 1;

        /// <summary>
        ///     击杀所有怪物
        /// </summary>
        public const int KILL_ALL_MONSTER = 2;

        /// <summary>
        ///     剩余血量
        /// </summary>
        public const int HP = 3;

        /// <summary>
        ///     通关时间
        /// </summary>
        public const int TIME = 4;

        /// <summary>
        ///     使用血瓶
        /// </summary>
        public const int BLOOD_VIAL = 5;

        /// <summary>
        ///     开启副本宝箱
        /// </summary>
        public const int ITEM_BOX = 6;
    }
}