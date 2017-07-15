// 模块名   :  Events
// 创建者   :  Steven Yang
// 创建日期 :  2012-12-12
// 描    述 :  定义各种事件参数类

using Mogo.RPC;
using System;

namespace Mogo.Util
{
    public static class Events
    {
        public static readonly string Unkown = "Unkown";

        // 定义事件参数类
        // 网络消息
        public static class NetworkEvent
        {
            public static readonly string Connect = "NetworkEvent.Connect"; //连接请求事件
            public static readonly string OnClose = "NetworkEvent.OnClose";
            public static readonly string OnDataRecv = "NetworkEvent.OnDataRecv";
            public static readonly string OnConnected = "NetworkEvent.OnConnected";
            public static readonly string OnSyncTimeFromServer = "NetworkEvent.OnSyncTimeFromServer"; //服务端同步时间到客户端
        }

        public static class FrameWorkEvent
        {
            public static readonly string EntityAttached = MSGIDType.CLIENT_ENTITY_ATTACHED.ToString();
            public static readonly string EntityCellAttached = MSGIDType.CLIENT_ENTITY_CELL_ATTACHED.ToString();
            public static readonly string AOINewEntity = MSGIDType.CLIENT_AOI_NEW_ENTITY.ToString(); //entity进入事件
            public static readonly string AOIDelEvtity = MSGIDType.CLIENT_AOI_DEL_ENTITY.ToString(); //entity退出事件
            public static readonly string BaseLogin = MSGIDType.BASEAPP_CLIENT_LOGIN.ToString();
            public static readonly string AvatarAttriSync = MSGIDType.CLIENT_AVATAR_ATTRI_SYNC.ToString();
            public static readonly string EntityPosPull = MSGIDType.CLIENT_ENTITY_POS_PULL.ToString();
            public static readonly string EntityPosSync = MSGIDType.CLIENT_ENTITY_POS_SYNC.ToString();
            public static readonly string EntityPosTeleport = MSGIDType.CLIENT_ENTITY_POS_TELEPORT.ToString();
            public static readonly string OtherAttriSync = MSGIDType.CLIENT_OTHER_ENTITY_ATTRI_SYNC.ToString();
            public static readonly string OtherEntityPosPull = MSGIDType.CLIENT_OTHER_ENTITY_POS_PULL.ToString();
            public static readonly string OtherEntityPosSync = MSGIDType.CLIENT_OTHER_ENTITY_POS_SYNC.ToString();
            public static readonly string OtherEntityPosTeleport = MSGIDType.CLIENT_OTHER_ENTITY_TELEPORT.ToString();
            public static readonly string Login = "FrameWorkEvent.Login";
            public static readonly string ReConnectKey = "FrameWorkEvent.RECONNECT_KEY";
            public static readonly string ReConnectRefuse = "FrameWorkEvent.RECONNECT_REFUSE";
            public static readonly string DefuseLogin = "FrameWorkEvent.DEFUSE_LOGIN";
            public static readonly string CheckDef = MSGIDType.LOGINAPP_CHECK.ToString();
        }

        // FSMMotionEvent
        public static class FSMMotionEvent
        {
            public static readonly string OnPrepareEnd = "FSMMotionEvent.OnPrepareEnd";
            public static readonly string OnAttackingEnd = "FSMMotionEvent.OnAttackingEnd";
            public static readonly string OnHitAnimEnd = "FSMMotionEvent.OnHitAnimEnd";
            public static readonly string OnRollEnd = "FSMMotionEvent.OnRollEnd";
            public static readonly string OnHit = "FSMMotionEvent.OnHit";
            public static readonly string OnForceMove = "FSMMotionEvent.OnForceMove";
        }

        // 战斗UI操作事件
        public static class UIBattleEvent
        {
            public static readonly string OnNormalAttack = "UIBattleEvent.OnNormalAttack";
            public static readonly string OnSpellOneAttack = "UIBattleEvent.OnSpellOneAttack";
            public static readonly string OnSpellTwoAttack = "UIBattleEvent.OnSpellTwoAttack";
            public static readonly string OnSpellThreeAttack = "UIBattleEvent.OnSpellThreeAttack";
            public static readonly string OnSpellXPAttack = "UIBattleEvent.OnSpellXPAttack";
            public static readonly string OnPowerChargeStart = "UIBattleEvent.OnPowerChargeStart";
            public static readonly string OnPowerChargeComplete = "UIBattleEvent.OnPowerChargeComplete";
            public static readonly string OnPowerChargeInterrupt = "UIBattleEvent.OnPowerChargeInterrupt";
            public static readonly string OnResetPowerCharge = "UIBattleEvent.OnResetPowerCharge";
            public static readonly string OnUseItem = "UIBattleEvent.OnUseItem";
            public static readonly string OnFlushBossBlood = "UIBattleEvent.OnFlushBossBlood";
            public static readonly string OnFlushMercenaryBlood = "UIBattleEvent.OnFlushMercenaryBlood";
            public static readonly string OnSpriteSkill = "UIBattleEvent.OnSpriteSkill";
        }

