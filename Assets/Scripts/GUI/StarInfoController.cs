/*********************************************************
 * Copyright (C) 2016 广州，爱游
 *
 * 模块名：GUI
 * 创建者：李建辉
 * 修改者列表：
 * 创建日期：2016/4/29 14:58:22
 * 模块描述：管理和控制星球信息
 * 用法实例：
 *
 * *******************************************************/

using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

internal class StarInfoController
{
    private static StarInfoController instance;

    public static StarInfoController getInstance()
    {
        if (instance == null)
        {
            instance = new StarInfoController();
        }
        return instance;
    }

    private StarInfoController()
    {
        FightSoldierIdList = new List<int>();
        BuildingTipsDic = new Dictionary<int, object[]>();
        m_starInfoUIMgr = UIManager.I.GetUILogic<StarInfoUIMgr>();
        m_comfirmUIMgr = UIManager.I.GetUILogic<ComfirmUIMgr>();
        m_soldierFightUIMgr = UIManager.I.GetUILogic<SoldierFightUIMgr>();
    }

    public List<int> FightSoldierIdList;//当前可以参加战斗的士兵类型
    private Dictionary<int, object[]> BuildingTipsDic;//保存建筑tips信息，key为建筑id
    private StarInfoUIMgr m_starInfoUIMgr;//
    private ComfirmUIMgr m_comfirmUIMgr;//
    private SoldierFightUIMgr m_soldierFightUIMgr;//

    public object[] GetBuildingTipsById(int buildingId, float x, float y)
    {
        if (BuildingTipsDic.ContainsKey(buildingId) == true) return BuildingTipsDic[buildingId];

        object[] tipsContent = new object[] { "", "", null };
        BuildingData data = BuildingData.dataMap[buildingId];
        tipsContent[0] = LanguageData.GetContent(data.name);
        tipsContent[1] = LanguageData.GetContent(170, data.building_consume);
        if (data.attack > 0)//建筑自身有攻击力
        {
            tipsContent[1] = string.Concat(tipsContent[1], "\n", LanguageData.GetContent(183, data.attack));
        }
        if (data.soldier_id > 0)//有士兵产出
        {
            tipsContent[1] = string.Concat(tipsContent[1], "\n", LanguageData.GetContent(171, data.production_time, LanguageData.GetContent(SoldierData.dataMap[data.soldier_id].name)));
            SoldierData soldierData = SoldierData.dataMap[data.soldier_id];
            string attackObject;
            if (soldierData.attack_object.Count == 2)
                attackObject = string.Concat(LanguageData.GetContent(173), ",", LanguageData.GetContent(174));
            else
                attackObject = soldierData.attack_object[0] == 1 ? LanguageData.GetContent(173) : LanguageData.GetContent(174);
            tipsContent[1] = string.Concat(tipsContent[1], "\n", LanguageData.GetContent(soldierData.name), "\n",
                LanguageData.GetContent(172, soldierData.energy, soldierData.attack, soldierData.move), attackObject);
        }

        tipsContent[2] = new Vector3(x, y, 0);
        BuildingTipsDic[buildingId] = tipsContent;
        return tipsContent;
    }

    #region 通过拖拉快速攻击星球

    private UnitStar beginStar;
    private UnitStar endStar;

    public void QuicklyAttackTargetStar(UnitStar _beginStar, UnitStar _endStar)
    {
        beginStar = _beginStar;
        endStar = _endStar;
        List<UnitStar> starList = MogoWorld.m_dataMapManager.GetStarList();
        foreach (UnitStar star in starList)
        {
            star.MovingRange.SetActive(false);
            star.SelectSign.SetActive(false);
        }
        if (m_starInfoUIMgr.bShow) m_starInfoUIMgr.Close();

        m_starInfoUIMgr.curStar = beginStar;
        m_starInfoUIMgr.targetStar = endStar;
        m_starInfoUIMgr.AttackStatus = AttackStatusType.DisPatch;//准备攻击状态
        beginStar.m_currentTargetStar = null;
        MogoWorld.m_dataMapManager.CurStarID = beginStar.UnitId;

        UIManager.I.ShowUI<ComfirmUIMgr>(ShowComfirmUI);
        UIManager.I.ShowUI<SoldierFightUIMgr>();
    }

    private void ShowComfirmUI()
    {
        m_comfirmUIMgr.ShowAll(true);
        m_comfirmUIMgr.SetPosition(endStar.UnitGO.transform.localPosition);
    }

    #endregion 通过拖拉快速攻击星球

    #region 通过星球id展示战斗主面板

