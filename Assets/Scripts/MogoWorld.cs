using Mogo.Game;
using Mogo.GameData;
using Mogo.RPC;
using Mogo.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MogoWorld
{
    public static ScenesManager m_sceneManager;
    public static FindServer m_findServer;
    public static MogoUIManager m_uiManager;
    public static BillboardManager billboardManager;
    public static MapDataManager m_dataMapManager;
    public static float CombatFrequeny;//战斗频率
    public static float CombatNumberInSecond;//在1秒战斗的次数
    public static float BuildingReversionFrequency;//建筑回复频率
    public static int BuildingReversionEnergy;//建筑能量恢复速度
    static private readonly Dictionary<uint, EntityParent> _entities = new Dictionary<uint, EntityParent>();
    public static float AttackStarFrequeny;//攻击星球频率
    //public static float AttackStarEnergy;//攻击星球能量
    public static int GameTime;//游戏时间
    public static bool IsInGame = false;

    static public Dictionary<uint, EntityParent> Entities
    {
        get { return _entities; }
    }

    private static EntityParent m_lockTarget;

    static public EntityParent lockTarget
    {
        get
        {
            return m_lockTarget;
        }
        set
        {
            m_lockTarget = value;
        }
    }

    private static MogoMainCamera m_mainCamera;

    public static MogoMainCamera MainCamera
    {
        get { return m_mainCamera; }
        set { m_mainCamera = value; }
    }

    private static EntityMyself m_thePlayer;

    static public EntityMyself thePlayer
    {
        get
        {
            return m_thePlayer;
        }
        set
        {
            m_thePlayer = value;
        }
    }

    static private int m_playerCount = 0;

    static public int playerCount
    {
        get { return m_playerCount; }
        set
        {
            m_playerCount = value;
        }
    }

    public static string ServerIP = string.Empty;
    public static bool IsSingleMatch;
    public static string TmpPlayerName;

    static MogoWorld()
    {
        AddListeners();
        Pluto.GetEntity = GetEntityDefById;
    }

    private static EntityDef GetEntityDefById(uint id)
    {
        var ety = MogoWorld.Entities.GetValueOrDefault(id, null);
        if (ety != null)
            return ety.entity;
        else
            return null;
    }

    private static void AddListeners()
    {
        EventDispatcher.AddEventListener<string, int>(Events.NetworkEvent.Connect, ConnectServer);

        // 增加 框架 协议处理函数
        EventDispatcher.AddEventListener<LoginResult>(Events.FrameWorkEvent.Login, OnLoginResp);
        EventDispatcher.AddEventListener<BaseAttachedInfo>(Events.FrameWorkEvent.EntityAttached, OnEntityAttached);
        EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.EntityCellAttached, OnEntityCellAttached);
        EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.AOINewEntity, AOINewEntity);
        EventDispatcher.AddEventListener<uint>(Events.FrameWorkEvent.AOIDelEvtity, AOIDelEntity);

        EventDispatcher.AddEventListener<AttachedInfo>(Events.FrameWorkEvent.AvatarAttriSync, OnAvatarAttriSync);
        EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.EntityPosPull, OnEntityPosPull);
        EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.EntityPosSync, OnEntityPosSync);
        EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.EntityPosTeleport, OnEntityPosTeleport);

        EventDispatcher.AddEventListener<AttachedInfo>(Events.FrameWorkEvent.OtherAttriSync, OnOtherAttriSync);
        EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.OtherEntityPosPull, OnOtherEntityPosPull);
        EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.OtherEntityPosSync, OnOtherEntityPosSync);
        EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.OtherEntityPosTeleport, OnOtherEntityPosTeleport);
    }

    #region 网络协议

    // 处理entity 进入场景事件
    public static void OnEnterWorld(EntityParent entity)
    {
        if (_entities.ContainsKey(entity.ID))
        {
            LoggerHelper.Error("Space has the same id:" + entity.ID);
            return;
        }
        if (entity is EntityPlayer)
        {
            playerCount++;
        }
        _entities.Add(entity.ID, entity);
    }

    // 处理 entity 离开场景事件
    private static void OnLeaveWorld(uint eid)
    {
        if (!_entities.ContainsKey(eid))
        {
            return;
        }

        var entity = _entities[eid];

        if (entity is EntityPlayer)
        {
            playerCount--;
        }
        _entities.Remove(eid);
    }

    // 帐号登录回调函数
    static public void OnLoginResp(LoginResult result)
    {
    }

    static private void OnEntityCellAttached(CellAttachedInfo info)
    {
        if (info.id == thePlayer.ID)
        {
            thePlayer.SetEntityCellInfo(info);
            thePlayer.OnEnterWorld();
            thePlayer.CreateModel();
        }
    }

    // 协议处理函数
    static private void OnEntityAttached(BaseAttachedInfo baseInfo)
    {
        if (baseInfo.entity == null)
        {
            LoggerHelper.Error("Entity Attach Error.");
            return;
        }
        switch (baseInfo.entity.Name)
        {
            case "Avatar":
                {
                    if (MogoWorld.thePlayer == null)
                    {
                        thePlayer = new EntityMyself();
                    }
                    //UnityLog.Sys("MySelfAttribute: "+ObjectDumper.Dump(baseInfo));
                    thePlayer.SetEntityInfo(baseInfo);
                    thePlayer.entity = baseInfo.entity;
                    if (IsSingleMatch)
                        StartSingleMatch();
                    else
                        StartMatch();
                    TimerHeap.AddTimer(1000, 1000, () =>
                    {
                        MogoWorld.thePlayer.RpcCall("HeartBeat");
                    });
                    break;
                }
            default:
                break;
        }
    }

    // 协议处理函数， 新的 entity 进入视野
    static private void AOINewEntity(CellAttachedInfo info)
    {
        EntityParent entity;
        //LoggerHelper.Debug(info.entity.Name);
        if (Entities.ContainsKey(info.id) || (thePlayer != null && thePlayer.ID == info.id))
        {
            LoggerHelper.Debug("has same id entity in world");
            return;
        }

        switch (info.entity.Name)
        {
            case "Avatar"://对应Avatar.def
                entity = new EntityPlayer();
                break;

            default:
                entity = new EntityParent();
                break;
        }
        entity.ID = info.id;
        entity.entity = info.entity;
        entity.SetEntityCellInfo(info);
        entity.OnEnterWorld();
        entity.CreateModel();
        OnEnterWorld(entity);
    }

    static private void AOIDelEntity(uint eid)
    {
        if (!_entities.ContainsKey(eid))
            return;
        EntityParent entity = _entities[eid];
        if (entity == null)
        {
            return;
        }
        entity.OnLeaveWorld();
        billboardManager.RemoveBillboard(eid);
        OnLeaveWorld(eid);
        if (lockTarget == entity)
            lockTarget = null;
    }

    static private void OnAvatarAttriSync(AttachedInfo info)
    {
        if (thePlayer == null)
            return;
        thePlayer.SynEntityAttrs(info);
    }

    static private void OnEntityPosPull(CellAttachedInfo info)
    {
    }

    static private void OnEntityPosSync(CellAttachedInfo info)
    {
        thePlayer.MoveTo(info.position);
    }

    static private void OnEntityPosTeleport(CellAttachedInfo info)
    {
        if (thePlayer != null)
        {
            thePlayer.SetEntityCellInfo(info);
            //thePlayer.UpdatePosition();
        }
    }

    static private void OnOtherAttriSync(AttachedInfo info)
    {
        var entity = MogoWorld.Entities.GetValueOrDefault(info.id, null);
        if (entity != null)
            entity.SynEntityAttrs(info);
    }

    static private void OnOtherEntityPosPull(CellAttachedInfo info)
    {
    }

    private static void OnOtherEntityPosSync(CellAttachedInfo info)
    {
        if (!Entities.ContainsKey(info.id))
        {
            return;
        }
        var entity = Entities[info.id];

        // >0移动到XY的服务器预计时间   ==0移动到XY    ==1当前服务器XY校验
        entity.FaceTo(new Vector3(0, info.face * 2, 0));
        entity.MoveTo(info.position);
    }

    static private void OnOtherEntityPosTeleport(CellAttachedInfo info)
    {
        if (!MogoWorld.Entities.ContainsKey(info.id))
            return;
        var entity = MogoWorld.Entities[info.id];

        entity.SetEntityCellInfo(info);
        //entity.UpdatePosition();
    }

    #endregion 网络协议

    public static EntityParent GetEntityById(uint id)
    {
        if (Entities.ContainsKey(id))
        {
            return Entities.Get(id);
        }
        else
        {
            return thePlayer;
        }
    }

    public static void Init(Action callback)
    {
        CombatFrequeny = GlobalData.dataMap[0].CombatFrequeny;
        CombatNumberInSecond = (1 / CombatFrequeny);
        BuildingReversionFrequency = GlobalData.dataMap[0].BuildingReversionFrequency;
        BuildingReversionEnergy = GlobalData.dataMap[0].BuildingReversionEnergy;
        AttackStarFrequeny = GlobalData.dataMap[0].AttackStarFrequeny;
        //AttackStarEnergy = GlobalData.dataMap[0].AttackStarEnergy;
        GameTime = GlobalData.dataMap[0].GameTime;
        m_sceneManager = new ScenesManager();
        m_findServer = new FindServer();
        m_uiManager = new MogoUIManager();
        billboardManager = new BillboardManager();
        m_dataMapManager = new MapDataManager();
        SoundManager.Init();
        ServerProxy.InitServerProxy(new RemoteProxy());
        ServerProxy.Instance.Init(callback);
    }

    public static void StartGame()
    {
        AssetCacheMgr.GetNoCacheResource("MogoBlackWhiteMat.mat", null);
        Pluto.CurrentEntity = DefParser.Instance.GetEntityByName("Avatar");
        m_sceneManager.LoadLoginScene();
        m_uiManager.LoadMainUI();
        m_uiManager.LoadTopUI();
    }

    public static void StartMatch()
    {
        thePlayer.RpcCall("StartMatch", 2, 2);
    }

    public static void StartSingleMatch()
    {
        thePlayer.RpcCall("StartMatch", 1, 1);
    }

    public static void Process()
    {
        if (thePlayer != null)
        {
            thePlayer.Process();
        }
        ServerProxy.Instance.Process();
        ServerProxy.Instance.Update();
        billboardManager.UpdateBillboard();

        if (IsInGame)
        {
            var deltaTime = Time.deltaTime;
            //更新建筑生产
            m_dataMapManager.UpdateBuildingProduction(deltaTime);
            //更新士兵移动时间
            m_dataMapManager.UpdateSoldierMoveTime(deltaTime);
            //更新战斗
            m_dataMapManager.UpdateWar(deltaTime);
            //攻击星球
            m_dataMapManager.UpdateAttackStarTime(deltaTime);
        }
    }

    public static string CreateServer()
    {
        m_findServer.CreateUDP();
        return ServerProxy.Instance.StartServer();
    }

    static public void ConnectServer(string ip, int port)
    {
        bool rst = ServerProxy.Instance.Connect(ip, port);
        Debug.Log("ConnectServer:" + rst);
    }

    static public void DisConnectServer()
    {
        ServerProxy.Instance.Disconnect();
    }

    public static void Login()
    {
        string name = TmpPlayerName;
        if (name == string.Empty || name == null)
        {
            LoggerHelper.Error("MogoWorld.Login: player name error!");
            return;
        }
        ServerProxy.Instance.Login(name, "1", "1");
        UIManager.I.ShowUI<LoadingBattleUIMgr>();
    }

    public static void Quit()
    {
        ServerProxy.Instance.Disconnect();
        ServerProxy.Instance.Release();
        ServerProxy.Instance.StopServer();
        m_findServer.Release();
    }

    public static void FindServer()
    {
        EventDispatcher.TriggerEvent(FindServerEvent.FindServerStart);
    }

    public static void StopFindServer()
    {
        EventDispatcher.TriggerEvent(FindServerEvent.FindServerStop);
    }

    public static void OnMainUILoaded()
    {
        m_uiManager.ShowFPSUI();
        UIManager.I.ShowUI<GameStartUIMgr>();
        billboardManager.ViewCamera = m_uiManager.BillBoardCamera;
    }

    public static void EnterMainEufloria(int mapId)
    {
        UIManager.I.CloseUI<GameStartUIMgr>();
        bool needAutoCloseLoadingUI = false;
        var loadingUIMgr = UIManager.I.GetUILogic<LoadingSceneUIMgr>();
        loadingUIMgr.Show(delegate()
        {
            if (needAutoCloseLoadingUI)
            {
                loadingUIMgr.Close();
            }
        });

        //UIManager.I.CloseUI<GameStartUIMgr>();
        UIManager.I.CloseUI<WaitingUIMgr>();
        //UIManager.I.ShowUI<EufloriaMainUIMgr>();
        UIManager.I.CloseUI<LoadingBattleUIMgr>();
        //UIManager.I.ShowUI<StarInfoUIMgr>();
        MogoWorld.m_sceneManager.LoadMapScene(mapId);

        if (loadingUIMgr.bShow)
        {
            loadingUIMgr.Close();
        }
        else
        {
            needAutoCloseLoadingUI = true;
        }
    }

    public static void CloseUI()
    {
        UIManager.I.CloseUI<BalanceUIMgr>();
        UIManager.I.CloseUI<StarInfoUIMgr>();
        UIManager.I.CloseUI<BattleScoreUIMgr>();
        UIManager.I.CloseUI<ComfirmUIMgr>();
        UIManager.I.CloseUI<BalanceUIMgr>();
        UIManager.I.CloseUI<SoldierFightUIMgr>();
    }
}