        // 帐号UI操作事件
        public static class UIAccountEvent
        {
            public static readonly string OnLogin = "UIAccountEvent.OnLogin";
            public static readonly string OnCreateCharacter = "UIAccountEvent.OnCreateCharacter";
            public static readonly string OnCreateCharacterFinished = "UIAccountEvent.OnCreateCharacterFinished";
            public static readonly string OnDelCharacter = "UIAccountEvent.DelCharacter";
            public static readonly string OnStartGame = "UIAccountEvent.OnStartGame";
            public static readonly string OnChooseServer = "UIChooseServerEvent.OnChooseServer";
            public static readonly string OnChangeServer = "UIChooseServerEvent.OnChangeServer";
            public static readonly string OnGetRandomName = "UIChooseServerEvent.OnGetRandomName";
            public static readonly string OnGetRandomNation = "UIChooseServerEvent.OnGetRandomNation";
        }

        // OtherEvent
        public static class OtherEvent
        {
            public static readonly string OnEvent1 = "OtherEvent.OnEvent1";
            public static readonly string OnEvent2 = "OtherEvent.OnEvent2";
            public static readonly string OnEvent3 = "OtherEvent.OnEvent3";
            public static readonly string OnThink = "OnThink";
            public static readonly string CallTeammate = "CallTeammate";
            public static readonly string OnChangeWeapon = "OtherEvent.OnChangeWeapon";
            public static readonly string MainCameraComplete = "OtherEvent.MainCameraComplete";
            public static readonly string MapIdChanged = "OtherEvent.MapIdChanged";
            public static readonly string ChangeDummyRate = "OtherEvent.ChangeDummyRate";
            public static readonly string ResetDummyRate = "OtherEvent.ResetDummyRate";
            public static readonly string ClientGM = "OtherEvent.ClientGM";
            public static readonly string SecondPast = "OtherEvent.OneSecondPast";
            public static readonly string Withdraw = "OtherEvent.Withdraw";
            public static readonly string DiamondMine = "OtherEvent.DiamondMine";
            public static readonly string CheckCharge = "OtherEvent.CheckCharge";
            public static readonly string BossDie = "OtherEvent.BossDie";
            public static readonly string Charge = "OtherEvent.Charge";
            public static readonly string DailyBuyItem = "OtherEvent.DailyBuyItem";
            public static readonly string TreasureMapMonsterBorn = "OtherEvent.TreasureMapMonsterBorn";
        }

        public static class TaskEvent
        {
            public static readonly string NPCInSight = "TaskEvent.NPCInSight";
            public static readonly string CloseToNPC = "TaskEvent.CloseToNPC";
            public static readonly string LevelWin = "TaskEvent.LevelWin";
            public static readonly string GuideDone = "TaskEvent.GuidDone";
            public static readonly string LeaveFromNPC = "TaskEvent.LeaveFromNPC";
            public static readonly string AcceptTask = "TaskEvent.AcceptTask";
            public static readonly string TalkEnd = "TaskEvent.TalkEnd";

            /// <summary>
            ///     模拟点击任务追踪
            /// </summary>
            public static readonly string ClickTaskTrace = "TaskEvent.ClickTaskTrace";

            public static readonly string ShowRewardEnd = "TaskEvent.ShowRewardEnd";
            public static readonly string GoToNextTask = "TaskEvent.GoToNextTask";
            public static readonly string AcceptNewTask = "TaskEvent.AcceptNewTask";
            public static readonly string NPCSetSign = "TaskEvent.OnNPCSetSign";
            public static readonly string CheckNpcInRange = "TaskEvent.CheckNpcInRange";
        }

        /// <summary>
        ///     副本事件
        /// </summary>
        public static class InstanceEvent
        {
            public static readonly string UpdateMissionMessage = "InstanceEvent.UpdateMissionMessage";
            public static readonly string UpdateEnterableMissions = "InstanceEvent.UpdateFinishedMissions";
            public static readonly string UpdateMissionTimes = "InstanceEvent.UpdateMissionTimes";
            public static readonly string UpdateMissionStars = "InstanceEvent.UpdateMissionStars";
            public static readonly string UpdateMap = "InstanceEvent.UpdateMap";
            public static readonly string InstanceSelected = "InstanceEvent.InstanceSelected";
            public static readonly string BeforeInstanceLoaded = "InstanceEvent.BeforeInstanceLoaded";
            public static readonly string BeforeSwitchScene = "InstanceEvent.BeforeSwitchScene";
            public static readonly string InstanceLoaded = "InstanceEvent.InstanceLoaded";
            public static readonly string InstanceUnLoaded = "InstanceEvent.InstanceUnLoaded";
            public static readonly string MissionStart = "InstanceEvent.MissionStart";
            public static readonly string ReturnHome = "InstanceEvent.ReturnHome";
            public static readonly string WinReturnHome = "InstanceEvent.WinReturnHome";