    private int showStarId;

    public void ShowStarInfoUI(int _showStarId)
    {
        showStarId = _showStarId;

        if (showStarId == MogoWorld.m_dataMapManager.CurStarID && m_starInfoUIMgr.AttackStatus != 0) return;//正在战斗或集合状态，点击的是当前星球id，不做操作
        if (m_starInfoUIMgr.AttackStatus == AttackStatusType.Gather && m_starInfoUIMgr.curStar != null && m_starInfoUIMgr.curStar.TogetherId != -1) return;//如果处于集合状态，且已经有集合点，只能执行删除集合点操作

        List<UnitStar> starList = MogoWorld.m_dataMapManager.GetStarList();
        foreach (UnitStar star in starList)
        {
            star.MovingRange.SetActive(false);
            star.SelectSign.SetActive(false);
        }
        if (m_comfirmUIMgr.bShow) m_comfirmUIMgr.Close();
        if (m_soldierFightUIMgr.bShow) m_soldierFightUIMgr.Close();

        if (m_starInfoUIMgr.AttackStatus == 0)//
            UIManager.I.ShowUI<StarInfoUIMgr>(SetStarInfo);
        else
            SetStarInfo();

        /*if (m_starInfoUIMgr.AttackStatus == AttackStatusType.DisPatch)//等待战斗
        {
            UIManager.I.ShowUI<SoldierFightUIMgr>();
        }*/
        if ((m_starInfoUIMgr.AttackStatus == AttackStatusType.DisPatch) || (m_starInfoUIMgr.AttackStatus == AttackStatusType.Gather))//等待战斗或集合
        {
            //LoggerHelper.Error("ShowComfirmBtn");
            UIManager.I.ShowUI<ComfirmUIMgr>(ShowComfirmBtn);
            UIManager.I.ShowUI<SoldierFightUIMgr>();
        }
    }

    private void SetStarInfo()
    {
        m_starInfoUIMgr.ShowStarInfoById(showStarId);
    }

    private void ShowComfirmBtn()
    {
        m_comfirmUIMgr.ShowAll(true);
        m_comfirmUIMgr.SetPosition(m_starInfoUIMgr.targetStar.UnitGO.transform.localPosition);
        if (m_starInfoUIMgr.AttackStatus == AttackStatusType.Gather)
        {
            m_comfirmUIMgr.ShowPercent(false);
            m_comfirmUIMgr.ShowReconSoldierBtn(false);
        }
    }

    #endregion 通过星球id展示战斗主面板

    #region 战斗UI调度和攻击逻辑

    public void AttackStar()
    {
        m_starInfoUIMgr.AttackStatus = AttackStatusType.DisPatch;
        m_starInfoUIMgr.ShowOwnerRangle(true);
        m_starInfoUIMgr.Close();
        UIManager.I.ShowUI<ComfirmUIMgr>(ShowCancelBtn);
    }

    public void GatherStar()
    {
        m_starInfoUIMgr.AttackStatus = AttackStatusType.Gather;
        m_starInfoUIMgr.ShowOwnerRangle(true);
        m_starInfoUIMgr.Close();
        UIManager.I.ShowUI<ComfirmUIMgr>(ShowCancelBtn);
        if (m_starInfoUIMgr.curStar.TogetherId != -1)//已经有集合点，显示删除按钮
            m_comfirmUIMgr.ShowDeleteBtn(true);
    }

    private void ShowCancelBtn()
    {
        if (m_starInfoUIMgr.AttackStatus == AttackStatusType.Gather) m_comfirmUIMgr.ShowPercent(false);
        m_comfirmUIMgr.ShowAttackCancelBtn(true);
        m_comfirmUIMgr.SetPosition(m_starInfoUIMgr.curStar.UnitGO.transform.localPosition);
    }

    public void DeleteGatherStar()
    {
        m_starInfoUIMgr.curStar.TogetherId = -1;
        ReturnNormalStatus();
    }

