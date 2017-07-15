using Mogo.Game;
using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStar : UnitBase
{
    public StarData BaseData;
    public GameObject MovingRange;
    public GameObject MovingArrow;
    private TrollDrawCircle m_movingRangeDrawLine;
    private TrollDrawCircle m_starRadiusDrawLine;
    private TrollDrawArrow m_movingDrawArrow;
    private UISprite StarColor;

    /// <summary>
    /// Edge的集合 -- 出边表(保存该星球到范围内星球的信息)
    /// </summary>
    public List<MapUtil.Edge> EdgeList = new List<MapUtil.Edge>();

    public Action CallBack;
    private MogoUIBtn UIBtn;

    /// <summary>
    /// 建筑数据（key：位置；value：建筑实体）
    /// </summary>
    private Dictionary<int, UnitBuilding> BuildingDic = new Dictionary<int, UnitBuilding>();

    /// <summary>
    /// 士兵数据（key：玩家id；value：（key：位置；value：士兵实体））
    /// </summary>
    public Dictionary<uint, Dictionary<int, UnitSoldier>> SoldierDic = new Dictionary<uint, Dictionary<int, UnitSoldier>>();

    /// <summary>
    /// 星球上总攻击（0全部 1建筑 2 士兵)
    /// </summary>
    private Dictionary<uint, List<uint>> SoldierAttack = new Dictionary<uint, List<uint>>();

    /// <summary>
    /// 根据被攻击优先级排序的士兵列表（key：玩家id；value：（key：位置；value：士兵实体））
    /// </summary>
    //private Dictionary<uint, List<KeyValuePair<int, UnitSoldier>>> AttackPriority = new Dictionary<uint, List<KeyValuePair<int, UnitSoldier>>>();
    private Dictionary<uint, SortedList<int, UnitSoldier>> AttackPriority = new Dictionary<uint, SortedList<int, UnitSoldier>>();


    /// <summary>
    /// 根据被攻击优先级排序的建筑列表（（key：位置；value：建筑实体））
    /// </summary>
    private SortedList<int, UnitBuilding> AttackBuildingPriorityList = new SortedList<int, UnitBuilding>(new DuplicateKeyDescendingComparer<int>());


    /// <summary>
    /// 建筑 消耗士兵 优先级
    /// </summary>
    private Dictionary<uint, SortedList<int, UnitSoldier>> BuildingPriorityList = new Dictionary<uint, SortedList<int, UnitSoldier>>();//SortedList<int, UnitSoldier>(new DuplicateKeyComparer<int>());

    private List<Vector2> BuildingPositionList = new List<Vector2>();
    private List<Vector2> SoliderPositionList = new List<Vector2>();

    private EntityParent StarBelongTo = null;
    private GameObject m_BanBGOB;

    /// <summary>
    /// 该星球生产兵力集合星球id，-1表示未有集合点
    /// </summary>
    public int TogetherId = -1;

    private bool m_isExploxe = false;

    private uint m_iFirstArried = 0;

    public GameObject SelectSign;//星球选中状态

    private StarInfoUIMgr m_starInfoUIMgr;//

    private float StarEnergyTime = 1.0f;//星球能量增加时间

    private List<int> BuildingLog = new List<int>();//存放建筑历史

    protected override string prefabName
    {
        get { return "StarUI.prefab"; }
    }

    public EntityParent BelongTo
    {
        get { return StarBelongTo; }
        set
        {

            if (StarBelongTo == null)
            {
                CurEnergy = BaseData.energy;
            }
            else
            {
                if (StarBelongTo.ID != value.ID)
                {
                    CurEnergy = BaseData.energy / 2;
                }
            }

            StarBelongTo = value;
            if (UIBtn == null)
            {
                return;
            }
            if ((StarBelongTo != null) && (StarBelongTo.ID == MogoWorld.thePlayer.ID))
            {
                //UIBtn.IsEnabled =true;
                m_BanBGOB.SetActive(false);
                //UIManager.I.GetUILogic<MapUIMgr>().EraserTexture(PositionX,PositionY,(BaseData.radius+BaseData.range));
            }
            else
            {
                //UIBtn.IsEnabled=false;
                m_BanBGOB.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 是否是探索过的星球
    /// </summary>
    public bool IsExplore
    {
        get { return m_isExploxe; }
        set
        {
            m_isExploxe = value;

            foreach (var _soldierList in SoldierDic.Values)
            {
                foreach (var _soldier in _soldierList.Values)
                {
                    _soldier.ShowUnit(m_isExploxe);
                }
            }

            foreach (var _building in BuildingDic.Values)
            {
                _building.ShowUnit(m_isExploxe);
            }

            SetGroupData();
        }
    }

    protected override void InitUnitData()
    {
        if (UIBtn == null)
        {
            UIBtn = UnitGO.transform.FindChild("StarModel/StarBtn").GetComponent<MogoUIBtn>();
            UIBtn.ClickAction = OnClickStar;
            m_BanBGOB = UnitGO.transform.FindChild("StarModel/BanBG").gameObject;
            BelongTo = StarBelongTo;
        }

        UnitGO.transform.parent = UnitParent;
        UnitGO.transform.localScale = new Vector3(1, 1, 1);
        UnitGO.transform.localPosition = new Vector3(PositionX, PositionY, 0);
        UnitGO.transform.FindChild("StarModel").localScale = new Vector3(BaseData.radius * 2, BaseData.radius * 2, -2);//两倍后才接近真实大小
        UnitGO.transform.FindChild("StarModel/StarBtn").GetComponent<BoxCollider>().size = Vector3.one;

        m_starInfoUIMgr = UIManager.I.GetUILogic<StarInfoUIMgr>();
        InitDrawings();

        StarColor = UnitGO.transform.FindChild("StarColor").GetComponent<UISprite>();
        StarColor.transform.localScale = new Vector3(BaseData.radius * 2, BaseData.radius * 2, 1);//两倍后才接近真实大小
        StarColor.gameObject.SetActive(false);

        UIBtn.m_imgNormal.spriteName = BaseData.icon;
        UIBtn.m_imgPressed.spriteName = BaseData.icon;
        TweenRotation.Begin(UIBtn.m_imgNormal.gameObject, MogoWorld.GameTime, Quaternion.Euler(new Vector3(0, 0, Utils.CreateRandom().Next(90, 270))));
        //得到星球上建筑的位置
        BuildingPositionList = MapUtil.GetBuildingPosList(this, BaseData.count);
        //得到士兵在星球上的位置
        SoliderPositionList = MapUtil.GetBuildingPosList(this, 6, BaseData.radius * 1.5f);

        if (CallBack != null)
        {
            CallBack();
            CallBack = null;
        }
    }

    protected override void ClearUnitData()
    {
        BelongTo = null;
        CallBack = null;
        m_iFirstArried = 0;
        /* foreach (var _item in BuildingDic)
         {
             RemoveBuilding(_item.Key, _item.Value);
         }*/
        BuildingDic.Clear();
        SoldierDic.Clear();
        AttackPriority.Clear();
    }

    private void InitDrawings()
    {
        MovingRange = UnitGO.transform.FindChild("MovingRange").gameObject;
        MovingArrow = UnitGO.transform.FindChild("MovingArrow").gameObject;
        SelectSign = GameObject.Instantiate(MovingRange) as GameObject;
        SelectSign.transform.parent = MovingRange.transform.parent;
        SelectSign.transform.localPosition = Vector3.zero;
        SelectSign.transform.localEulerAngles = Vector3.zero;
        SelectSign.transform.localScale = Vector3.one;
        SelectSign.SetActive(false);
        m_starRadiusDrawLine = SelectSign.AddComponent<TrollDrawCircle>();//星球大小
        m_starRadiusDrawLine.SetRadius(BaseData.radius);

        MovingRange.SetActive(false);
        m_movingRangeDrawLine = MovingRange.AddComponent<TrollDrawCircle>();//移动半径
        m_movingRangeDrawLine.SetRadius(BaseData.radius + BaseData.range);

        m_movingDrawArrow = MovingArrow.AddComponent<TrollDrawArrow>();//移动箭头
        m_movingDrawArrow.SetSourcePos(MovingArrow.transform.position);
        m_movingDrawArrow.SetSourceRadius(BaseData.radius);

        var opt = OptDragListener.Get(UIBtn.gameObject);
        opt.onDrag = UpdateArrow;
        opt.onPress = SetArrowVisible;

        var cam = UIManager.I.GetUILogic<MapUIMgr>().MyCameraController;
        cam.onDrag += OnCameraDrag;
        cam.onScale += OnCameraScaleChanged;
    }

    private Vector3 m_totalDelta;
    public UnitStar m_currentTargetStar;
    private List<UnitStar> m_currentPath = new List<UnitStar>();
    private float m_totleDeltaTime;

    public void DrawLineToStar(UnitStar target)
    {
        if (m_movingDrawArrow)
        {
            MovingArrow.SetActive(true);
            m_movingDrawArrow.SetSourcePos(UnitGO.transform.position);
            m_movingDrawArrow.DrawLineToTargetPoint(target.UnitGO.transform.position);
        }
    }

    public void DrawArrowToStar(UnitStar target)
    {
        if (m_movingDrawArrow)
        {
            MovingArrow.SetActive(true);
            m_movingDrawArrow.SetSourcePos(UnitGO.transform.position);
            m_movingDrawArrow.DrawArrowToTargetCircle(target.BaseData.radius, target.UnitGO.transform.position);
        }
    }

    public void HideAllLine()
    {
        if (m_currentPath.Count != 0)
        {
            for (int i = 0; i < m_currentPath.Count; i++)
            {
                m_currentPath[i].HideMovingArrow();
            }
            m_currentPath.Clear();
        }
    }

    public void SetCurrentPath(List<UnitStar> starPath)
    {
        m_currentPath = starPath;
    }

    public void HideMovingArrow()
    {
        MovingArrow.SetActive(false);
    }

    private void OnCameraDrag(GameObject go, Vector3 delta, Vector3 currentPos)
    {
        DrawAllPathLine();
    }

    private void SetArrowVisible(GameObject go, bool isPress)
    {
        if (!m_isExploxe) return;//还未探索，不做操作
        if (this.UnitId == MogoWorld.m_dataMapManager.CurStarID && UIManager.I.GetUILogic<StarInfoUIMgr>().AttackStatus != 0) return;//目标是自己的星球，不做操作
        m_totalDelta = Vector3.zero;
        m_movingDrawArrow.SetSourcePos(go.transform.position);
        m_movingDrawArrow.DrawLineToTargetPoint(go.transform.position);
        MovingArrow.SetActive(isPress);
        HideOtherLines();

        if (!isPress && m_currentTargetStar == null)
            m_starInfoUIMgr.ShowOwnerRangle(false, true);

        if (!isPress && m_currentTargetStar != null)
        {
            UIManager.I.GetUILogic<MapUIMgr>().MyCameraController.MoveTo(new Vector3(-m_currentTargetStar.PositionX, -m_currentTargetStar.PositionY, 0));
            //var cam = UIManager.I.GetUILogic<MapUIMgr>().MyCameraController;
            //TweenPosition.Begin(cam.gameObject, 0.2f, new Vector3(-m_currentTargetStar.PositionX, -m_currentTargetStar.PositionY, 0));
            StarInfoController.getInstance().QuicklyAttackTargetStar(this, m_currentTargetStar);
            MovingRange.SetActive(true);
            SelectSign.SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏有路径出现时除了初始星球的线以外的线，可重复调用
    /// </summary>
    private void HideOtherLines()
    {
        if (m_currentPath.Count != 0)
        {
            for (int i = 1; i < m_currentPath.Count; i++)
            {
                m_currentPath[i].HideMovingArrow();
            }
            m_currentPath.Clear();
        }
    }

    /// <summary>
    /// 根据星球路线画出箭头
    /// </summary>
    public void DrawAllPathLine()
    {
        if (m_currentPath.Count > 1)
        {
            for (int j = 0; j < m_currentPath.Count - 1; j++)
            {
                m_currentPath[j].DrawLineToStar(m_currentPath[j + 1]);
            }

            m_currentPath[m_currentPath.Count - 2].DrawArrowToStar(m_currentPath[m_currentPath.Count - 1]);
        }
    }

    private void UpdateArrow(GameObject go, Vector3 delta, Vector3 currentPos)
    {
        if (!m_isExploxe) return;//还未探索，不做操作
        m_totalDelta += delta;
        var pos = go.transform.position;
        var targetPos = pos + m_totalDelta;
        if (m_movingDrawArrow)
        {
            m_movingDrawArrow.SetSourcePos(pos);

            if (m_currentTargetStar != null)//如果缓存的目标星球不为空，则每帧检测有没有拖出范围，保证效果
            {
                var starPos = m_currentTargetStar.UnitGO.transform.position;
                if (Vector3.Distance(targetPos, starPos) < m_currentTargetStar.BaseData.radius * 1.5)
                {
                    return;
                }
            }

            if (Time.time - m_totleDeltaTime > 0.2f)//隔固定时间检测有没有箭头顶点有没有在可达星球范围内
            {
                m_totleDeltaTime = Time.time;
                for (int i = 0; i < MogoWorld.m_dataMapManager.ReachStarList.Count; i++)
                {
                    var star = MogoWorld.m_dataMapManager.ReachStarList[i];
                    if (star == this)
                        continue;
                    var starPos = star.UnitGO.transform.position;
                    //Debug.Log(Vector3.Distance(targetPos, starPos));
                    if (Vector3.Distance(targetPos, starPos) < star.BaseData.radius * 1.5)
                    {
                        m_currentPath = MapUtil.Plan(this, star).PassedStars;
                        m_currentTargetStar = star;

                        DrawAllPathLine();
                        return;
                    }
                }
            }
            //都没有就跟着移动坐标画
            m_movingDrawArrow.DrawArrowToTargetPoint(targetPos);
            m_currentTargetStar = null;

            HideOtherLines();

            //var cam = UIManager.I.GetUILogic<MapUIMgr>().MyCameraController;
            ////cam.transform.localPosition -= delta;
            //TweenPosition.Begin(cam.gameObject, 0.2f, new Vector3(-targetPos.x, -targetPos.y, 0));

            m_starInfoUIMgr.ShowOwnerRangle(true, true);
        }
    }

    private void OnCameraScaleChanged(GameObject go, float scale)
    {
        if (m_movingRangeDrawLine)
            m_movingRangeDrawLine.SetRadius((BaseData.radius + BaseData.range) * scale);
        if (m_starRadiusDrawLine)
            m_starRadiusDrawLine.SetRadius(BaseData.radius * scale);
        if (m_movingDrawArrow)
            m_movingDrawArrow.SetSourceRadius(BaseData.radius * scale);
    }

    private void OnClickStar(MogoUIBtn btn)
    {
        StarInfoController.getInstance().ShowStarInfoUI(UnitId);
        if (StarInfoController.getInstance().GetCurAttackStatus() == 0)
        {
            MovingRange.SetActive(true);
            SelectSign.SetActive(true);
        }
    }

    /// <summary>
    /// 初始化该星球跟在其范围内的所有星球的权值
    /// </summary>
    /// <param name="starList"></param>
    public void InitEdge(List<UnitStar> starList)
    {
        foreach (UnitStar star in starList)
        {
            if (star.UnitId == UnitId)
                continue;
            if (CheckStarReachByDistance(star) == false)
                continue;
            MapUtil.Edge edge = new MapUtil.Edge();
            edge.StartNodeID = UnitId;
            edge.EndNodeID = star.UnitId;
            edge.Weight = GetTargetStarDistance(star);//算出两个星球的距离
            EdgeList.Add(edge);
        }
    }

    /// <summary>
    /// 通过多个星球移动范围检查节点是否能到达目标节点(是否可连通)
    /// </summary>
    /// <param name="targetNode"></param>
    /// <returns></returns>
    public bool CheckStarReachByRangle(UnitStar targetStar)
    {
        if (MogoWorld.m_dataMapManager.ReachStarList.IndexOf(targetStar) != -1)
            return true;

        return false;
    }

    /// <summary>
    /// 检查节点是否能到达目标节点(是否可连通)
    /// </summary>
    /// <param name="targetNode"></param>
    /// <returns></returns>
    public bool CheckStarReachByDistance(UnitStar targetStar)
    {
        bool canReach = false;
        double dis = GetTargetStarDistance(targetStar);
        if (dis <= BaseData.radius + BaseData.range + targetStar.BaseData.radius)
        {
            canReach = true;
        }

        return canReach;
    }

    private Dictionary<int, double> starDistance = new Dictionary<int, double>();

    public double GetTargetStarDistance(UnitStar targetStar)
    {
        if (!starDistance.ContainsKey(targetStar.UnitId))
        {
            starDistance[targetStar.UnitId] = Mathf.Sqrt(Mathf.Pow(PositionX - targetStar.PositionX, 2) + Mathf.Pow(PositionY - targetStar.PositionY, 2));
        }
        return starDistance[targetStar.UnitId];
    }

    /// <summary>
    /// 得到可以修建建筑的位置
    /// </summary>
    /// <returns></returns>
    public int GetAddBuildingIndex()
    {
        for (int i = 0; i < BaseData.count; i++)
        {
            if (BuildingDic.ContainsKey(i))
            {
                if (BuildingDic[i] == null)
                {
                    return i;
                }
            }
            else
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 返回第index个建筑的位置
    /// </summary>
    /// <param name="_type"></param>
    /// <returns></returns>
    public Vector2 GetAddBuildingPos(int _index)
    {
        if (_index >= BuildingPositionList.Count)
        {
            //LoggerHelper.Error("_index:" + _index + "BuildingPositionList.Count:" + BuildingPositionList.Count);
            return new Vector2(0, 0);
        }
        return BuildingPositionList[_index];
    }

    public void AddBuilding(int _key, UnitBuilding _building)
    {
        BuildingDic[_key] = _building;
        _building.Key = _key;
        _building.ShowUnit(m_isExploxe);
        AttackBuildingPriorityList.Add(_building.BaseBuildingData.attack_priority, _building);
    }

    public void RemoveBuilding(int _key, UnitBuilding _building)
    {
        SoundManager.PlaySound("021_TreeDeath_01.ogg");
        _building.ClearUnit();
        _building.UnitGO.transform.localScale = new Vector3(0.1f, 0.1f, 1);
        BuildingDic.Remove(_key);

        int _index = AttackBuildingPriorityList.IndexOfValue(_building);
        if (_index >= 0)
        {
            AttackBuildingPriorityList.RemoveAt(_index);
        }

    }

    public UnitBuilding GetBuildingByIndex(int _index)
    {
        if (BuildingDic.ContainsKey(_index))
        {
            return BuildingDic[_index];
        }
        return null;
    }

    public UnitBuilding GetBuildingById(int id)
    {
        foreach (UnitBuilding building in BuildingDic.Values)
        {
            if (building.BaseBuildingData.id == id)
            {
                return building;
            }
        }
        return null;
    }

    public UnitSoldier GetSoldierById(uint playerId, int _soldierType)
    {
        //LoggerHelper.Error("player:" + playerId + "_soldierType:" + _soldierType);
        if (SoldierDic.ContainsKey(playerId))
        {
            if (SoldierDic[playerId].ContainsKey(_soldierType))
            {
                return SoldierDic[playerId][_soldierType];
            }
        }
        return null;
    }

    public void AddSoldier(uint _playerId, int _soldierType, UnitSoldier _soldier)
    {
        //第一个到达
        if ((m_iFirstArried <= 0) && (SoldierDic.Count <= 0))
        {
            m_iFirstArried = _playerId;
        }

        if (!SoldierDic.ContainsKey(_playerId))
        {
            SoldierDic[_playerId] = new Dictionary<int, UnitSoldier>();
        }
        //LoggerHelper.Error("m_isExploxe:" + m_isExploxe);
        _soldier.ShowUnit(m_isExploxe);
        SoldierDic[_playerId][_soldierType] = _soldier;

        /*foreach (var _item in SoldierDic)
        {
            AttackPriority[_item.Key] = new List<KeyValuePair<int, UnitSoldier>>(_item.Value);
            AttackPriority[_item.Key].Sort(delegate(KeyValuePair<int, UnitSoldier> s1, KeyValuePair<int, UnitSoldier> s2)
            { return s2.Value.BaseSoldierData.attack_priority.CompareTo(s1.Value.BaseSoldierData.attack_priority); });
        }*/
        if (!AttackPriority.ContainsKey(_playerId))
        {
            AttackPriority[_playerId] = new SortedList<int, UnitSoldier>(new DuplicateKeyDescendingComparer<int>());
        }
        if (!AttackPriority[_playerId].ContainsValue(_soldier))
        {
            AttackPriority[_playerId].Add(_soldier.BaseSoldierData.attack_priority, _soldier);
        }
        if (!BuildingPriorityList.ContainsKey(_playerId))
        {
            BuildingPriorityList[_playerId] = new SortedList<int, UnitSoldier>(new DuplicateKeyDescendingComparer<int>());
        }
        if (!BuildingPriorityList[_playerId].ContainsValue(_soldier))
        {
            BuildingPriorityList[_playerId].Add(_soldier.BaseSoldierData.building_priority, _soldier);
        }
        //LoggerHelper.Debug("UnitId:" + UnitId + "_playerId:" + _playerId + "MogoWorld.thePlayer.ID:" + MogoWorld.thePlayer.ID);
        //这个星球探索过了
        //LoggerHelper.Error("IsExplore:" + IsExplore + " _playerid:" + _playerId + " MogoWorld.thePlayer.ID:" + MogoWorld.thePlayer.ID);
        if (!IsExplore && (_playerId == MogoWorld.thePlayer.ID))
        {
            IsExplore = true;
        }

        //foreach (var _item in BuildingPriorityList[_playerId])
        //{
        //    LoggerHelper.Debug("_key:" + _item.Key);
        //}
    }

    public UnitSoldier RemoveSoldier(uint _playerId, int _soldierID)
    {
        UnitSoldier _soldier = null;

        if (SoldierDic.ContainsKey(_playerId))
        {
            if (SoldierDic[_playerId].ContainsKey(_soldierID))
            {
                _soldier = SoldierDic[_playerId][_soldierID];
                SoldierDic[_playerId].Remove(_soldierID);
                if (SoldierDic[_playerId].Count <= 0)
                {
                    SoldierDic.Remove(_playerId);
                }
            }
        }

        if (AttackPriority.ContainsKey(_playerId))
        {
            int _index = AttackPriority[_playerId].IndexOfValue(_soldier);
            //LoggerHelper.Error("_playerId:"+_playerId + "_index:" + _index);
            if (_index >= 0)
            {
                AttackPriority[_playerId].RemoveAt(_index);
            }
        }

        if (BuildingPriorityList.ContainsKey(_playerId))
        {
            int _index = BuildingPriorityList[_playerId].IndexOfValue(_soldier);
            if (_index >= 0)
            {
                BuildingPriorityList[_playerId].RemoveAt(_index);
            }
        }

        return _soldier;
    }

    /// <summary>
    /// 计算星球上各方总战力
    /// </summary>
    public void CulAllSoldierAttack()
    {
        Dictionary<int, UnitSoldier> _soldiers;
        foreach (var _playerId in SoldierDic.Keys)
        {
            if (!SoldierAttack.ContainsKey(_playerId))
            {
                SoldierAttack[_playerId] = new List<uint>();
            }
            SoldierAttack[_playerId].Clear();
            _soldiers = SoldierDic[_playerId];
            uint allAttack = 0;
            uint buildingAttack = 0;
            uint soldierAttack = 0;
            foreach (var _soldierItem in _soldiers.Values)
            {
                if (_soldierItem.BaseSoldierData.attack_object.Count > 2)
                {
                    allAttack += CulSoldierAttack(_soldierItem);
                }
                else
                {
                    if (_soldierItem.BaseSoldierData.attack_object[1] == 1)//只攻击建筑
                    {
                        buildingAttack += CulSoldierAttack(_soldierItem);
                    }
                    else
                    {//只攻击士兵
                        soldierAttack += CulSoldierAttack(_soldierItem);
                    }
                }
            }

            if ((BelongTo != null) && (BelongTo.ID == _playerId))//建筑攻击力
            {
                int _len = BuildingDic.Count;
                foreach (var _value in BuildingDic.Values)
                {
                    soldierAttack += (uint)_value.BaseBuildingData.attack;
                }
            }

            SoldierAttack[_playerId].Add(allAttack);
            SoldierAttack[_playerId].Add(buildingAttack);
            SoldierAttack[_playerId].Add(soldierAttack);
        }
    }

    private uint CulSoldierAttack(UnitSoldier _soldier)
    {
        return (uint)(_soldier.BaseSoldierData.attack * _soldier.GetSoldierNum());
    }

    /// <summary>
    /// 获取我方星球上的士兵数量
    /// </summary>
    /// <returns></returns>
    public int GetSoldierNum()
    {
        int num = 0;

        if (!SoldierDic.ContainsKey(MogoWorld.thePlayer.ID))
        {
            return num;
        }
        foreach (var soldier in SoldierDic[MogoWorld.thePlayer.ID].Values)
        {
            num += soldier.GetSoldierNum();
        }
        return num;
    }

    /// <summary>
    /// 获取我方星球上的士兵能量
    /// </summary>
    /// <returns></returns>
    public int GetSoldierEnergy()
    {
        int num = 0;

        if (!SoldierDic.ContainsKey(MogoWorld.thePlayer.ID))
        {
            return num;
        }
        foreach (var soldier in SoldierDic[MogoWorld.thePlayer.ID].Values)
        {
            num += soldier.CurEnergy;
        }
        return num;
    }

    /// <summary>
    /// 获取星球上的建筑数量
    /// </summary>
    /// <returns></returns>
    public int GetBuildingNum()
    {
        return BuildingDic.Count;
    }

    public int GetSoliderBuildingNum()
    {
        int num = 0;
        foreach (int key in BuildingDic.Keys)
        {
            if (BuildingDic[key] != null && BuildingDic[key].BaseBuildingData.soldier_id > 0) num++;
        }
        return num;
    }

    public Vector2 GetSoliderPositionById(int _id)
    {
        if (_id <= SoliderPositionList.Count)
        {
            return SoliderPositionList[_id - 1];
        }
        return new Vector2(0, 0);
    }

    public void UpdateBuildingProduction(float _dtTime)
    {
        if ((BelongTo != null) && (BelongTo.ID == MogoWorld.thePlayer.ID))
        {
            //LoggerHelper.Error("BuildingDic.cunt:" + BuildingDic.Count);
            foreach (var building in BuildingDic.Values)
            {
                if (building != null)
                {
                    building.BuildingProcess(_dtTime, UnitId);
                }
            }
        }

        UpdateStarEnergy(_dtTime);
    }

    public bool HasOtherSoldier()
    {
        ///有主星球
        if ((BelongTo != null) && (BelongTo.ID == MogoWorld.thePlayer.ID))
        {
            /*if (UnitId == 1)
            {
                LoggerHelper.Error("HasOtherSoldier 1");
            }*/
            return HasOtherCampSoldier();
        }
        //无主星球，第一个到达玩家
        if ((BelongTo == null) && (MogoWorld.thePlayer.ID == m_iFirstArried))
        {
            /*if (UnitId == 1)
            {
                LoggerHelper.Error("HasOtherSoldier 2");
            }*/
            return HasOtherCampSoldier();
        }

        return false;
    }

    /// <summary>
    /// 是否有其它阵营士兵
    /// </summary>
    /// <returns></returns>
    private bool HasOtherCampSoldier()
    {
        /*if (UnitId == 1)
        {
            LoggerHelper.Error("HasOtherSoldier 3");
        }*/
        if (SoldierDic.Count < 1)
        {
            /*if (UnitId == 1)
            {
                LoggerHelper.Error("HasOtherSoldier 4");
            }*/
            return false;
        }

        uint _teamId = 999;
        if (BelongTo != null)
        {
            _teamId = MogoWorld.thePlayer.TeamId;
        }
        EntityParent _entity;
        foreach (uint _key in SoldierDic.Keys)
        {
            if (MogoWorld.Entities.ContainsKey(_key))
            {
                _entity = MogoWorld.Entities.Get(_key);

            }
            else
            {
                _entity = MogoWorld.thePlayer;
            }

            if (_teamId == 999)
            {
                _teamId = _entity.TeamId;
            }
            else
            {
                if (_teamId != _entity.TeamId)
                {
                    /*if (UnitId == 1)
                    {
                        LoggerHelper.Error("HasOtherSoldier 6");
                    }*/
                    return true;
                }
            }
        }
        /*if (UnitId == 1)
        {
            LoggerHelper.Error("HasOtherSoldier 5");
        }*/
        return false;
    }

    /// <summary>
    /// 摆阵
    /// </summary>
    public void ArrayedSoldier()
    {
        int _indexX = 0;
        int _MyindexX = 0;
        int _indexY = -3;
        Dictionary<int, UnitSoldier> dic;
        EntityParent _entity;
        foreach (var _soldiers in SoldierDic)
        {
            _entity = MogoWorld.GetEntityById(_soldiers.Key);

            dic = _soldiers.Value;
            if (_entity.TeamId == MogoWorld.thePlayer.TeamId)
            {
                _MyindexX++;
                _indexY = -3;
                foreach (var _soldierItem in dic.Values)
                {
                    var x = PositionX - 100 * _MyindexX;
                    var y = PositionY - _indexY * 30;
                    _soldierItem.Move(x, y, 0.1f);
                    //_soldierItem.OnAttack(new Vector3(x + 200, y, 0));
                    _indexY++;
                }
            }
            else
            {
                _indexX++;
                _indexY = -3;
                foreach (var _soldierItem in dic.Values)
                {
                    var x = PositionX + 100 * _indexX;
                    var y = PositionY - _indexY * 30;
                    _soldierItem.Move(x, y, 0.1f);
                    //_soldierItem.OnAttack(new Vector3(x - 200, y, 0));
                    _indexY++;
                }
            }
        }
    }

    /// <summary>
    /// 结束战斗 士兵回复原位
    /// </summary>
    public void HomePosition()
    {
        Dictionary<int, UnitSoldier> dic;
        foreach (var _soldiers in SoldierDic)
        {
            dic = _soldiers.Value;
            foreach (var _soldier in dic)
            {
                Vector2 _vec = SoliderPositionList[_soldier.Key - 1];
                _soldier.Value.Move(_vec.x, _vec.y);
                //_soldier.Value.IsInFightingScale(false);
            }
        }
    }

    private List<uint> fightingRPC = new List<uint>();

    /// <summary>
    /// 战斗过程
    /// </summary>
    public void FightingProcess()
    {
        fightingRPC.Clear();
        List<uint> _attack;
        EntityParent attackEntity;
        EntityParent getHitEntity;
        foreach (var _key in SoldierAttack.Keys)
        {
            _attack = SoldierAttack[_key];
            /*  if (UnitId == 1)
              {
                  LoggerHelper.Error("_key:" + _key + "_attack[0]:" + _attack[0] + "_attack[1]:" + _attack[1] +
                      "_attack[2]:" + _attack[2]);
              }*/
            ///攻击者
            if (MogoWorld.Entities.ContainsKey(_key))
            {
                attackEntity = MogoWorld.Entities.Get(_key);
            }
            else
            {
                attackEntity = MogoWorld.thePlayer;
            }

            foreach (var _enemyKey in AttackPriority.Keys)
            {
                ///受击者
                if (MogoWorld.Entities.ContainsKey(_enemyKey))
                {
                    getHitEntity = MogoWorld.Entities.Get(_enemyKey);
                }
                else
                {
                    getHitEntity = MogoWorld.thePlayer;
                }

                ///不是同一个队伍
                if (attackEntity.TeamId != getHitEntity.TeamId)
                {
                    bool _isRunOut = false;
                    uint _attackFirst = (uint)(_attack[2] / MogoWorld.CombatNumberInSecond);
                    //LoggerHelper.Error("_attackFirst 0:" + _attackFirst);
                    //LoggerHelper.Error("AttackPriority[_enemyKey]:" + AttackPriority[_enemyKey].Count);
                    foreach (var _KeyValuelist in AttackPriority[_enemyKey])//士兵的战斗
                    {
                        if ((_KeyValuelist.Value.CurEnergy <= 0) || (_KeyValuelist.Value.UnitParent == null))
                        {
                            continue;
                        }
                        if (_attackFirst < _KeyValuelist.Value.CurEnergy)//只攻击士兵 的战力消耗
                        {
                            _isRunOut = true;
                            _attackFirst += (uint)(_attack[0] / MogoWorld.CombatNumberInSecond);
                            //LoggerHelper.Error("_attackFirst 1:" + _attackFirst);
                            if (_attackFirst < _KeyValuelist.Value.CurEnergy)//两栖士兵 的战力消耗
                            {
                                //LoggerHelper.Error("11111_enemyKey:" + _enemyKey + " AttackPriority[_enemyKey]:" + AttackPriority[_enemyKey].Count);
                                fightingRPC.Add(1);
                                fightingRPC.Add(_enemyKey);
                                fightingRPC.Add((uint)UnitId);
                                fightingRPC.Add((uint)_KeyValuelist.Value.BaseSoldierData.id);
                                fightingRPC.Add(_attackFirst);
                                _attackFirst = 0;
                                //LoggerHelper.Error("_attackFirst 2:" + _attackFirst);
                                break;
                            }
                        }
                        //LoggerHelper.Error("22222_enemyKey:" + _enemyKey + " AttackPriority[_enemyKey]:" + AttackPriority[_enemyKey].Count);
                        //LoggerHelper.Error("22222_attackFirst:" + _attackFirst + " _KeyValuelist.Value.CurEnergy:" + _KeyValuelist.Value.CurEnergy);
                        _attackFirst -= (uint)_KeyValuelist.Value.CurEnergy;
                        //LoggerHelper.Error("_attackFirst 3:" + _attackFirst);
                        fightingRPC.Add(1);
                        fightingRPC.Add(_enemyKey);
                        fightingRPC.Add((uint)UnitId);
                        fightingRPC.Add((uint)_KeyValuelist.Value.BaseSoldierData.id);
                        fightingRPC.Add((uint)_KeyValuelist.Value.CurEnergy);
                    }
                    //LoggerHelper.Error("_isRunOut:" + _isRunOut);
                    if (!_isRunOut)
                    {
                        _attackFirst = (uint)(_attack[0] / MogoWorld.CombatNumberInSecond);
                    }

                    if ((BelongTo != null) && (_key != BelongTo.ID))//这星球上的建筑不是我的：打
                    {
                        //LoggerHelper.Error("FightingProcess1:" + _attackFirst);
                        _attackFirst += (uint)(_attack[1] / MogoWorld.CombatNumberInSecond);
                        //LoggerHelper.Error("_attackFirst 4:" + _attackFirst);
                        //LoggerHelper.Error(_attack[1] + "FightingProcess2:" + _attackFirst);
                        int _len = BuildingDic.Count;

                        foreach (var _itemBuilding in AttackBuildingPriorityList.Values)
                        {
                            if ((_itemBuilding.CurEnergy * _len) < _attackFirst)
                            {
                                //LoggerHelper.Error(_itemBuilding.CurEnergy * _len + "FightingProcess4:" + _attackFirst);
                                _attackFirst -= (uint)_itemBuilding.CurEnergy;
                                //LoggerHelper.Error("_attackFirst 6:" + _attackFirst);
                                //MogoWorld.thePlayer.RpcCall("RemoveBuilding", BelongTo.ID, UnitId, _Buildingkey, BuildingDic[_Buildingkey].CurEnergy);
                                fightingRPC.Add(2);
                                fightingRPC.Add(_key);
                                fightingRPC.Add((uint)UnitId);
                                fightingRPC.Add((uint)_itemBuilding.Key);
                                fightingRPC.Add((uint)_itemBuilding.CurEnergy);
                            }
                            else
                            {
                                //MogoWorld.thePlayer.RpcCall("RemoveBuilding", BelongTo.ID, UnitId, _Buildingkey, _attackFirst);
                                fightingRPC.Add(2);
                                fightingRPC.Add(_key);
                                fightingRPC.Add((uint)UnitId);
                                fightingRPC.Add((uint)_itemBuilding.Key);
                                fightingRPC.Add((uint)_attackFirst);

                                _attackFirst = 0;
                                //LoggerHelper.Error("_attackFirst 5:" + _attackFirst);
                            }
                        }
                    }
                }
            }
        }
        SendMsg();
    }

    private void SendMsg()
    {
        for (int i = 0; i < fightingRPC.Count; i = i + 5)
        {
            /*LoggerHelper.Error("i:" + i + "fightingRPC[i]:" + fightingRPC[i] + "fightingRPC[i+1]:" + fightingRPC[i + 1] +
                "fightingRPC[i+2]:" + fightingRPC[i + 2] + "fightingRPC[i+3]:"
                + fightingRPC[i + 3]);*/
            if (fightingRPC[i + 4] <= 0)
            {
                continue;
            }
            if (fightingRPC[i] == 1)
            {
                MogoWorld.thePlayer.RpcCall("RemoveSoldier", fightingRPC[i + 1], fightingRPC[i + 2], fightingRPC[i + 3], fightingRPC[i + 4], 1);
            }
            else
            {
                //MogoWorld.thePlayer.RpcCall("RemoveBuilding", BelongTo.ID, UnitId, _Buildingkey, _attackFirst);
                MogoWorld.thePlayer.RpcCall("RemoveBuilding", fightingRPC[i + 1], fightingRPC[i + 2], fightingRPC[i + 3], fightingRPC[i + 4]);
            }
        }
    }

    protected override void SetGroupData()
    {
        if (StarColor != null && MyGroup != null)
        {
            StarColor.gameObject.SetActive(m_isExploxe);
            StarColor.spriteName = MyGroup.star_color;
        }
    }

    public void BuildingConsume(int _Energy)
    {
        SortedList<int, UnitSoldier> _list = BuildingPriorityList[MogoWorld.thePlayer.ID];// AttackPriority[MogoWorld.thePlayer.ID];

        ReduceSoldierByNumber(_list, _Energy);
    }

    /// <summary>
    /// 减少士兵 通过数量
    /// </summary>
    /// <param name="_list"></param>
    /// <param name="_Energy"></param>
    private void ReduceSoldierByNumber(SortedList<int, UnitSoldier> _list, int _Energy)
    {
        //LoggerHelper.Error("ReduceSoldierByNumber:"+_Energy);
        foreach (KeyValuePair<int, UnitSoldier> _keyvalue in _list)
        {
            if (_keyvalue.Value.CurEnergy < _Energy)
            {
                _Energy -= _keyvalue.Value.CurEnergy;
                MogoWorld.thePlayer.RpcCall("RemoveSoldier", MogoWorld.thePlayer.ID, UnitId, _keyvalue.Value.BaseSoldierData.id, _keyvalue.Value.CurEnergy, 0);
            }
            else
            {
                int _lefEnergy = ((_keyvalue.Value.CurEnergy - _Energy) % _keyvalue.Value.BaseSoldierData.energy);

                //LoggerHelper.Error("_lefEnergy:" + _lefEnergy + " _Energy:" + _Energy);

                MogoWorld.thePlayer.RpcCall("RemoveSoldier", _keyvalue.Value.BelongTo.ID, UnitId, _keyvalue.Value.BaseSoldierData.id, _lefEnergy + _Energy, 0);
                _Energy = 0;
                return;
            }
        }
    }

    /// <summary>
    /// 是否是升级建筑
    /// </summary>
    /// <param name="_buildingType"></param>
    /// <returns></returns>
    public int IsLevelUpBuilding(int _buildingType)
    {
        foreach (var _item in BuildingDic)
        {
            if (_item.Value.BaseBuildingData.level_up.Contains(_buildingType))
            {
                return _item.Key;
            }
        }
        return -1;
    }

    /// <summary>
    /// 是否在攻击星球
    /// </summary>
    /// <returns></returns>
    public bool IsInAttackStar()
    {
        //LoggerHelper.Error("AttackStar 1");
        if ((BelongTo != null) && (BelongTo.ID == MogoWorld.thePlayer.ID))
        {
            //LoggerHelper.Error("AttackStar 2");
            if (BuildingDic.Count <= 0)//没建筑才能攻击建筑
            {
                //LoggerHelper.Error("AttackStar 3");
                foreach (var _key in SoldierDic.Keys)
                {
                    //LoggerHelper.Error("AttackStar 4");
                    //存在不属于星球主人的士兵
                    if (_key != BelongTo.ID)
                    {
                        //LoggerHelper.Error("AttackStar 5");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void AttackStar()
    {
        //LoggerHelper.Error("AttackStar");
        SortedList<int, UnitSoldier> _soldierList = null;
        uint _attackPlayerId = 1;
        foreach (var _key in SoldierDic.Keys)
        {
            if (_key != BelongTo.ID)
            {
                _attackPlayerId = _key;
                _soldierList = AttackPriority[_key];
                break;
            }
        }
        if (_soldierList == null)
        {
            return;
        }

        int _Energy = 0;
        foreach (var _keyvalue in _soldierList.Values)
        {
            _Energy = (int)_keyvalue.GetAttackStarEnergy();

            MogoWorld.thePlayer.RpcCall("RemoveSoldier", _keyvalue.BelongTo.ID, UnitId, _keyvalue.BaseSoldierData.id, _Energy, 2);

            break;
        }
        //LoggerHelper.Error("_Energy:" + _Energy);
        //ReduceSoldierByNumber(_soldierList, _Energy);
        if (_Energy > 0)
        {
            MogoWorld.thePlayer.RpcCall("AttackStar", _attackPlayerId, BelongTo.ID, UnitId, _Energy);
        }
    }


    /// <summary>
    /// 星球能量
    /// </summary>
    /// <param name="_dtTime"></param>
    private void UpdateStarEnergy(float _dtTime)
    {
        if (CurEnergy < BaseData.energy)
        {
            StarEnergyTime += _dtTime;
            if (StarEnergyTime > MogoWorld.BuildingReversionFrequency)
            {
                //LoggerHelper.Error("UpdateStarEnergy: UnitId" + UnitId);
                StarEnergyTime -= MogoWorld.BuildingReversionFrequency;
                CurEnergy += MogoWorld.BuildingReversionEnergy;
                if ((MogoWorld.m_dataMapManager.CurStarID == UnitId) && (BelongTo.ID == MogoWorld.thePlayer.ID))
                {
                    //LoggerHelper.Error("UpdateStarEnergy:"+CurEnergy);
                    UIManager.I.GetUILogic<StarInfoUIMgr>().SetStarEnergy(CurEnergy);
                }
            }
        }
    }

    /// <summary>
    /// 记录星球建筑历史
    /// </summary>
    public void AddBuildingLog(int _buildingType)
    {
        BuildingLog.Add(_buildingType);
    }

    /// <summary>
    /// 攻占结束后恢复建筑
    /// </summary>
    public void RecoveryBuilding()
    {
        for (int i = 0; i < BaseData.count; i++)
        {
            MogoWorld.thePlayer.RpcCall("AddBuilding", BelongTo.ID, UnitId, BuildingLog[BuildingLog.Count - i - 1], 100);//key为建筑类型
        }
    }



    /// <summary>
    /// 显示攻击者 攻击士兵时的表现
    /// </summary>
    public void showAttackSoldierEffect(uint gethitPlayer, Vector3 _position)
    {
        ///士兵
        EntityParent _gethitEntity = MogoWorld.GetEntityById(gethitPlayer);
        EntityParent _attackEntity;
        Dictionary<int, UnitSoldier> _dicSoldier;
        foreach (var _key in SoldierDic.Keys)
        {
            _attackEntity = MogoWorld.GetEntityById(_key);

            if (_attackEntity.TeamId == _gethitEntity.TeamId)//同阵营
            {
                continue;
            }

            _dicSoldier = SoldierDic[_key];
            foreach (var _soldierItem in _dicSoldier.Values)
            {
                if (_soldierItem.BaseSoldierData.attack_object.Contains(2))//可以攻击士兵
                {
                    _soldierItem.OnAttack(_position);
                }
            }
        }

        ///建筑
        if ((BelongTo != null) && (BelongTo.TeamId != _gethitEntity.TeamId))
        {
            foreach (var _buildingItem in BuildingDic.Values)
            {
                if (_buildingItem.BaseBuildingData.attack > 0)
                {
                    _buildingItem.OnAttack(_position);
                }
            }
        }
    }

    /// <summary>
    /// 显示攻击者 攻击建筑时的表示
    /// </summary>
    public void showAttackBuildingEffect(Vector3 _position)
    {
        EntityParent _gethitEntity = BelongTo;

        EntityParent _attackEntity;
        Dictionary<int, UnitSoldier> _dicSoldier;
        foreach (var _key in SoldierDic.Keys)
        {
            _attackEntity = MogoWorld.GetEntityById(_key);

            if (_attackEntity.TeamId == _gethitEntity.TeamId)//同阵营
            {
                continue;
            }

            _dicSoldier = SoldierDic[_key];
            foreach (var _soldierItem in _dicSoldier.Values)
            {
                if (_soldierItem.BaseSoldierData.attack_object.Contains(1))//可以攻击建筑
                {
                    _soldierItem.OnAttack(_position);
                }
            }
        }
    }
}