            //public static readonly string ResetMission = "InstanceEvent.ResetMission";
            public static readonly string CameraLoaded = "InstanceEvent.CameraLoaded";

            public static readonly string SpawnPointStart = "InstanceEvent.SpawnPointStart";
            public static readonly string NotReborn = "InstanceEvent.NotReborn";
            public static readonly string Reborn = "InstanceEvent.Reborn";
            public static readonly string GetCurrentReward = "InstanceEvent.GetCurrentReward";
            public static readonly string GetMercenaryInfo = "InstanceEvent.GetMercenaryInfo";
            public static readonly string AddFriendDegree = "InstanceEvent.AddFriendDegree";
            public static readonly string UploadMaxCombo = "InstanceEvent.UploadMaxCombo";
            public static readonly string SweepMission = "InstanceEvent.SweepMission";
            public static readonly string GetSweepMissionList = "InstanceEvent.GetSweepMissionList";
            public static readonly string GetSweepTimes = "InstanceEvent.GetSweepTimes";
            public static readonly string GetChestReward = "InstanceEvent.GetChestReward";
            public static readonly string StopAutoFight = "InstanceEvent.StopAutoFight";

            //public static readonly string GetBossChestRewardReq = "InstanceEvent.GetBossChestRewardReq";
            public static readonly string EnterRandomMission = "InstanceEvent.EnterRandomMission";
        }

        /// <summary>
        ///     机关事件
        /// </summary>
        public static class GearEvent
        {
            public static readonly string LoadEnd = "GearEvent.LoadEnd";
            public static readonly string SetGearEnable = "GearEvent.SetGearEnable";
            public static readonly string SetGearDisable = "GearEvent.SetGearDisable";
            public static readonly string SetGearStateOne = "GearEvent.SetGearStateOne";
            public static readonly string SetGearStateTwo = "GearEvent.SetGearStateTwo";
            public static readonly string SetGearEventEnable = "GearEvent.SetGearEventEnable";
            public static readonly string SetGearEventDisable = "GearEvent.SetGearEventDisable";
            public static readonly string SetGearEventStateOne = "GearEvent.SetGearEventStateOne";
            public static readonly string SetGearEventStateTwo = "GearEvent.SetGearEventStateTwo";
            public static readonly string FlushGearState = "GearEvent.FlushGearState";
            public static readonly string UploadAllGear = "GearEvent.UploadAllGear";
            public static readonly string DownloadAllGear = "GearEvent.DownLoadALLGear";
            public static readonly string SwitchLightMapFog = "GearEvent.SwitchLightMapFog";
            public static readonly string Teleport = "GearEvent.Teleport";
            public static readonly string Damage = "GearEvent.Damage";
            public static readonly string SpawnPointDead = "GearEvent.SpawnPointDead";
            public static readonly string MotorHandleEnd = "GearEvent.MotorHandleEnd";
            public static readonly string TrapBegin = "GearEvent.TrapBegin";
            public static readonly string TrapEnd = "GearEvent.TrapEnd";
            public static readonly string LiftEnter = "GearEvent.LiftEnter";
            public static readonly string PathPointTrigger = "GearEvent.PathPointTrigger";
            public static readonly string CrockBroken = "GearEvent.CrockBroken";
            public static readonly string ChestBroken = "GearEvent.ChestBroken";
            public static readonly string CongealMagma = "GearEvent.CongealMagma";
            public static readonly string OccupyTowerClockReady = "GearEvent.OccupyTowerClockReady";
            public static readonly string OccupyTowerClockTime = "GearEvent.OccupyTowerClockTime";
            public static readonly string GuildBuildingReady = "GearEvent.GuildBuildingReady";
            public static readonly string GuildBuildingCurrentLevel = "GearEvent.GuildBuildingCurrentLevel";
            public static readonly string GuildResetNavigateType = "GearEvent.GuildResetNavigateType";
        }

        public static class NPCEvent
        {
            public static readonly string FrushIcon = "NPCEvent.FrushIcon";
            public static readonly string TurnToPlayer = "NPCEvent.TurnToPlayer";
            public static readonly string TalkEnd = "NPCEvent.TalkEnd";
        }

        /// <summary>
        ///     副本UI事件
        /// </summary>
        public static class InstanceUIEvent
        {
            public static readonly string UpdateMapName = "InstanceUIEvent.UpdateMap";

            //public static readonly string UpdateMissionEnable = "InstanceUIEvent.UpdateGridEnable";
            //public static readonly string UpdateMissionName = "InstanceUIEvent.UpdateMissionName";
            //public static readonly string UpdateMissionStar = "InstanceUIEvent.UpdateMissionStar";
            //public static readonly string UpdateLevelEnable = "InstanceUIEvent.UpdateLevelEnable";
            public static readonly string UpdateLevelTime = "InstanceUIEvent.UpdateLevelTime";