    public void AttackOrGatherToStar()
    {
        if (m_starInfoUIMgr.AttackStatus == AttackStatusType.DisPatch)//是调度
        {
            //检查是否选择了出战兵
            if (FightSoldierIdList.Count <= 0)
            {
                BillboardManager.I.FloatText(LanguageData.GetContent(181), new Vector3(m_starInfoUIMgr.curStar.PositionX, m_starInfoUIMgr.curStar.PositionY, -3), 1.5f);
                return;
            }
            LuaTable lt = MoveSoldierToLuaTable(MogoWorld.thePlayer.ID, m_starInfoUIMgr.curStar.UnitId, m_starInfoUIMgr.targetStar.UnitId, 0, soldierNumPercent, FightSoldierIdList);
            //LoggerHelper.Error("MoveSoldier 3");
            MogoWorld.thePlayer.RpcCall("MoveSoldier", lt);
        }
        else if (m_starInfoUIMgr.AttackStatus == AttackStatusType.Gather)//是集合
        {
            //要检查是否回路
            int tempTogetherId = m_starInfoUIMgr.curStar.TogetherId;//先缓存起来
            m_starInfoUIMgr.curStar.TogetherId = m_starInfoUIMgr.targetStar.UnitId;
            bool isLoop = false;
            UnitStar tempStar = m_starInfoUIMgr.curStar;
            while (tempStar.TogetherId != -1)//检查是否有回路
            {
                tempStar = MogoWorld.m_dataMapManager.GetUnitStarById(tempStar.TogetherId);
                if (tempStar.UnitId == m_starInfoUIMgr.curStar.UnitId)
                {
                    isLoop = true;
                    break;
                }
            }
            if (isLoop == true)
            {
                BillboardManager.I.FloatText(LanguageData.GetContent(180), new Vector3(m_starInfoUIMgr.curStar.PositionX, m_starInfoUIMgr.curStar.PositionY, -3), 1.5f);
                LoggerHelper.Debug("集合路径形成了回路！");
                m_starInfoUIMgr.curStar.TogetherId = tempTogetherId;
                ReturnNormalStatus();
                return;
            }
            m_starInfoUIMgr.curStar.TogetherId = m_starInfoUIMgr.targetStar.UnitId;
            LuaTable lt = MoveSoldierToLuaTable(MogoWorld.thePlayer.ID, m_starInfoUIMgr.curStar.UnitId, m_starInfoUIMgr.targetStar.UnitId, 0, soldierNumPercent, FightSoldierIdList);
            //LoggerHelper.Error("MoveSoldier 4");
            MogoWorld.thePlayer.RpcCall("MoveSoldier", lt);
        }
        ReturnNormalStatus();
    }

    public void ReturnNormalStatus()
    {
        if (m_starInfoUIMgr.AttackStatus == AttackStatusType.DisPatch)
            UIManager.I.CloseUI<SoldierFightUIMgr>();
        m_comfirmUIMgr.ShowAll(false);
        m_starInfoUIMgr.AttackStatus = 0;
        m_starInfoUIMgr.ShowOwnerRangle(false);
        m_starInfoUIMgr.curStar.HideAllLine();
        m_starInfoUIMgr.curStar.SelectSign.SetActive(true);//非攻击状态显示当前星球的选中状态和范围
        m_starInfoUIMgr.curStar.MovingRange.SetActive(true);
    }

    /// <summary>
    /// 发送一个侦察兵
    /// </summary>
    public void SendReconSoldier()
    {
        //检查是否选择了出战兵
        if (FightSoldierIdList.Count <= 0)
        {
            BillboardManager.I.FloatText(LanguageData.GetContent(181), new Vector3(m_starInfoUIMgr.curStar.PositionX, m_starInfoUIMgr.curStar.PositionY, -3), 1.5f);
            return;
        }
        foreach (SoldierData data in SoldierData.dataMap.Values)
        {
            UnitSoldier soldier = m_starInfoUIMgr.curStar.GetSoldierById(MogoWorld.thePlayer.ID, data.id);
            if (soldier != null)
            {
                soldierNumPercent = 1 - (float)data.energy / (float)soldier.CurEnergy;
                break;
            }
            soldierNumPercent = 1;
        }
        LuaTable lt = MoveSoldierToLuaTable(MogoWorld.thePlayer.ID, m_starInfoUIMgr.curStar.UnitId, m_starInfoUIMgr.targetStar.UnitId, 0, soldierNumPercent, SortReconSoldier(FightSoldierIdList));
        MogoWorld.thePlayer.RpcCall("MoveSoldier", lt);
    }

