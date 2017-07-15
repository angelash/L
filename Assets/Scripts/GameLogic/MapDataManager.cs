using Mogo.Game;
using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MapDataManager
{
    public int CurStarID = -1;

    //建筑层
    public Transform BuildLayer;

    //士兵层
    public Transform SoldierLyaer;

    //建筑数据
    private List<UnitBuilding> m_BuildingList = new List<UnitBuilding>();

    //士兵数据
    private List<UnitSoldier> m_SoldierList = new List<UnitSoldier>();

    //移动的士兵数据
    private List<UnitSoldier> MoveSoldierList = new List<UnitSoldier>();

    /// <summary>
    /// 攻占星球
    /// </summary>
    private List<UnitStar> AttackStar = new List<UnitStar>();

    //保存玩家阵营信息
    private Dictionary<uint, GroupData> GroupDic = new Dictionary<uint, GroupData>();

    //保存我方所有可达到星球
    public List<UnitStar> ReachStarList = new List<UnitStar>();

    ////保存所有星球对应的集结点，值为0表示未有集合点
    //private List<int> TogetherList = new List<int>();
    private float AttackStarTime = 0.0f;

    private Dictionary<uint, List<uint>> PlayerScore = new Dictionary<uint, List<uint>>();

    public StarDataManager m_starDataManager;

    public MapDataManager()
    {
        m_starDataManager = new StarDataManager();
    }

    /// <summary>
    /// 初始化地图上的星球数据
    /// </summary>
    public void InitStarDataInMap(List<int> _starIDs, List<int> _starX, List<int> _starY, Transform _parent)
    {
        //int _len = _starIDs.Count;
        for (int i = 0; i < _starIDs.Count; i++)
        {
            if (i == (_starIDs.Count - 1))
            {
                m_starDataManager.InitStarData(i, _starIDs[i], _starX[i], _starY[i], _parent, SendFirstStar);
            }
            else
            {
                m_starDataManager.InitStarData(i, _starIDs[i], _starX[i], _starY[i], _parent, null);
            }
        }

        m_starDataManager.InitStarEdge();
    }

    /// <summary>
    /// 初始化星球后 初始化各玩家总部信息
    /// 发送总部星球ID
    /// </summary>
    private void SendFirstStar()
    {
        //LoggerHelper.Error("SendFirstStar");
        MogoWorld.thePlayer.RpcCall("OccupyStar", MogoWorld.thePlayer.ID, MogoWorld.thePlayer.HeadQuarterId);
        foreach (var item in GlobalData.dataMap[0].InitBuilding)
        {
            for (int i = 0; i < item.Value; i++)//value为建筑数量
            {
                //LoggerHelper.Error("SendFirstStar AddBuilding " + i);
                MogoWorld.thePlayer.RpcCall("AddBuilding", MogoWorld.thePlayer.ID, MogoWorld.thePlayer.HeadQuarterId, item.Key, 100);//key为建筑类型
            }
        }

        foreach (var item in GlobalData.dataMap[0].InitSoldier)
        {
            MogoWorld.thePlayer.RpcCall("AddSoldier", MogoWorld.thePlayer.ID, MogoWorld.thePlayer.HeadQuarterId, item.Key, (item.Value * SoldierData.dataMap.Get(item.Key).energy), 0);//key为士兵类型，value为数量
        }
        //初始化我方星球移动范围
        ReachStarList.Add(GetUnitStarById(MogoWorld.thePlayer.HeadQuarterId));
        UpdateStarBelongTo(MogoWorld.thePlayer.ID, MogoWorld.thePlayer.HeadQuarterId);

        MogoWorld.IsInGame = true;
        UIManager.I.ShowUI<BattleScoreUIMgr>(MogoWorld.GameTime);

        if (PlayerPrefs.GetInt("NoviceGuide") != 1)
        {
            UIManager.I.ShowUI<NoviceGuideUIMgr>();
        }
    }

    public void StarBelong(uint _playerId, int _starId)
    {
        UnitStar _star = GetUnitStarById(_starId);
        _star.BelongTo = MogoWorld.GetEntityById(_playerId);
        _star.SetGroup(GetGroupByPalyerId(_playerId));//设置阵营信息
    }

    /// <summary>
    /// 创建星球(此方法还没初始化星球数据和它跟其他星球的有向边)
    /// </summary>
    /// <returns></returns>
    private UnitStar CreateStar()
    {
        return m_starDataManager.CreateStar();
    }

    /// <summary>
    /// 清除地图上的星球
    /// </summary>
    public void ClearDataInMap()
    {
        m_starDataManager.ClearStarData();

        ClearBuildingDataInMap();
        ClearSoldierDataInMap();
    }

    /// <summary>
    /// 对星球增加建筑
    /// </summary>
    /// <param name="_starID"></param>
    /// <param name="_buildingTime"></param>
    public void addBuildingToStar(uint dbid, int _starID, int _buildingType, int finishPercentage)
    {
        //LoggerHelper.Error("dbid:" + dbid);
        UnitStar _star = GetUnitStarById(_starID);

        if (_star.BelongTo != null && _star.BelongTo.ID != dbid)
        {
            LoggerHelper.Error("星球和建筑主人不对");
            return;
        }

        //是否是升级建筑
        int _index = _star.IsLevelUpBuilding(_buildingType);
        if (_index >= 0)
        {
            _star.RemoveBuilding(_index, _star.GetBuildingByIndex(_index));
        }
        else
        {//新建建筑
            _index = _star.GetAddBuildingIndex();
            if (_index >= 0)
            {
                _star.AddBuildingLog(_buildingType);
            }
        }

        if (_index < 0)
        {
            return;
        }
        UnitBuilding _building = GetUnitBuildingFromPool();
        Vector2 _pos = _star.GetAddBuildingPos(_index);
        if (_index >= 0)
        {
            _building.UnitId = _star.UnitId * 100 + _index;
            _building.BaseBuildingData = BuildingData.dataMap.Get(_buildingType);
            _building.PositionX = _pos.x;
            _building.PositionY = _pos.y;
            _building.UnitParent = BuildLayer;
            _building.InitPercentage = finishPercentage;
            _building.BelongToStar = _star;
            _building.AttackPlayerId = dbid;
            _building.InitUnit();
            _building.SetGroup(GetGroupByPalyerId(dbid));//设置阵营信息
            _star.AddBuilding(_index, _building);

            if (_starID == CurStarID)//根据当前准备建设的建筑来刷新战斗UI
            {
                UIManager.I.GetUILogic<StarInfoUIMgr>().ShowBuildings(true);
            }
        }

        m_starDataManager.StarOrEndStartFighting(_star);
    }

    /// <summary>
    /// 建筑消耗
    /// </summary>
    /// <param name="_starID"></param>
    /// <param name="_buildingType"></param>
    public void BuildingConsume(int _buildingType)
    {
        UnitStar _star = GetUnitStarById(CurStarID);
        _star.BuildingConsume(BuildingData.dataMap.Get(_buildingType).building_consume);
    }

    /// <summary>
    /// 攻击 建筑
    /// </summary>
    /// <param name="_starID"></param>
    /// <param name="_buildingTime"></param>
    public void AttackBuilding(uint _attackplayerId, int _starID, int buildingPosition, int _Energy)
    {
        UnitStar _star = GetUnitStarById(_starID);
        UnitBuilding _building = _star.GetBuildingByIndex(buildingPosition);

        //LoggerHelper.Error("_building.BelongToStar.BelongTo.ID:" + _building.BelongToStar.BelongTo.ID + "_Energy:" + _Energy);

        if (_building != null)
        {
            //被攻击建筑所属
            if ((_building.BelongToStar != null) && (_building.BelongToStar.BelongTo != null) && (_building.BelongToStar.BelongTo.ID == _attackplayerId))
            {
                return;
            }

            _building.ReduceBuildingEnergy(_Energy);
            ShowFloatText(string.Concat(-_Energy), _building.PositionX, _building.PositionY, _building.BaseBuildingData.float_color);

            if (_building.CurEnergy <= 0)
            {
                _building.OnDead();
            }
            else
            {
                _building.OnHit();
            }
        }
        else
        {
            //LoggerHelper.Debug("没有该类建筑");
            return;
        }

        //LoggerHelper.Error("_attackplayerId:" + _attackplayerId + "_building.CurEnergy:" + _building.CurEnergy);
        if (_building.CurEnergy <= 0)
        {
            //LoggerHelper.Error("_building.CurEnergy:" + _building.CurEnergy);
            _star.RemoveBuilding(buildingPosition, _building);
            //拆一个建筑后就可以占领星球
            /*if (!AttackStar.Contains(_star))
            {
                if (_star.IsInAttackStar())
                {
                    AttackStar.Add(_star);
                }
            }*/
            IsAttackStar(_star);
        }

        _star.showAttackBuildingEffect(_building.UnitGO.transform.localPosition);
    }

    /// <summary>
    /// 清除地图的建筑数据
    /// </summary>
    private void ClearBuildingDataInMap()
    {
        foreach (UnitBuilding _building in m_BuildingList)
        {
            _building.ClearUnit();
        }
    }

    /// <summary>
    /// 根据ID得到星球的动态数据
    /// </summary>
    /// <param name="_id"></param>
    /// <returns></returns>
    public UnitStar GetUnitStarById(int _id)
    {
        return m_starDataManager.GetUnitStarById(_id);
    }

    /// <summary>
    /// 从对象池取 建筑
    /// </summary>
    /// <returns></returns>
    private UnitBuilding GetUnitBuildingFromPool()
    {
        foreach (UnitBuilding _building in m_BuildingList)
        {
            if (_building.UnitId == -1)
            {
                return _building;
            }
        }

        UnitBuilding _newbuilding = new UnitBuilding();
        m_BuildingList.Add(_newbuilding);
        return _newbuilding;
    }

    /// <summary>
    /// 获取所有星球数据
    /// </summary>
    /// <returns></returns>
    public List<UnitStar> GetStarList()
    {
        return m_starDataManager.StarList;
    }

    /// <summary>
    /// 清除地图上所有士兵的数据
    /// </summary>
    private void ClearSoldierDataInMap()
    {
        foreach (UnitSoldier _soldier in m_SoldierList)
        {
            _soldier.ClearUnit();
        }
    }

    private int SoldierIndex = 0;

    /// <summary>
    /// 对星球增加士兵
    /// </summary>
    /// <param name="_starID"></param>
    /// <param name="_buildingTime"></param>
    public void addSoldierToStar(uint _playerId, int _starID, int _soldierType, int _Energy, int _isSelfProduce, bool _isMoveAnimation = true)
    {
        UnitStar _star = GetUnitStarById(_starID);
        UnitSoldier _Soldier = _star.GetSoldierById(_playerId, _soldierType);
        if (_star.BelongTo != null && _star.BelongTo.ID == MogoWorld.thePlayer.ID && _starID == CurStarID)
        {
            EventDispatcher.TriggerEvent(Events.StarUIEvent.AddSoldierEvent);//发送我方当前星球添加兵力的消息
        }
        if (_Soldier != null)
        {
            _Soldier.AddSoldier(_Energy, _isSelfProduce);
            _star.CulAllSoldierAttack();

            if (_isMoveAnimation && (_Soldier.GetGatherSoldierNum() > 0) )
            {
                m_starDataManager.StarTogether(_star);
            }

            m_starDataManager.StarOrEndStartFighting(_star);
            return;
        }
        _Soldier = GetUnitSoldierFromPool();
        SoldierIndex++;
        _Soldier.UnitId = SoldierIndex;
        _Soldier.BelongTo = MogoWorld.GetEntityById(_playerId);
        _Soldier.BaseSoldierData = SoldierData.dataMap.Get(_soldierType);
        Vector2 _pos = _star.GetSoliderPositionById(_soldierType);
        //LoggerHelper.Error("_pos.x:" + _pos.x + "_pos.y:" + _pos.y);
        _Soldier.PositionX = _pos.x;
        _Soldier.PositionY = _pos.y;
        _Soldier.UnitParent = SoldierLyaer;
        _Soldier.AddSoldier(_Energy, _isSelfProduce);
        _Soldier.BelongToStar = _star;
        _Soldier.IsMoveAnimation = _isMoveAnimation;
        _Soldier.InitUnit();
        _Soldier.SetGroup(GetGroupByPalyerId(_playerId));//设置阵营信息
        
        _star.AddSoldier(_playerId, _soldierType, _Soldier);
        _star.CulAllSoldierAttack();

        m_starDataManager.StarOrEndStartFighting(_star);
        if (_isMoveAnimation && (_Soldier.GetGatherSoldierNum() > 0))
        {
            m_starDataManager.StarTogether(_star);
            //_Soldier.CallBack = SoldierCallback;
        }
        IsAttackStar(_star);
    }

    /*private void SoldierCallback(UnitStar _star)
    {
        LoggerHelper.Error("SoldierCallback");
        m_starDataManager.StarTogether(_star);
    }*/

    private void IsAttackStar(UnitStar _star)
    {
        //是否在占领星球
        if (!AttackStar.Contains(_star))
        {
            if (_star.IsInAttackStar())
            {
                AttackStar.Add(_star);
            }
        }
        else//在攻占星球
        {
            //士兵全军覆没，登陆不成功
            if (!_star.IsInAttackStar())
            {
                AttackStar.Remove(_star);
            }
        }
    }

    /// <summary>
    /// 攻击士兵
    /// </summary>
    /// <param name="_starID"></param>
    /// <param name="_buildingTime"></param>
    public void AttackSoldier(uint _playerId, int _starID, int _soldierType, int _Energy,int _type)
    {
        /*LoggerHelper.Error("_playerId:" + _playerId + "_starID:" + _starID +
            "_soldierType:" + _soldierType + "_Energy:" + _Energy );*/
        if (_Energy < 0)
        {
            _Energy = 0;
        }
        UnitStar _star = GetUnitStarById(_starID);
        UnitSoldier _Soldier = _star.GetSoldierById(_playerId, _soldierType);
        if (_Soldier != null)
        {
            _Soldier.RemoveSoldier(_Energy);
            //LoggerHelper.Error(_Soldier.BaseSoldierData.energy + " CurEnergy:" + _Soldier.CurEnergy);
            if (_star.IsExplore && (_Energy >= _Soldier.BaseSoldierData.energy))
            {
                ShowFloatText(string.Concat(-(_Energy / _Soldier.BaseSoldierData.energy)), _Soldier.PositionX, _Soldier.PositionY, _Soldier.BaseSoldierData.float_color);
            }
            //LoggerHelper.Error("_playerId:" + _playerId + " _Energy" + (_Energy / _Soldier.BaseSoldierData.energy));
        }
        else
        {
            //LoggerHelper.Debug("没有该类士兵");
            return;
        }

        switch (_type)
        { //0建筑 1hit 2侵入
            case 0:
                _Soldier.OnIntrusion(_Energy);
                break;
            case 1:
                if (_Soldier.CurEnergy <= 0)
                {
                    _Soldier.OnDead();
                    _star.showAttackSoldierEffect(_playerId, _Soldier.UnitGO.transform.localPosition);
                }
                else
                {
                    _Soldier.OnHit();
                    _star.showAttackSoldierEffect(_playerId, _Soldier.UnitGO.transform.localPosition);
                }

                break;
            case 2:
                _Soldier.OnIntrusion(_Energy);
                break;
        }
        //type 类型士兵全军覆没
        if (_Soldier.CurEnergy <= 0)
        {
            _star.RemoveSoldier(_playerId, _soldierType);
            _Soldier.ClearUnit();

            IsAttackStar(_star);
        }
        _star.CulAllSoldierAttack();

        m_starDataManager.StarOrEndStartFighting(_star);

    }

    /// <summary>
    /// 从对象池取 士兵
    /// </summary>
    /// <returns></returns>
    private UnitSoldier GetUnitSoldierFromPool()
    {
        foreach (UnitSoldier _soldier in m_SoldierList)
        {
            if (_soldier.UnitId == -1)
            {
                return _soldier;
            }
        }
        UnitSoldier _newSoldier = new UnitSoldier();
        m_SoldierList.Add(_newSoldier);
        return _newSoldier;
    }

    /// <summary>
    /// 更新 建筑生产
    /// </summary>
    public void UpdateBuildingProduction(float deltaTime)
    {
        m_starDataManager.UpdateBuildingProduction(deltaTime);
    }

    /// <param name="playerId">玩家id</param>
    /// <param name="startStarId">出发星球</param>
    /// <param name="endStarId">目标星球</param>
    /// <param name="isSelfProduce">1表示只出战自产兵，0表示全部出战</param>
    /// <param name="soldierTypeList">出战兵种id列表</param>
    /// <param name="percent">出战兵种数量百分比，0表示只出战一个</param>
    public void SendSoldier(LuaTable lt)
    {
        uint _playerId = Convert.ToUInt32(lt["0"]);
        UnitStar _StartStar = GetUnitStarById(Convert.ToInt32(lt["1"]));
        UnitStar _endStar = GetUnitStarById(Convert.ToInt32(lt["2"]));
        int isSelfProduce = Convert.ToInt32(lt["3"]);
        LuaTable soldierTypeluatable = lt["4"] as LuaTable;
        float percent = (float)Convert.ToDouble(lt["5"]);
        int _soldierType = 0;
        UnitSoldier _soldier;
        MapUtil.RoutePlanResult _result = MapUtil.Plan(_StartStar, _endStar);
        for (int i = 0; i < soldierTypeluatable.Count; i++)
        {
            _soldierType = Convert.ToInt32(soldierTypeluatable[i.ToString()]);

            _soldier = _StartStar.GetSoldierById(_playerId, _soldierType);

            if (_soldier == null)
            {
                continue;
            }

            if ( isSelfProduce == 1 && _soldier.SelfProduceEnergy <= 0)
            {
                continue;
            }

            _soldier = _StartStar.RemoveSoldier(_playerId, _soldierType);

            int _moveOrdinaryEnergy;
            int _OrdinaryEnergy = _soldier.CurEnergy - _soldier.SelfProduceEnergy;
            if (isSelfProduce == 0)//移动全部士兵
            {
                _moveOrdinaryEnergy = (int)(_OrdinaryEnergy * percent);
            }
            else
            {//移动自产士兵
                _moveOrdinaryEnergy = 0;
            }
            int _moveSelfProduceEnergy = (int)(_soldier.SelfProduceEnergy * percent);
            /*if (_soldier.BaseSoldierData.id == 1)
            {
                LoggerHelper.Error("_moveSelfProduceEnergy:" + _moveSelfProduceEnergy + " percent:" + percent + " _soldier.SelfProduceEnergy:" + _soldier.SelfProduceEnergy);
            }*/
            int _leftOrinaryEnergy = _OrdinaryEnergy - _moveOrdinaryEnergy;
            int _leftSelfProduceEnergy = _soldier.SelfProduceEnergy - _moveSelfProduceEnergy;
            _soldier.RemoveSoldier(_leftOrinaryEnergy, false);
            _soldier.RemoveSoldier(_leftSelfProduceEnergy);
            if (_leftOrinaryEnergy > 0)
            {
                addSoldierToStar(_playerId, _StartStar.UnitId, _soldierType, _leftOrinaryEnergy, 0, false);
            }
            if (_leftSelfProduceEnergy > 0)
            {
                addSoldierToStar(_playerId, _StartStar.UnitId, _soldierType, _leftSelfProduceEnergy, 1, false);
            }

            if (_soldier != null)
            {
                _soldier.MovePath = _result;
                MoveSoldierList.Add(_soldier);
            }
        }
    }

    public void RemoveArrivedSoldier(UnitSoldier _soldier)
    {
        if (MoveSoldierList.Contains(_soldier))
        {
            MoveSoldierList.Remove(_soldier);
            _soldier.ClearUnit();
        }
    }

    public void UpdateSoldierMoveTime(float deltaTime)
    {
        //int _len = MoveSoldierList.Count;
        if (MoveSoldierList.Count <= 0)
        {
            return;
        }

        //float _time = Time.deltaTime * 1000;
        for (int i = 0; i < MoveSoldierList.Count; i++)
        {
            MoveSoldierList[i].CulMoveTime(deltaTime);
        }
    }

    /// <summary>
    /// 星球大战 过程
    /// </summary>
    public void UpdateWar(float deltaTime)
    {
        m_starDataManager.UpdateWar(deltaTime);
    }

    /// <summary>
    /// 1 开始战斗  2结束战斗
    /// </summary>
    /// <param name="starid"></param>
    /// <param name="type"></param>
    public void ArrayedSoldier(int starid, int type)
    {
        m_starDataManager.ArrayedSoldier(starid, type);
    }

    /// <summary>
    /// 通过玩家id获得阵营信息
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public GroupData GetGroupByPalyerId(uint playerId)
    {
        //Debug.Log("playerId:" + playerId);

        EntityParent _entiy = MogoWorld.GetEntityById(playerId);

        if (_entiy == null)
        {
            return null;
        }
        //LoggerHelper.Error("playerId:" + playerId + "TeamId:" + _entiy.TeamId);
        if (GroupDic.ContainsKey(_entiy.TeamId))
        {
            return GroupDic[_entiy.TeamId];
        }

        GroupDic[_entiy.TeamId] = GroupData.dataMap[(int)_entiy.TeamId + 1];//初始化阵营

        return GroupDic[_entiy.TeamId];
    }

    /// <summary>
    /// 更新星球归属
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="starId"></param>
    public void UpdateStarBelongTo(uint playerId, int starId)
    {
        //Debug.Log("playerId:" + playerId + "   starId:" + starId);
        UnitStar updateStar = GetUnitStarById(starId);
        bool _isOccupy = false;
        if (updateStar.BelongTo != null && updateStar.BelongTo.ID != playerId && playerId == MogoWorld.thePlayer.ID)
        {
            //LoggerHelper.Error("ddd");
            _isOccupy = true;
        }

        GroupData data = GetGroupByPalyerId(playerId);//先设置阵营信息
        updateStar.SetGroup(data);
        if (MogoWorld.thePlayer.ID == playerId)
        {
            SoundManager.PlaySound("tree grow 1.ogg");
            updateStar.BelongTo = MogoWorld.thePlayer;
            if (CurStarID == starId)
                EventDispatcher.TriggerEvent(Events.StarUIEvent.AddBuildingEvent);//发送我方当前星球添加建筑的消息
            foreach (UnitStar star in m_starDataManager.StarList)//添加我方星球移动范围
            {
                if (updateStar.CheckStarReachByDistance(star) == true && ReachStarList.IndexOf(star) == -1)
                    ReachStarList.Add(star);
            }
        }
        else
        {
            if (updateStar.BelongTo != null && updateStar.BelongTo.ID == MogoWorld.thePlayer.ID)//之前是我方星球，被攻占了
                RemoveStarRangle(updateStar);
            updateStar.BelongTo = MogoWorld.Entities.Get(playerId);
        }
        //攻占成功
        if (AttackStar.Contains(updateStar))
        {
            if (!updateStar.IsInAttackStar())
            {
                AttackStar.Remove(updateStar);
            }
        }

        if (_isOccupy)//占领成功 自动送一个建筑
        {
            //updateStar.CurEnergy = updateStar.BaseData.energy / 2;
            //LoggerHelper.Error("占领成功 自动送一个建筑");
            //MogoWorld.thePlayer.RpcCall("AddBuilding", playerId, starId, 1, 100);//key为建筑类型
            
            updateStar.RecoveryBuilding();
        }

    }

    /// <summary>
    /// 移除我方星球移动范围
    /// </summary>
    /// <param name="updateStar"></param>
    private void RemoveStarRangle(UnitStar updateStar)
    {
        List<UnitStar> nonLinkUpdateStarList = new List<UnitStar>();
        List<UnitStar> linkUpdateStarList = new List<UnitStar>();
        foreach (UnitStar star in ReachStarList)
        {
            if (star.UnitId == updateStar.UnitId) continue;
            if (star.CheckStarReachByDistance(updateStar) == false)
                nonLinkUpdateStarList.Add(star);
            else
                linkUpdateStarList.Add(star);
        }
        bool isLink = false;

        foreach (UnitStar lStar in linkUpdateStarList)
        {
            isLink = false;
            foreach (UnitStar nStar in nonLinkUpdateStarList)
            {
                if (nStar.CheckStarReachByDistance(lStar) == true)
                    isLink = true;
            }
            if (isLink == false)
            {
                ReachStarList.Remove(lStar);
            }
        }

        nonLinkUpdateStarList = null;
        linkUpdateStarList = null;
    }

    private void ShowFloatText(string _text, float _posX, float _posY, int colorId, float duration = 1.5f)
    {
        BillboardManager.I.FloatText(_text, new Vector3(_posX, _posY, -3), colorId, duration);

        //BillboardManager.I.FloatText(_text, new Vector3(_posX, _posY, -3), duration);
    }

    /// <summary>
    /// 更新攻击星球
    /// </summary>
    /// <param name="deltaTime"></param>
    public void UpdateAttackStarTime(float deltaTime)
    {
        if (AttackStar.Count <= 0)
        {
            return;
        }

        AttackStarTime += deltaTime;

        if (AttackStarTime < MogoWorld.AttackStarFrequeny)
        {
            return;
        }

        AttackStarTime -= MogoWorld.AttackStarFrequeny;

        for (int i = 0; i < AttackStar.Count; i++)
        {
            AttackStar[i].AttackStar();
        }
    }

    public void PlayerAttackStar(uint attackPlayerId, uint starBelongPlayerId, int StarId, int energy)
    {
        UnitStar _star = GetUnitStarById(StarId);
        if (_star == null)
        {
            return;
        }
        //LoggerHelper.Error("attackPlayerId:" + attackPlayerId + "_star.BelongTo.ID:" + _star.BelongTo.ID);
        //自己攻击自己星球
        if ((_star.BelongTo != null) && (attackPlayerId == _star.BelongTo.ID))
        {
            return;
        }

        //LoggerHelper.Error("_star.CurEnergy:" + _star.CurEnergy + " energy:" + energy);
        _star.CurEnergy -= energy;
        if (_star.CurEnergy <= 0)
        {
            MogoWorld.thePlayer.RpcCall("UpdateStarBelongTo", attackPlayerId, StarId);
        }
        ShowFloatText(string.Concat(-energy), _star.PositionX, _star.PositionY, 1);
        if (CurStarID == StarId)//是当前星球，显示能量减少的过程
        {
            UIManager.I.GetUILogic<StarInfoUIMgr>().SetStarEnergy(_star.CurEnergy);
            if (_star.CurEnergy <= 0)
            {
                UIManager.I.GetUILogic<StarInfoUIMgr>().SetStarEnergy(_star.BaseData.energy);
            }
        }
    }

    /// <summary>
    /// 计算玩家分数
    /// </summary>
    public void CulPlayerScroe()
    {
        PlayerScore.Clear();

        //星球分数
        Dictionary<uint, int> _starScore = m_starDataManager.GetStarScore();

        foreach (var _key in _starScore.Keys)
        {
            AddScore(_key, _starScore[_key], 2);
        }

        //建筑数据
        //private List<UnitBuilding> m_BuildingList = new List<UnitBuilding>();
        for (int i = 0; i < m_BuildingList.Count; i++)
        {
            if ((m_BuildingList[i].BelongToStar != null) && (m_BuildingList[i].BelongToStar.BelongTo != null))
            {
                AddScore(m_BuildingList[i].BelongToStar.BelongTo.ID, m_BuildingList[i].BaseBuildingData.score, 0);
            }
        }
        //士兵数据
        //private List<UnitSoldier> m_SoldierList = new List<UnitSoldier>();
        for (int i = 0; i < m_SoldierList.Count; i++)
        {
            if (m_SoldierList[i].BelongTo != null)
            {
                AddScore(m_SoldierList[i].BelongTo.ID, m_SoldierList[i].BaseSoldierData.score * m_SoldierList[i].GetSoldierNum(), 1);
            }
        }

        //设置分数
        BattleScoreUIMgr _battleScore = UIManager.I.GetUILogic<BattleScoreUIMgr>();
        _battleScore.SetScoreRed((int)PlayerScore[MogoWorld.thePlayer.ID][3]);
        foreach (var _itme in PlayerScore.Keys)
        {
            if (_itme != MogoWorld.thePlayer.ID)
            {
                _battleScore.SetScoreBlue((int)PlayerScore[_itme][3]);
            }
        }
    }

    /// <summary>
    /// 0 星球  1建筑 2士兵 3 总分
    /// </summary>
    /// <param name="playerid"></param>
    /// <param name="score"></param>
    /// <param name="_type"></param>
    private void AddScore(uint playerid, int score, int _type)
    {
        if (!PlayerScore.ContainsKey(playerid))
        {
            PlayerScore[playerid] = new List<uint>();
            PlayerScore[playerid].Add(0);
            PlayerScore[playerid].Add(0);
            PlayerScore[playerid].Add(0);
            PlayerScore[playerid].Add(0);
        }
        PlayerScore[playerid][_type] += (uint)score;
        PlayerScore[playerid][3] += (uint)score;
    }

    public bool IsWin = true;

    public void GetScore(uint _playerId, List<UILabel> _label)
    {
        for (int i = 0; i < 4; i++)
        {
            _label[i].text = PlayerScore[_playerId][i].ToString();
        }
        if (_playerId != MogoWorld.thePlayer.ID)
        {
            IsWin = (PlayerScore[MogoWorld.thePlayer.ID][3] >= PlayerScore[_playerId][3]);
        }
    }




    /// <summary>
    /// 士兵攻击星球
    /// </summary>
    /// <param name="_starID"></param>
    /// <param name="_buildingTime"></param>
    public void ShowSoldierIntrusionStar(uint _playerId, int _starID, int _soldierType, int _Energy,float _PositionX,float _PositionY)
    {
        UnitStar _star = GetUnitStarById(_starID);
        UnitSoldier _Soldier =  GetUnitSoldierFromPool();
        SoldierIndex++;
        _Soldier.UnitId = SoldierIndex;
        _Soldier.BelongTo = MogoWorld.GetEntityById(_playerId);
        _Soldier.BaseSoldierData = SoldierData.dataMap.Get(_soldierType);
        _Soldier.PositionX = _PositionX;
        _Soldier.PositionY = _PositionY;
        _Soldier.UnitParent = SoldierLyaer;
        _Soldier.AddSoldier(_Energy, 1);
        _Soldier.BelongToStar = _star;
        _Soldier.IsMoveAnimation = false;
        _Soldier.IsShowSoldierIntrusionStar = true;
        _Soldier.InitUnit();
        _Soldier.SetGroup(GetGroupByPalyerId(_playerId));//设置阵营信息


    }

}