            public static readonly string UpdateLevelStar = "InstanceUIEvent.UpdateLevelStar";
            public static readonly string UpdateLevelButtonsVisible = "InstanceUIEvent.UpdateLevelButtonsVisible";
            public static readonly string UpdateLevelSCondition = "InstanceUIEvent.UpdateLevelSCondition";

            //public static readonly string CheckMissionTimes = "InstanceUIEvent.CheckMissionTimes";
            public static readonly string GetDrops = "InstanceUIEvent.GetDrops";

            public static readonly string GetChestRewardGotMessage = "InstanceUIEvent.GetChestRewardGotMessage";

            //public static readonly string ShowResetMissionWindow = "InstanceUIEvent.ShowResetMissionWindow";
            //public static readonly string UpdateMercenaryButton = "InstanceUIEvent.UpdateMercenaryButton";
            public static readonly string FlipCard = "InstanceUIEvent.FlipCard";

            public static readonly string FlipRestCard = "InstanceUIEvent.FlipRestCard";
            public static readonly string AutoFlipCard = "InstanceUIEvent.AutoFlipCard";
            public static readonly string AutoFlipRestCard = "InstanceUIEvent.AutoFlipRestCard";
            public static readonly string UpdateLevelRecord = "InstanceUIEvent.UpdateLevelRecord";
            public static readonly string GetBossChestRewardGotMessage = "InstanceUIEvent.GetBossChestRewardGotMessage";
            public static readonly string UpdateChestMessage = "InstanceUIEvent.UpdateChestMessage";
            public static readonly string ShowCard = "InstanceUIEvent.ShowCard";
            public static readonly string GetSceneBasePerson = "InstanceUIEvent.GetSceneBasePerson";
            public static readonly string GetSceneBasePersonInfo = "InstanceUIEvent.GetSceneBasePersonInfo";
        }

        public static class RuneEvent
        {
            public static readonly string GetRuneBag = "RuneEvent.GetRuneBag";
            public static readonly string GetBodyRunes = "RuneEvent.GetBodyRunes";
            public static readonly string GameMoneyRefresh = "RuneEvent.GameMoneyRefresh";
            public static readonly string FullRefresh = "RuneEvent.FullRefresh";
            public static readonly string RMBRefresh = "RuneEvent.RMBRefresh";
            public static readonly string AutoCombine = "RuneEvent.AutoCombine";
            public static readonly string AutoPickUp = "RuneEvent.AutoPickUp";
            public static readonly string UseRune = "RuneEvent.UseRune";
            public static readonly string PutOn = "RuneEvent.PutOn";
            public static readonly string PutDown = "RuneEvent.PutDown";
            public static readonly string ChangeIndex = "RuneEvent.ChangeIndex";
            public static readonly string ChangePosi = "RuneEvent.ChangePosi";
            public static readonly string ShowTips = "RuneEvent.ShowTips";
            public static readonly string CloseDragon = "RuneEvent.CloseDragon";
            public static readonly string ChangePage = "RuneEvent.ChangePage";
        }

        public static class TowerEvent
        {
            public static readonly string EnterMap = "TowerEvent.EnterMap";
            public static readonly string NormalSweep = "TowerEvent.NormalSweep";
            public static readonly string VIPSweep = "TowerEvent.VIPSweep";
            public static readonly string SweepAll = "TowerEvent.SweepAll";
            public static readonly string GetInfo = "TowerEvent.GetInfo";
            public static readonly string CreateDoor = "TowerEvent.CreateDoor";
            public static readonly string FinishSingle = "TowerEvent.FinishSingle";
            public static readonly string ClearCD = "TowerEvent.ClearCD";
        }

        public static class StoryEvent
        {
            public static readonly string CGBegin = "StoryEvent.CGBegin";
            public static readonly string CGEnd = "StoryEvent.CGEnd";
        }

        public static class CommandEvent
        {
            public static readonly string CommandEnd = "CommandEvent.CommandEnd";
        }

        public static class ChallengeUIEvent
        {
            public static readonly string Enter = "ChallengeUIEvent.Enter";
            public static readonly string GetOgreMustDieTime = "ChallengeUIEvent.GetOgreMustDieTime";
            public static readonly string CollectChallengeState = "ChallengeUIEvent.CollectChallengeState";

            public static readonly string ReceiveChallengeUIGridMessage =
                "ChallengeUIEvent.ReceiveChallengeUIGridMessage";

            public static readonly string FlushChallengeUIGridSortedResult =
                "ChallengeUIEvent.FlushChallengeUIGridSortedResult";
        }