    /// <summary>
    /// 选择规则：以积分低的士兵优先，若积分相等的再以建筑消耗优先级高的优先
    /// </summary>
    /// <param name="_listSoldierIds"></param>
    /// <returns></returns>
    private List<int> SortReconSoldier(List<int> _listSoldierIds)
    {
        UnitSoldier _ReconSoldier = null;
        UnitSoldier _TempSoldier = null;
        for (int i = 0; i < _listSoldierIds.Count; i++)
        {
            if (_ReconSoldier == null)
            {
                _ReconSoldier = m_starInfoUIMgr.curStar.GetSoldierById(MogoWorld.thePlayer.ID, _listSoldierIds[i]);
            }
            else
            {
                _TempSoldier = m_starInfoUIMgr.curStar.GetSoldierById(MogoWorld.thePlayer.ID, _listSoldierIds[i]);
                if (_TempSoldier.BaseSoldierData.score < _ReconSoldier.BaseSoldierData.score)//优先传送分数低的士兵
                {
                    _ReconSoldier = _TempSoldier;
                }
                else if (_TempSoldier.BaseSoldierData.score == _ReconSoldier.BaseSoldierData.score)//若积分相等的再以建筑消耗优先级高的优先
                {
                    if (_TempSoldier.BaseSoldierData.building_priority > _ReconSoldier.BaseSoldierData.building_priority)
                    {
                        _ReconSoldier = _TempSoldier;
                    }
                }
            }
        }
        //LoggerHelper.Error("_ReconSoldier.BaseSoldierData.id:" + _ReconSoldier.BaseSoldierData.id);
        var _list = new List<int>();
        _list.Add(_ReconSoldier.BaseSoldierData.id);
        return _list;
    }

    public AttackStatusType GetCurAttackStatus()
    {
        return m_starInfoUIMgr.AttackStatus;
    }

    /// <summary>
    /// 获取星球可出战士兵id
    /// </summary>
    /// <param name="star"></param>
    /// <returns></returns>
    public List<int> GetStarFightSoliderIdList(UnitStar star)
    {
        List<int> fightSoldierIdList = new List<int>();
        UnitSoldier soldier;
        foreach (SoldierData data in SoldierData.dataMap.Values)
        {
            soldier = star.GetSoldierById(MogoWorld.thePlayer.ID, data.id);
            if (soldier != null)//当前有这种士兵
            {
                fightSoldierIdList.Add(data.id);
            }
        }
        return fightSoldierIdList;
    }

    #endregion 战斗UI调度和攻击逻辑

    #region 转化移动士兵参数为luatable

    /// <summary>
    ///
    /// </summary>
    /// <param name="playerId">玩家id</param>
    /// <param name="startStarId">出发星球</param>
    /// <param name="endStarId">目标星球</param>
    /// <param name="isSelfProduce">1表示只出战自产兵，0表示全部出战</param>
    /// <param name="soldierTypeList">出战兵种id列表</param>
    /// <param name="percent">出战各兵种统一数量百分比，0表示只出战一个</param>
    /// <returns></returns>
    public LuaTable MoveSoldierToLuaTable(uint playerId, int startStarId, int endStarId, int isSelfProduce, float _soldierNumPercent, List<int> fightSoldierIdList)
    {
        LuaTable lt = new LuaTable();
        lt.Add(0, playerId);
        lt.Add(1, startStarId);
        lt.Add(2, endStarId);
        lt.Add(3, isSelfProduce);

        LuaTable soldierTypeLuaTable = new LuaTable();
        for (int i = 0; i < fightSoldierIdList.Count; i++)
        {
            soldierTypeLuaTable.Add(i, fightSoldierIdList[i]);
        }

        lt.Add(4, soldierTypeLuaTable);
        lt.Add(5, 1 - _soldierNumPercent);
        return lt;
    }

    #endregion 转化移动士兵参数为luatable

    public void CloseUI()
    {
        if (m_starInfoUIMgr.AttackStatus == AttackStatusType.Normal)
        {
            m_starInfoUIMgr.HideUI();
            m_soldierFightUIMgr.Close();
            m_comfirmUIMgr.ShowAttackCancelBtn(false);
            MogoWorld.m_dataMapManager.CurStarID = -1;
        }
    }

    #region 设置攻击士兵数量百分比

    public float soldierNumPercent = 0;//剩余兵力百分比，0表示全部出战

    public void SetSoldierNumPercent(Vector3 targetPos)
    {
        float tanR = Mathf.Atan2(targetPos.y, targetPos.x);
        float loopR = 0;
        if (targetPos.x < 0 && targetPos.y > 0)
            loopR = tanR - Mathf.PI / 2;
        else
            loopR = Mathf.PI * 3 / 2 + tanR;
        soldierNumPercent = loopR / (Mathf.PI * 2);
        m_comfirmUIMgr.ShowPercent(true);
    }

    #endregion 设置攻击士兵数量百分比
}

/// <summary>
/// 战斗UI显示状态
/// </summary>
public enum AttackStatusType
{
    Normal = 0,//处于非攻击状态，显示星球信息
    DisPatch = 1,//处于调度攻击状态
    Gather = 2,//处于集合攻击状态
}