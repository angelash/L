/*********************************************************
/*********************************************************
 * Copyright (C) 2013 广州，爱游
 *
 * 模块名：MogoUIListener
 * 创建者：李建辉
 * 修改者列表：
 * 创建日期：2016/4/22 14:32:05
 * 模块描述：
 * 用法实例：
 *
 * *******************************************************/

using Mogo.GameData;
using Mogo.UI;
using Mogo.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierFightUIMgr : UILogic
{
    public override UIProperties Properties
    {
        get
        {
            return UIProperties.DontShowLoadingTip;
        }
    }

    private GameObject SoldierList;
    private List<MogoUIBtn> SoldierTypeList;

    protected override string[] Resources
    {
        get
        {
            return new string[] { "SoldierFightPanel.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/Center"); ;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0, -312, -10);
        m_myTransform.localScale = new Vector3(1.0f, 1.0f, 1f);

        //显示建筑列表
        SoldierTypeList = new List<MogoUIBtn>();
        Transform soldierBg = FindTransform("ImgBg");
        SoldierList = FindTransform("SoldierList").gameObject;
        MogoUIBtn soldierType = FindTransform("SoldierType").GetComponent<MogoUIBtn>();
        int i = 0;
        foreach (SoldierData data in SoldierData.dataMap.Values)
        {
            GameObject copyBuildingsBg = GameObject.Instantiate(soldierBg.gameObject) as GameObject;
            GameObject copyBuildingType = GameObject.Instantiate(soldierType.gameObject) as GameObject;
            copyBuildingsBg.transform.parent = SoldierList.transform;
            copyBuildingType.transform.parent = SoldierList.transform;
            copyBuildingsBg.transform.localScale = new Vector3(80, 80, 80);
            copyBuildingsBg.SetActive(true);
            copyBuildingType.transform.localScale = new Vector3(1, 1, 1);
            copyBuildingType.SetActive(true);
            copyBuildingsBg.transform.localPosition = new Vector3(i * 85, 0, 0);
            copyBuildingType.transform.localPosition = new Vector3(i * 85, 0, 0);
            MogoUIBtn btn = copyBuildingType.GetComponent<MogoUIBtn>();
            btn.IDUint64 = (ulong)data.id;
            SoldierTypeList.Add(btn);
            SoldierTypeList[i].SetText(LanguageData.GetContent(data.name));
            var copyBuildingTypeButton = copyBuildingType.GetComponent<MogoUIBtn>();
            copyBuildingTypeButton.m_imgNormal.spriteName = data.icon;
            copyBuildingTypeButton.m_imgPressed.spriteName = data.icon;
            btn.ClickAction = OnSelectSoldierAction;
            i++;
        }

        SetStatus();
        AddListeners();
    }

    private void AddListeners()
    {
        EventDispatcher.AddEventListener(Events.StarUIEvent.AddSoldierEvent, OnAddSoldier);
    }

    private void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener(Events.StarUIEvent.AddSoldierEvent, OnAddSoldier);
    }

    private void OnAddSoldier()
    {
        if (bShow == false) return;
        UnitStar curStar = MogoWorld.m_dataMapManager.GetUnitStarById(MogoWorld.m_dataMapManager.CurStarID);
        UnitSoldier soldier;
        MogoUIBtn btn;
        UILabel numTxt;
        foreach (SoldierData data in SoldierData.dataMap.Values)
        {
            soldier = curStar.GetSoldierById(MogoWorld.thePlayer.ID, data.id);
            if (soldier != null)
            {
                int num = soldier.GetSoldierNum();
                if (num > 0)
                {
                    btn = GetSoldierBtn(data.id);
                    numTxt = btn.transform.FindChild("NumText").gameObject.GetComponent<UILabel>();
                    numTxt.text = ((int)(num * (1 - StarInfoController.getInstance().soldierNumPercent))).ToString();
                }
            }
        }
    }

    protected override void OnRelease()
    {
        RemoveListeners();
        base.OnRelease();
    }

    private void OnSelectSoldierAction(MogoUIBtn btn)
    {
        GameObject selectSign = btn.gameObject.transform.FindChild("SelectSign").gameObject;
        selectSign.SetActive(!selectSign.activeSelf);
        if (selectSign.activeSelf == true)
            StarInfoController.getInstance().FightSoldierIdList.Add((int)btn.IDUint64);
        else
            StarInfoController.getInstance().FightSoldierIdList.Remove((int)btn.IDUint64);
    }

    public void SetStatus()
    {
        UnitStar star = MogoWorld.m_dataMapManager.GetUnitStarById(MogoWorld.m_dataMapManager.CurStarID);
        UnitSoldier soldier;
        UILabel numTxt;
        MogoUIBtn soldierBtn;

        foreach (SoldierData data in SoldierData.dataMap.Values)
        {
            soldier = star.GetSoldierById(MogoWorld.thePlayer.ID, data.id);
            soldierBtn = GetSoldierBtn(data.id);
            numTxt = soldierBtn.transform.FindChild("NumText").gameObject.GetComponent<UILabel>();
            if (soldier != null)//当前有这种士兵
            {
                soldierBtn.IsEnabled = true;
                SetSelectSign(soldierBtn, true);
                StarInfoController.getInstance().FightSoldierIdList.Add(data.id);
                numTxt.text = ((int)(soldier.GetSoldierNum() * (1 - StarInfoController.getInstance().soldierNumPercent))).ToString();
            }
            else
            {
                soldierBtn.IsEnabled = false;
                SetSelectSign(soldierBtn, false);
                numTxt.text = "";
            }
        }
    }



    public void SetAggregateSoldier()
    {
        UnitStar star = MogoWorld.m_dataMapManager.GetUnitStarById(MogoWorld.m_dataMapManager.CurStarID);
        UnitSoldier soldier;
        StarInfoController.getInstance().FightSoldierIdList.Clear();
        foreach (SoldierData data in SoldierData.dataMap.Values)
        {
            soldier = star.GetSoldierById(MogoWorld.thePlayer.ID, data.id);
            if (soldier != null)//当前有这种士兵
            {
                StarInfoController.getInstance().FightSoldierIdList.Add(data.id);
            }
        }
    }


    private void SetSelectSign(MogoUIBtn btn, bool isShow)
    {
        GameObject selectSign = btn.gameObject.transform.FindChild("SelectSign").gameObject;
        selectSign.SetActive(isShow);
    }

    private MogoUIBtn GetSoldierBtn(int id)
    {
        for (int i = 0; i < SoldierTypeList.Count; i++)
        {
            if ((int)SoldierTypeList[i].IDUint64 == id)
                return SoldierTypeList[i];
        }
        return null;
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        base.OnShow(param, callback);
        StarInfoController.getInstance().FightSoldierIdList.Clear();
        SetStatus();
    }
}