        public static class NormalMainUIEvent
        {
            public static readonly string ShowChallegeIconTip = "NormalMainUIEvent.ShowChallegeIconTip";
            public static readonly string HideChallegeIconTip = "NormalMainUIEvent.HideChallegeIconTip";
            public static readonly string ShowArenaIconTip = "NormalMainUIEvent.ShowArenaIconTip";
            public static readonly string HideArenaIconTip = "NormalMainUIEvent.HideArenaIconTip";
            public static readonly string ShowMallConsumeIconTip = "NormalMainUIEvent.ShowMallConsumeIconTip";
            public static readonly string ShowTongIconTip = "NormalMainUIEvent.ShowTongIconTip";
            public static readonly string OnBtnShareUp = "NormalMainUIEvent.OnBtnShareUp";
            public static readonly string OnBtnTeamUp = "NormalMainUIEvent.OnBtnTeamUp";
            public static readonly string ShowMarketIconTip = "NormalMainUIEvent.ShowMarketIconTip";
            public static readonly string ShowTaskIconTip = "NormalMainUIEvent.ShowTaskIconTip";
            public static readonly string ShowDailyIconTip = "NormalMainUIEvent.ShowDailyIconTip";
        }

        public static class SpellEvent
        {
            public static readonly string OpenView = "SpellEvent.OpenView";
            public static readonly string SelectGroup = "SpellEvent.SelectGroup";
            public static readonly string SelectLevel = "SpellEvent.SelectLevel";
            public static readonly string Study = "SpellEvent.Study";
        }

        public static class AssistantEvent
        {
            public static readonly string SkillGridDragToBodyGrid = "AssistantUISkillGridDragToBodyGrid";
            public static readonly string MintmarkGridDragToBodyGrid = "AssistantUIMintmarkGridDragToBodyGrid";
            public static readonly string LevelUpSkillResp = "AssistantUILevelUpSkillResp";
            public static readonly string LevelUpMarkResp = "AssistantUILevelUpMarkResp";
            public static readonly string ClientDragSkillResp = "AssistantUIClientDragSkillResp";
            public static readonly string ClientDragMarkResp = "AssistantUIClientDragMarkResp";
            public static readonly string SkillGridDragOutside = "AssistantUISkillGridDragOutside";
            public static readonly string MintmarkGridDragOutside = "AssistantUIMintmarkGridDragOutside";
            public static readonly string SkillGridDragBegin = "AssistantUISkillGridDragBegin";
            public static readonly string MintmarkGridDragBegin = "AssistantUIMintmarkGridDragBegin";
            public static readonly string PropRefreshResp = "AssistantUIPropRefreshResp";
        }

        public static class OperationEvent
        {
            public static readonly string Charge = "OperationEvent.Charge";
            public static readonly string ChargeGetReward = "OperationEvent.ChargeGetReward";
            public static readonly string EventGetReward = "OperationEvent.EventGetReward";
            public static readonly string EventShareToGetDiamond = "OperationEvent.EventShareToGetDiamond";
            public static readonly string LogInGetReward = "OperationEvent.LogInGetReward";
            public static readonly string LogInBuy = "OperationEvent.LogInBuy";
            public static readonly string AchievementGetReward = "OperationEvent.AchievementGetReward";
            public static readonly string AchievementShareToGetDiamond = "OperationEvent.AchievementShareToGetDiamond";
            public static readonly string GetChargeRewardMessage = "OperationEvent.GetChargeRewardMessage";
            public static readonly string GetActivityMessage = "OperationEvent.GetActivityMessage";

            //public static readonly string GetLoginMessage = "OperationEvent.GetLoginMessage";
            public static readonly string GetAchievementMessage = "OperationEvent.GetAchievementMessage";

            public static readonly string CheckEventOpen = "OperationEvent.CheckEventOpen";
            public static readonly string FlushCharge = "OperationEvent.FlushCharge";
            public static readonly string CheckFirstShow = "OperationEvent.CheckFirstShow";
            public static readonly string GetAllActivity = "OperationEvent.GetAllActivity";
            public static readonly string EventTimesUp = "OperationEvent.EventTimesUp";
            public static readonly string GetLoginMarket = "OperationEvent.GetLoginMarket";
        }

        public static class AIEvent
        {
            public static readonly string DummyThink = "AIEvent.DummyThink";
            public static readonly string DummyStiffEnd = "AIEvent.DummyStiffEnd";
            public static readonly string ProcessBossDie = "AIEvent.ProcessBossDie";
            public static readonly string SomeOneDie = "AIEvent.ProcessSomeOneDie";
            public static readonly string WarnOtherSpawnPointEntities = "AIEvent.WarnOtherSpawnPointEntities";
        }

        public static class SanctuaryEvent
        {
            public static readonly string RefreshRank = "SanctuaryEvent.RefreshRank";
            public static readonly string RefreshMyInfo = "SanctuaryEvent.RefreshMyInfo";
            public static readonly string EnterSanctuary = "SanctuaryEvent.EnterSanctuary";
            public static readonly string BuyExtraTime = "SanctuaryEvent.BuyExtraTime";
            public static readonly string CanBuyExtraTime = "SanctuaryEvent.CanBuyExtraTime";
            public static readonly string QuerySanctuaryInfo = "SanctuaryEvent.QuerySanctuaryInfo";
        }

