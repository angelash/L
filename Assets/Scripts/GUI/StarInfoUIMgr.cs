using Mogo.Game;
using Mogo.GameData;
using Mogo.UI;
using Mogo.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarInfoUIMgr : UILogic
{
    private List<MogoUIBtn> BuildingTypeList = new List<MogoUIBtn>();

    private UILabel StarNameTxt;
    private UILabel EnergyTxt;
    private UISprite EnergySp;
    private MogoUIBtn GatherBtn;//集合
    private MogoUIBtn AttackBtn;//攻击
    private UILabel BuildingNumTxt;//建筑数量
    public AttackStatusType AttackStatus = 0;//0表示归属星球未攻击状态，1表示归属星球准备攻击状态，2表示归属星球选择集合点状态
    public UnitStar curStar;//当前战斗星球
    public UnitStar targetStar;//攻击目标
    private UnitStar clickStar;//当前点击星球
    private GameObject BuildingList;//建筑列表
    private GameObject EnergyGo;//星球能量体显示隐藏
    public bool IsOpenRangleCheck = true;//是否开启范围检测  临时关闭，用于测试
    private int curSoldierNum = 0;//记录当前星球我方士兵数量
    private int curSoldierEnergy = 0;//记录当前星球我方士兵能量

    public override UIProperties Properties
    {
        get
        {
            return UIProperties.DontShowLoadingTip;
        }
    }

    protected override string[] Resources
    {
        get
        {
            return new string[] { "ControllerPanel.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/Center");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0, -280, -10);
        m_myTransform.localScale = new Vector3(1.0f, 1.0f, 1f);

        StarNameTxt = FindTransform("StarName").GetComponent<UILabel>();
        EnergyTxt = FindTransform("LblEnergy").GetComponent<UILabel>();
        EnergySp = FindTransform("EnergyBar").GetComponent<UISprite>();
        EnergyGo = FindTransform("Energy").gameObject;
        BuildingNumTxt = FindTransform("BuildingNumTxt").GetComponent<UILabel>();

        SetBuildingInfo();

        GatherBtn = FindTransform("EufloriaBtnGather").GetComponent<MogoUIBtn>();
        AttackBtn = FindTransform("EufloriaBtnAttack").GetComponent<MogoUIBtn>();
        GatherBtn.ClickAction = OnEufloriaBtnGatherAction;
        AttackBtn.ClickAction = OnEufloriaBtnAttackAction;
        GatherBtn.SetText(LanguageData.GetContent(185));
        AttackBtn.SetText(LanguageData.GetContent(184));

        HideAllInfo();

        AddListeners();
    }

    private void HideAllInfo()
    {
        ShowAttackCancelBtn(false);
        GatherBtn.gameObject.SetActive(false);
        AttackBtn.gameObject.SetActive(false);
        EnergyGo.SetActive(false);
        BuildingNumTxt.gameObject.SetActive(false);
        ShowBuildings(false);
    }

    private void SetBuildingInfo()
    {
        BuildingList = FindTransform("BuildingList").gameObject;
        //显示建筑列表
        Transform buildingBg = FindTransform("ImgBg");
        MogoUIBtn buildingType = FindTransform("BuildingType").GetComponent<MogoUIBtn>();
        BuildingTypeList.Clear();
        int i = 0;
        foreach (BuildingData data in BuildingData.dataMap.Values)
        {
            GameObject copyBuildingsBg = GameObject.Instantiate(buildingBg.gameObject) as GameObject;
            GameObject copyBuildingType = GameObject.Instantiate(buildingType.gameObject) as GameObject;
            copyBuildingsBg.transform.parent = BuildingList.transform;
            copyBuildingType.transform.parent = BuildingList.transform;
            copyBuildingsBg.transform.localScale = new Vector3(80, 80, 80);
            copyBuildingsBg.SetActive(true);
            copyBuildingType.transform.localScale = new Vector3(1, 1, 1);
            copyBuildingType.SetActive(true);
            copyBuildingsBg.transform.localPosition = new Vector3(i * 90, 0, 0);
            copyBuildingType.transform.localPosition = copyBuildingsBg.transform.localPosition;
            MogoUIBtn btn = copyBuildingType.GetComponent<MogoUIBtn>();
            BuildingTypeList.Add(btn);

            BuildingTypeList[i].IDUint64 = (ulong)data.id;
            BuildingTypeList[i].SetText(LanguageData.GetContent(data.name));
            BuildingTypeList[i].m_imgNormal.spriteName = data.icon;
            BuildingTypeList[i].m_imgPressed.spriteName = data.icon;
            UIEventListener.Get(BuildingTypeList[i].gameObject).onPress = OnPressBuildingAction;
            i++;
        }
    }

    private void ShowAttackOkBtn(bool isShow)
    {
        EventDispatcher.TriggerEvent<bool>(Events.StarUIEvent.ShowAttackOkEvent, isShow);
    }

    private void ShowAttackCancelBtn(bool isShow)
    {
        EventDispatcher.TriggerEvent<bool>(Events.StarUIEvent.ShowAttackCancelEvent, isShow);
    }

    private void ShowReconSoldierBtn(bool isShow)
    {
        EventDispatcher.TriggerEvent<bool>(Events.StarUIEvent.ShowReconSoldierEvent, isShow);
    }

    private void OnEufloriaBtnAttackAction(MogoUIBtn btn)
    {
        StarInfoController.getInstance().AttackStar();
    }

    private void OnEufloriaBtnGatherAction(MogoUIBtn btn)
    {
        StarInfoController.getInstance().GatherStar();
    }

    private bool IsShowOwnerRangle = false;

    /// <summary>
    /// 展示自己已经攻占的范围
    /// </summary>
    /// <param name="isShow"></param>
    public void ShowOwnerRangle(bool isShow, bool check = false)
    {
        if (check == true && IsShowOwnerRangle == isShow) return;
        foreach (UnitStar star in MogoWorld.m_dataMapManager.GetStarList())
        {
            if (star.BelongTo != null && star.BelongTo.ID == MogoWorld.thePlayer.ID)
            {
                star.MovingRange.SetActive(isShow);
            }
        }
        IsShowOwnerRangle = isShow;
    }

    /// <summary>
    /// 根据条件展示建筑（每出生一个兵调用，待优化）
    /// </summary>
    /// <param name="isShow"></param>
    public void ShowBuildings(bool isShow)
    {
        if (BuildingList == null)
        {
            return;
        }
        BuildingList.SetActive(isShow);
        if (isShow == false) return;//根据我方兵力展示各种建筑
        int lastBuildingId = 0;
        UnitBuilding lastBuilding;
        foreach (MogoUIBtn btn in BuildingTypeList)
        {
            if (curSoldierEnergy >= BuildingData.dataMap[(int)btn.IDUint64].building_consume)
            {
                lastBuildingId = GetLastBuildingId((int)btn.IDUint64);
                if (lastBuildingId != 0)//有上级建筑，判断当前星球上级建筑是否存在
                {
                    lastBuilding = curStar.GetBuildingById(lastBuildingId);
                    if (lastBuilding != null && lastBuilding.IsBuilding == false)//有上级建筑且已经建成
                        btn.IsEnabled = true;
                    else
                        btn.IsEnabled = false;
                }
                else
                {
                    if (curStar.GetBuildingNum() < curStar.BaseData.count)//建筑数量未达到上限
                        btn.IsEnabled = true;
                    else
                        btn.IsEnabled = false;
                }
            }
            else
                btn.IsEnabled = false;
        }
    }

    /// <summary>
    /// 获得上一级建筑id
    /// </summary>
    /// <returns></returns>
    private int GetLastBuildingId(int buildingId)
    {
        foreach (BuildingData data in BuildingData.dataMap.Values)
        {
            if (data.level_up.IndexOf(buildingId) != -1)
            {
                return data.id;
            }
        }
        return 0;
    }

    protected override void OnRelease()
    {
        RemoveListeners();
        base.OnRelease();
    }

    private void AddListeners()
    {
        EventDispatcher.AddEventListener<int>(Events.StarUIEvent.ClickStarEvent, ShowStarInfoById);
        EventDispatcher.AddEventListener(Events.StarUIEvent.AddSoldierEvent, OnAddSoldier);
        EventDispatcher.AddEventListener(Events.StarUIEvent.AddBuildingEvent, OnAddBuilding);
    }

    private void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener<int>(Events.StarUIEvent.ClickStarEvent, ShowStarInfoById);
        EventDispatcher.RemoveEventListener(Events.StarUIEvent.AddSoldierEvent, OnAddSoldier);
        EventDispatcher.RemoveEventListener(Events.StarUIEvent.AddBuildingEvent, OnAddBuilding);
    }

    public void ShowStarInfoById(int _id)
    {
        if (AttackStatus == AttackStatusType.Gather && curStar != null && curStar.TogetherId != -1)//如果处于集合状态，且已经有集合点，只能执行删除集合点操作
            return;

        clickStar = MogoWorld.m_dataMapManager.GetUnitStarById(_id);
        if (clickStar.BelongTo != null)
        {
            if (clickStar.BelongTo.ID == MogoWorld.thePlayer.ID)//是自己的星球
                SetOwnerStar();
            else//敌方的星球
                SetOtherStar();
        }
        else//未开发的星球
        {
            SetNonStar();
        }
    }

    private void SetNonStar()
    {
        SetOtherAndNonStar();
    }

    private void SetOtherAndNonStar()
    {
        if (AttackStatus == AttackStatusType.DisPatch || AttackStatus == AttackStatusType.Gather)//准备攻击，点了下一个星球
        {
            if (curStar.CheckStarReachByRangle(clickStar) == false && IsOpenRangleCheck)//暂时去掉，给战斗测试
            {
                ShowAttackOkBtn(false);
                ShowReconSoldierBtn(false);
            }
            else
            {
                ShowAttackOkBtn(true);
                ShowReconSoldierBtn(true);
            }

            SetAttackAndgatherStatus();
        }
        else//展示未开发星球信息
        {
            curStar = clickStar;
            MogoWorld.m_dataMapManager.CurStarID = clickStar.UnitId;
            StarNameTxt.gameObject.SetActive(true);
            StarNameTxt.text = LanguageData.GetContent(168);
            HideAllInfo();
            curSoldierEnergy = curStar.GetSoldierEnergy();
            if (curSoldierEnergy > 0)
                ShowBuildings(true);
        }
    }

    private void SetAttackAndgatherStatus()
    {
        clickStar.MovingRange.SetActive(false);
        ShowOwnerRangle(true);
        targetStar = clickStar;
        curStar.HideAllLine();//先全部隐藏
        curStar.SetCurrentPath(MapUtil.Plan(curStar, targetStar).PassedStars);
        curStar.DrawAllPathLine();
        UIManager.I.GetUILogic<MapUIMgr>().MyCameraController.MoveTo(new Vector3(-targetStar.PositionX, -targetStar.PositionY, 0));
        //TweenPosition.Begin(cam.gameObject, 0.2f, new Vector3(-targetStar.PositionX, -targetStar.PositionY, 0));
    }

    private void SetOtherStar()
    {
        SetOtherAndNonStar();
    }

    private void SetOwnerStar()
    {
        if (AttackStatus == 0)//未攻击状态
        {
            curStar = clickStar;
            SetCommonInfo();
            curSoldierNum = curStar.GetSoldierNum();
            if (curStar.TogetherId != -1)//处于集合状态
            {
                if (curSoldierNum > 10) AttackBtn.gameObject.SetActive(true);
            }
            else
            {
                if (curSoldierNum > 0) AttackBtn.gameObject.SetActive(true);
            }
            if (curStar.GetSoliderBuildingNum() > 0)
                GatherBtn.gameObject.SetActive(true);
        }
        else if (AttackStatus == AttackStatusType.DisPatch || AttackStatus == AttackStatusType.Gather)//准备攻击，点了下一个星球或选择集合点状态
        {
            if (clickStar.UnitId == curStar.UnitId)//点击的是当前战斗星球
                return;
            ShowAttackOkBtn(true);
            ShowReconSoldierBtn(true);
            SetAttackAndgatherStatus();
        }
        int buildingNum = curStar.GetBuildingNum();
        if (buildingNum > 0)
        {
            SetStarEnergy(curStar.CurEnergy);
        }
        SetBuildingNumTxt();
    }

    private void SetCommonInfo()
    {
        HideAllInfo();
        MogoWorld.m_dataMapManager.CurStarID = clickStar.UnitId;
        StarNameTxt.text = clickStar.UnitId.ToString();
        SetStarEnergy(clickStar.CurEnergy);
        curSoldierEnergy = curStar.GetSoldierEnergy();
        if (curSoldierEnergy > 0)
            ShowBuildings(true);
    }

    public void SetStarEnergy(int energy)
    {
        if (curStar == null) return;
        if (energy <= 0) energy = 0;
        if (EnergyTxt == null)
        {
            return;
        }
        if (StarNameTxt.gameObject.activeSelf) StarNameTxt.gameObject.SetActive(false);
        EnergyGo.SetActive(true);
        EnergyTxt.text = string.Concat(energy, "/", curStar.BaseData.energy);//用concat创建对象比用+号少
        EnergySp.transform.localScale = new Vector3(((float)energy / (float)curStar.BaseData.energy) * 200f, 15, 0);
    }

    private void OnAddSoldier()
    {
        if (AttackStatus == 0 && curStar != null && bShow)
        {
            curSoldierNum = curStar.GetSoldierNum();
            curSoldierEnergy = curStar.GetSoldierEnergy();
            if (curStar.TogetherId != -1)//处于集合状态
            {
                if (curSoldierNum > 10) AttackBtn.gameObject.SetActive(true);
            }
            else
            {
                if (curSoldierNum > 0) AttackBtn.gameObject.SetActive(true);
            }
            if (GatherBtn.gameObject.activeSelf == false) GatherBtn.gameObject.SetActive(true);
            if (curSoldierEnergy > 0)
            {
                ShowBuildings(true);//这里可能涉及到性能的问题
            }
        }
    }

    /// <summary>
    /// 自己星球的建筑添加
    /// </summary>
    private void OnAddBuilding()
    {
        if (curStar == null) return;
        // 自个的建筑添加，星球能量值加满
        SetStarEnergy(curStar.CurEnergy);
        StarNameTxt.text = curStar.UnitId.ToString();
        SetBuildingNumTxt();
        ShowBuildings(true);//重新检测建筑列表展示
    }

    private void SetBuildingNumTxt()
    {
        BuildingNumTxt.gameObject.SetActive(true);
        int curNum = curStar.GetBuildingNum();
        if (curNum >= curStar.BaseData.count)
            BuildingNumTxt.color = Color.red;
        else
            BuildingNumTxt.color = Color.green;
        BuildingNumTxt.text = LanguageData.GetContent(167, curNum, curStar.BaseData.count);
    }

    private void OnBuildingAction(GameObject go)
    {
        SoundManager.PlaySound("growing_sound_norm_low.ogg");
        SendBuildingMsg((int)go.GetComponent<MogoUIBtn>().IDUint64);
    }

    private float beginPressTime = 0;

    private void OnPressBuildingAction(GameObject go, bool state)
    {
        //Debug.Log(state);
        if (state == true)
        {
            beginPressTime = Time.time;
            if (UIManager.I.GetUILogic<TipsUIMgr>().bShow == true) return;
            float x = -320 + go.transform.localPosition.x + m_myTransform.localPosition.x;
            float y = 120 + go.transform.localPosition.y + m_myTransform.localPosition.y;
            object[] tipsContent = StarInfoController.getInstance().GetBuildingTipsById((int)go.GetComponent<MogoUIBtn>().IDUint64, x, y);
            UIManager.I.GetUILogic<TipsUIMgr>().Show(tipsContent);
        }
        else
        {
            UIManager.I.CloseUI<TipsUIMgr>();
            if (Time.time - beginPressTime <= 0.5f)
            {
                if (go.GetComponent<MogoUIBtn>().IsEnabled)
                    OnBuildingAction(go);
            }
        }
    }

    private void SendBuildingMsg(int _type)
    {
        //LoggerHelper.Error("SendBuildingMsg _type" + _type);
        MogoWorld.thePlayer.RpcCall("AddBuilding", MogoWorld.thePlayer.ID, MogoWorld.m_dataMapManager.CurStarID, _type, 0);
        MogoWorld.m_dataMapManager.BuildingConsume(_type);
    }

    public void HideUI()
    {
        Close();
        ShowOwnerRangle(false);
        if (curStar != null)
        {
            curStar.MovingRange.SetActive(false);
            curStar.SelectSign.SetActive(false);
            curStar.HideAllLine();
            curStar = null;
        }
    }
}