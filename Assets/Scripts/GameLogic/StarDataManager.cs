using Mogo.Game;
using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StarDataManager
{
    //星球数据
    private List<UnitStar> m_StarList = new List<UnitStar>();

    //存在战斗的星球
    private Dictionary<int, UnitStar> WarOfStarDic = new Dictionary<int, UnitStar>();
    private List<UnitStar> WarOfStarList = new List<UnitStar>();
    private float CombatTime = 0.0f;

    public List<UnitStar> StarList
    {
        get { return m_StarList; }
    }

    public void InitStarData(int _index,int _starId,float _PosX,float _PosY,Transform _parent,Action _callBack)
    {
        if (_index >= m_StarList.Count)
        {
            m_StarList.Add(new UnitStar());
        }
        UnitStar _unitStar = m_StarList[_index];
        _unitStar.UnitId = _index;
        _unitStar.BaseData = StarData.dataMap.Get(_starId);
        _unitStar.PositionX = _PosX;
        _unitStar.PositionY = _PosY;
        _unitStar.UnitParent = _parent;
        _unitStar.CallBack = _callBack;
        
        _unitStar.InitUnit();
    }

    /// <summary>
    /// 初始化星球跟当前范围内的所有星球的有向边
    /// </summary>
    public void InitStarEdge()
    {
        foreach (UnitStar star in m_StarList) 
        {
            star.InitEdge(m_StarList);
        }
    }


    /// <summary>
    /// 创建星球(此方法还没初始化星球数据和它跟其他星球的有向边)
    /// </summary>
    /// <returns></returns>
    public UnitStar CreateStar()
    {
        foreach (UnitStar _star in m_StarList)
        {
            if (_star.UnitId == -1)
            {
                return _star;
            }
        }
        UnitStar _newStar = new UnitStar();
        m_StarList.Add(_newStar);
        return _newStar;
    }

    /// <summary>
    /// 清除 星球数据
    /// </summary>
    public void ClearStarData()
    {
        foreach (UnitStar _star in m_StarList)
        {
            _star.ClearUnit();
            _star.IsExplore = false;
        }
    }

    /// <summary>
    /// 根据ID得到星球的动态数据
    /// </summary>
    /// <param name="_id"></param>
    /// <returns></returns>
    public UnitStar GetUnitStarById(int _id)
    {
        foreach (UnitStar _star in m_StarList)
        {
            if (_star.UnitId == _id)
            {
                return _star;
            }
        }
        return null;
    }


    /// <summary>
    /// 更新 建筑生产
    /// </summary>
    public void UpdateBuildingProduction(float deltaTime)
    {
        //int _len = m_StarList.Count;
        for (int i = 0; i < m_StarList.Count; i++)
        {
            m_StarList[i].UpdateBuildingProduction(deltaTime);
        }
    }

    /// <summary>
    ///  得到星球分数
    /// </summary>
    /// <param name="_playerid"></param>
    /// <returns></returns>
    public Dictionary<uint,int> GetStarScore()
    {
        Dictionary<uint, int> _score = new Dictionary<uint, int>();
        for (int i = 0; i < m_StarList.Count; i++)
        {
            if (m_StarList[i].BelongTo != null)
            {
                if (!_score.ContainsKey(m_StarList[i].BelongTo.ID))
                {
                    _score[m_StarList[i].BelongTo.ID] = 0;
                }
                _score[m_StarList[i].BelongTo.ID] += m_StarList[i].BaseData.score;
            }
        }
        return _score;
    }

    /// <summary>
    /// 星球是否开始战争
    /// </summary>
    /// <param name="_star"></param>
    public void StarOrEndStartFighting(UnitStar _star)
    {
        if (WarOfStarDic.ContainsKey(_star.UnitId))
        {
            //结束战斗
            if (!_star.HasOtherSoldier())
            {
                WarOfStarDic.Remove(_star.UnitId);
                WarOfStarList.Remove(_star);
                MogoWorld.thePlayer.RpcCall("ArrayedSoldier", _star.UnitId, 2);
            }
        }
        else
        {
            //LoggerHelper.Debug("主角的星球上:" + _star.UnitId);
            //在主角的星球上存在多个阵营的士兵
            if (_star.HasOtherSoldier())
            {
                WarOfStarDic.Add(_star.UnitId, _star);
                WarOfStarList.Add(_star);
                //LoggerHelper.Debug("WarOfStarDic:" + WarOfStarDic.Count + "WarOfStarList:" + WarOfStarList.Count);
                MogoWorld.thePlayer.RpcCall("ArrayedSoldier", _star.UnitId, 1);
            }
        }

    }

    /// <summary>
    /// 星球大战 过程
    /// </summary>
    public void UpdateWar(float deltaTime)
    {
        if (WarOfStarList.Count <= 0)
        {
            return;
        }

        CombatTime += deltaTime;

        if (CombatTime < MogoWorld.CombatFrequeny)
        {
            return;
        }
        //LoggerHelper.Error(" UpdateWar ");
        CombatTime -= MogoWorld.CombatFrequeny;
        //int _len = WarOfStarList.Count;
        //LoggerHelper.Error(" _len " + _len);
        for (int i = 0; i < WarOfStarList.Count; i++)
        {
            //LoggerHelper.Error(" UpdateWar ");
            WarOfStarList[i].FightingProcess();
        }
    }

    /// <summary>
    /// 1 开始战斗  2结束战斗
    /// </summary>
    /// <param name="starid"></param>
    /// <param name="type"></param>
    public void ArrayedSoldier(int starid, int type)
    {
        UnitStar _star = GetUnitStarById(starid);
        switch (type)
        {
            case 1:
                _star.ArrayedSoldier();
                break;

            case 2:
                _star.HomePosition();
                break;
        }
    }

    public void StarTogether(UnitStar _star)
    {
        //检查是否要集合
        if ((_star.TogetherId != -1)  && (_star.BelongTo.ID == MogoWorld.thePlayer.ID))
        {
            List<int> fightSoliderIdList = StarInfoController.getInstance().GetStarFightSoliderIdList(_star);

            int _nextTogetherId = TogetherExtends(_star);
            
            LuaTable lt = StarInfoController.getInstance().MoveSoldierToLuaTable(MogoWorld.thePlayer.ID,
                _star.UnitId, _nextTogetherId, 1, 0, fightSoliderIdList);
            MogoWorld.thePlayer.RpcCall("MoveSoldier", lt);
        }
    }

    /// <summary>
    /// 集合继承
    /// </summary>
    private int TogetherExtends(UnitStar firstStar)
    {
        List<int> _listStarId = new List<int>();
        int nowStarId = firstStar.UnitId;
        _listStarId.Add(firstStar.UnitId);
        while (true)
        {
            UnitStar _nowStar = GetUnitStarById(nowStarId);
            if(_nowStar.TogetherId < 0)
            {//没下个集合点
                return _nowStar.UnitId;
            }
            UnitStar _nextStar = GetUnitStarById(_nowStar.TogetherId);
            if (_nextStar.BelongTo != null && _nextStar.BelongTo.ID != _nowStar.BelongTo.ID)
            {//下个集合点不属于玩家
                return _nowStar.UnitId;
            }

            if (_listStarId.Contains(_nowStar.TogetherId))
            {//下个集合点 有 环
                return _nowStar.UnitId;
            }

            _listStarId.Add(_nowStar.TogetherId);
            nowStarId = _nowStar.TogetherId;
        }
    }

}