        public static class ArenaEvent
        {
            public static readonly string RefreshWeak = "ArenaEvent.RefreshWeak";
            public static readonly string RefreshStrong = "ArenaEvent.RefreshStrong";
            public static readonly string RefreshRevenge = "ArenaEvent.RefreshRevenge";
            public static readonly string RefreshArenaData = "ArenaEvent.RefreshArenaData";
            public static readonly string EnterArena = "ArenaEvent.EnterArena";
            public static readonly string Challenge = "ArenaEvent.Challenge";
            public static readonly string ClearArenaCD = "ArenaEvent.ClearArenaCD";
            public static readonly string AddArenaTimes = "ArenaEvent.AddArenaTimes";
            public static readonly string GetArenaRewardInfo = "ArenaEvent.GetArenaRewardInfo";
            public static readonly string GetArenaReward = "ArenaEvent.GetArenaReward";
            public static readonly string GetArenaBattleRecord = "ArenaEvent.GetArenaBattleRecord";
            public static readonly string GetArenaRankList = "ArenaEvent.GetArenaRankList";
            public static readonly string TabSwitch = "ArenaEvent.TabSwitch";
        }

        public static class ComboEvent
        {
            public static readonly string AddCombo = "ComboEnent.AddCombo";
            public static readonly string ResetCombo = "ComboEnent.ResetCombo";
        }

        public static class DailyTaskEvent
        {
            public static readonly string ShowDailyEvent = "DailyTaskEvent.ShowDailyEvent";
            public static readonly string GetDailyEventReward = "DailyTaskEvent.GetDailyEventReward";
            public static readonly string GetDailyEventData = "DailyTaskEvent.GetDailyEventData";
            public static readonly String OpenDailyTaskUI = "DailyTaskSystemController.OpenDailyTaskUI";
            public static readonly String DailyTaskJumpToOtherUI = "DailyTaskSystemController.DailyTaskJumpToOtherUI";
        }

        public static class EnergyEvent
        {
            public static readonly string BuyEnergy = "EnergyEvent.BuyEnergy";
            public static readonly string UpdateVipLevel = "EnergyEvent.UpdateVipLevel";
        }

        public static class EquipmentEvent
        {
            public static readonly string SetEquipmentUICloseValueZ = "EquipmentEvent.SetEquipmentUICloseValueZ";
        }

        public static class DirecterEvent
        {
            public static readonly string DirActive = "DirecterEvent.DirActive";
        }

        public static class DiamondToGoldEvent
        {
            public static readonly string GoldMetallurgy = "DiamondToGoldEvent.GoldMetallurgy";
        }

        public static class LocalServerEvent
        {
            public static readonly string ExitMission = "LocalServerEvent.ExitMission";
            public static readonly string SummonToken = "LocalServerEvent.SummonToken";
        }

        public static class CampaignEvent
        {
            public static readonly string JoinCampaign = "CampaignEvent.JoinCampaign";
            public static readonly string LeaveCampaign = "CampaignEvent.LeaveCampaign";
            public static readonly string MatchCampaign = "CampaignEvent.MatchCampaign";
            public static readonly string ExitCampaign = "CampaignEvent.ExitCampaign";
            public static readonly string GetCampaignLeftTimes = "CampaignEvent.GetCampaignLeftTimes";
            public static readonly string GetCampaignLastTime = "CampaignEvent.GetCampaignLastTime";
            public static readonly string SetPlayerMessage = "CampaignEvent.SetPlayerMessage";
            public static readonly string FlushPlayerBlood = "CampaignEvent.FlushPlayerBlood";
            public static readonly string RemovePlayerMessage = "CampaignEvent.RemovePlayerMessage";
            public static readonly string CrystalAttacked = "CampaignEvent.CrystalAttacked";
        }

        public static class LogicSoundEvent
        {
            public static readonly string OnHitYelling = "LogicSoundEvent.OnHitYelling";
        }

        public static class WingEvent
        {
            public static readonly string Open = "WingEvent.Open";
            public static readonly string Close = "WingEvent.Close";
            public static readonly string Buy = "WingEvent.Buy";
            public static readonly string Upgrade = "WingEvent.Upgrade";
            public static readonly string Active = "WingEvent.Active";
            public static readonly string PutOn = "WingEvent.PutOn";
            public static readonly string Undo = "WingEvent.Undo";
            public static readonly string UnLock = "WingEvent.UnLock";
            public static readonly string CommonWing = "WingEvent.CommonWing";
            public static readonly string MagicWing = "WingEvent.MagicWing";
            public static readonly string OpenTip = "WingEvent.OpenTip";
            public static readonly string CloseTip = "WingEvent.CloseTip";
            public static readonly string OpenBuy = "WingEvent.OpenBuy";
            public static readonly string OpenUpgrade = "WingEvent.OpenUpgrade";
            public static readonly string CloseUpgrade = "WingEvent.CloseUpgrade";
            public static readonly string TipBuyClick = "WingEvent.TipBuyClick";
            public static readonly string ClosePreview = "WingEvent.ClosePreview";
        }

        /// <summary>
        ///     PVP事件
        /// </summary>
        public static class OccupyTowerEvent
        {
            public static readonly string GetOccupyTowerStatePoint = "OccupyTowerEvent.GetOccupyTowerStatePoint";
            public static readonly string JoinOccupyTower = "OccupyTowerEvent.JoinOccupyTower";
            public static readonly string LeaveOccupyTower = "OccupyTowerEvent.LeaveOccupyTower";
            public static readonly string ExitOccupyTower = "OccupyTowerEvent.ExitOccupyTower";
            public static readonly string SetOccupyTowerUIScorePoint = "OccupyTowerEvent.SetOccupyTowerUIScorePoint";
        }

        public static class RewardEvent
        {
            public static readonly string OpenRewardUI = "RewardEvent.OpenRewardUI";

            //public readonly static string OpenRewardUI2 = "RewardEvent.OpenRewardUI2";
            public static readonly string OpenChargeRewardUI = "RewardEvent.OpenChargeRewardUI";

            public static readonly string WingIcon = "RewardEvent.WingIcon";
            public static readonly string ChargeReward = "RewardEvent.ChargeReward";
            public static readonly string ChargeGold = "RewardEvent.ChargeGold";
            public static readonly string ElfDiamond = "RewardEvent.ElfDiamond";
            public static readonly string LoginReward = "RewardEvent.LoginReward";
            public static readonly string MLoginReward = "RewardEvent.MLoginReward";
            public static readonly string GetLoginReward = "RewardEvent.GetLoginReward";
            public static readonly string SelectReward = "RewardEvent.SelectReward";
            public static readonly string GetChargeReward = "RewardEvent.GetChargeReward";
            public static readonly string GetWing = "RewardEvent.GetWing";
            public static readonly string ShowRewardTip = "RewardEvent.ShowRewardTip";
            public static readonly string OpenTurnTable = "RewardEvent.OpenTurnTable";
            public static readonly string RollTheTable = "RewardEvent.RollTheTable";
            public static readonly string ChargeReturnR = "RewardEvent.ChargeReturnR";
            public static readonly string GetChargeReturnR = "RewardEvent.GetChargeReturnR";
        }

        public static class MogoGlobleUIManagerEvent
        {
            public static readonly String ShowWaitingTip = "MogoGlobleUIManager.ShowWaitingTip";
        }

        public static class NormalMainUIViewManagerEvent
        {
            public static readonly String PVEPLAYICONUP = "NormalMainUIViewManager.PVEPLAYICONUP";
            public static readonly String PVPPLAYICONUP = "NormalMainUIViewManager.PVPPLAYICONUP";
        }

        public static class MFUIManagerEvent
        {
            public static readonly String SwitchUIWithLoad = "MFUIManager.SwitchUIWithLoad";
        }

        public static class ComposeManagerEvent
        {
            public static readonly String SwitchToCompose = "ComposeManager.SwitchToCompose";
        }

        public static class IAPConsumeEvent
        {
            public static readonly String OpenIAPConsumeUI = "IAPConsumeUIController.OpenIAPConsumeUI";
        }

        public static class MonsterEvent
        {
            public static readonly String TowerDamage = "MonsterEvent.TowerDamage";
        }

        public static class NewAchievementEvent
        {
            public static readonly String ShowAchievementSummary = "NewAchievementEvent.ShowAchievementSummary";
            public static readonly String ShowAchievementByMainType = "NewAchievementEvent.ShowAchievementByMainType";

            public static readonly String ShowAchievementByMainTypeAndIsGot =
                "NewAchievementEvent.ShowAchievementByMainTypeAndIsGot";

            public static readonly String ShowAchievementTip = "NewAchievementEvent.ShowAchievementTip";

            public static readonly String ShowUpAchievementUIButtonUp =
                "NewAchievementEvent.ShowUpAchievementUIButtonUp";

            public static readonly string ShowUpAchievement = "NewAchievementEvent.ShowUpAchievement";

            public static readonly String ShowOtherPlayerAchievementTip =
                "NewAchievementEvent.ShowOtherPlayerAchievementTip";
        }

        public static class TongUpgradeUIEvent
        {
            public static readonly String OnTongUpgradeUIShow = "TongUpgradeUIEvent.OnTongUpgradeUIShow";
            public static readonly String OnTabClick = "TongUpgradeUIEvent.OnTabClick";
            public static readonly String OnUpgradeClick = "TongUpgradeUIEvent.OnUpgradeClick";
            public static readonly String OnCompareDialogShow = "TongUpgradeUIEvent.OnCompareDialogShow";
        }

        public static class TongContributionUIEvent
        {
            public static readonly String OnTongContributionUIShow = "TongContributionUIEvent.OnTongUpgradeUIShow";
            public static readonly String OnTabClick = "TongContributionUIEvent.OnTabClick";
            public static readonly String OnContribution1Click = "TongContributionUIEvent.OnContribution1Click";
            public static readonly String OnContribution2Click = "TongContributionUIEvent.OnContribution2Click";

            public static readonly String OnContributionDiamondClick =
                "TongContributionUIEvent.OnContributionDiamondClick";
        }

        public static class TitleEvent
        {
            public static readonly String ShowAllTitle = "TitleEvent.ShowAllTitle";
            public static readonly String ShowHasGotTitle = "TitleEvent.ShowHasGotTitle";
            public static readonly String ShowHasNotGotTitle = "TitleEvent.ShowHasNotGotTitle";
            public static readonly String GetTitleDetail = "TitleEvent.GetTitleDetail";
            public static readonly String EquitTitle = "TitleEvent.EquitTitle";
            public static readonly String JumpToAchievementUI = "TitleEvent.JumpToAchievementUI";
        }

        public static class CrossFriendEvent
        {
            public const string OnAddFriend = "CrossFriendEvent.OnAddFriend";
            public const string OnShowFriendList = "CrossFriendEvent.OnShowFriendList";
            public const string OnWatchFriend = "CrossFriendEvent.OnWatchFriend";
            public const string OnFight = "CrossFriendEvent.OnFight";
        }

        public static class TargetTowardEvent
        {
            public static readonly string LoginDaysChange = "TargetTowardEvent.LoginDaysChange";
            public static readonly string InitTargetTowardTipPan = "TargetTowardEvent.InitTargetTowardTipPan";
        }

        public static class GlobalEvent
        {
            public static readonly string LevelUp = "GlobalEvent.LevelUp";
            public static readonly string GoldChanged = "GlobalEvent.GoldChanged";
            public static readonly string BindDiamondChanged = "GlobalEvent.BindDiamondChanged";
            public static readonly string ChargeDiamondChanged = "GlobalEvent.ChargeDiamondChanged";
            public static readonly string CurrentUIChanged = "CurrentUIChange";
        }

        public static class ResScrambleEvent
        {
            public static readonly string HandleIconTip = "ResScrambleEvent.HandleIconTip";
        }

        public static class CountryFightEvent
        {
            public static readonly string ShowCountryResultUI = "CountryFightEvent.ShowCountryResultUI";
            public static readonly string ShowPersonalResultUI = "CountryFightEvent.ShowPersonalResultUI";
            public static readonly string ReadyUIClosed = "CountryFightEvent.ReadyUIClosed";
            public static readonly string MapUIClosed = "CountryFightEvent.MapUIClosed";
            public static readonly string ShowPointBattleFlag = "CountryFightEvent.ShowPointBattleFlag";
            public static readonly string ShowPointVotingFlag = "CountryFightEvent.ShowPointVotingFlag";
            public static readonly string EnterUIOpen = "CountryFightEvent.EnterUIOpen";
            public static readonly string BossDead = "CountryFightEvent.BossDead";
            public static readonly string StartWarTime = "CountryFightEvent.StartWarTime";
            public static readonly string AttendWarTime = "CountryFightEvent.AttendWarTime";
            public static readonly string WaitingNumUpdate = "CountryFightEvent.WaitingNumUpdate";
        }

        public static class FirstBuyUIEvent
        {
            public static readonly string SetBuyBtnTextByState = "FirstBuyUIEvent.SetBuyBtnTextByState";
            public static readonly string FirstChargeRewardResp = "FirstBuyUIEvent.FirstChargeRewardResp";
            public static readonly string StartGiftFly = "FirstBuyUIEvent.StartGiftFly";
        }

        public static class StarUIEvent
        {
            //点击星球
            public static readonly string ClickStarEvent = "StarUIEvent.ClickStarEvent";

            //星球士兵增加
            public static readonly string AddSoldierEvent = "StarUIEvent.AddSoldierEvent";

            //星球建筑增加
            public static readonly string AddBuildingEvent = "StarUIEvent.AddBuildingEvent";

            //星球增加
            public static readonly string AddStarEvent = "StarUIEvent.AddStarEvent";

            //展示确定攻击按钮
            public static readonly string ShowAttackOkEvent = "StarUIEvent.ShowAttackOkEvent";

            //展示取消攻击按钮
            public static readonly string ShowAttackCancelEvent = "StarUIEvent.ShowAttackCancelEvent";

            //攻击星球
            public static readonly string AttackStarEvent = "StarUIEvent.AttackStarEvent";

            //取消攻击
            public static readonly string CancelAttackEvent = "StarUIEvent.CancelAttackEvent";

            //发送点击位置
            public static readonly string SetAttackPositionEvent = "StarUIEvent.SetAttackPositionEvent";

            //展示侦查兵
            public static readonly string ShowReconSoldierEvent = "StarUIEvent.ShowReconSoldierEvent";
        }
    }
}