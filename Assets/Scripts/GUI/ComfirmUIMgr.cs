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
using UnityEngine;

public class ComfirmUIMgr : UILogic
{
    public override UIProperties Properties
    {
        get
        {
            return UIProperties.DontShowLoadingTip;
        }
    }

    private MogoUIBtn ComfirmBtn;//确定
    private MogoUIBtn CancelBtn;//取消
    private MogoUIBtn ReconSoldierBtn;//侦察兵按钮
    private MogoUIBtn DeleteBtn;//取消集合
    private MogoUIBtn PercentBtn;//显示百分比

    protected override string[] Resources
    {
        get
        {
            return new string[] { "ComfirmUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_dataMapManager.GetUnitStarById(MogoWorld.thePlayer.HeadQuarterId).UnitParent.parent;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        ComfirmBtn = FindTransform("EufloriaBtnOK").GetComponent<MogoUIBtn>();
        CancelBtn = FindTransform("EufloriaBtnCancel").GetComponent<MogoUIBtn>();
        ReconSoldierBtn = FindTransform("ReconSoldierBtn").GetComponent<MogoUIBtn>();
        DeleteBtn = FindTransform("EufloriaBtnDelete").GetComponent<MogoUIBtn>();
        PercentBtn = FindTransform("PercentBtn").GetComponent<MogoUIBtn>();

        ComfirmBtn.ClickAction = OnComfirmBtnClickHandler;
        CancelBtn.ClickAction = OnCancelBtnClickHandler;
        ReconSoldierBtn.ClickAction = OnReconSoldierBtnClickHandler;
        DeleteBtn.ClickAction = OnDeleteBtnClickHandler;

        ComfirmBtn.gameObject.SetActive(false);
        CancelBtn.gameObject.SetActive(false);
        ReconSoldierBtn.gameObject.SetActive(false);
        DeleteBtn.gameObject.SetActive(false);

        ComfirmBtn.SetText(LanguageData.GetContent(186));
        CancelBtn.SetText(LanguageData.GetContent(187));
        DeleteBtn.SetText(LanguageData.GetContent(188));

        var opt = OptDragListener.Get(PercentBtn.gameObject);
        opt.onDrag = UpdateArrow;

        ReconSoldierBtn.SetText(LanguageData.GetContent(182));
        ShowPercent(true);

        AddListeners();
    }

    private void UpdateArrow(GameObject go, Vector3 delta, Vector3 currentPos)
    {
        var pos = go.transform.position;
        var targetPos = currentPos - pos;

        //float r = Mathf.Sqrt(Mathf.Pow(targetPos.x, 2) + Mathf.Pow(targetPos.y, 2));
        //Debug.Log("r:" + r);
        //if (r < 10) return;//限定拖拉范围
        //开始计算
        StarInfoController.getInstance().SetSoldierNumPercent(targetPos);
    }

    public void ShowAttackOkBtn(bool isShow)
    {
        if (ComfirmBtn == null) return;
        ComfirmBtn.gameObject.SetActive(isShow);
    }

    public void ShowAttackCancelBtn(bool isShow)
    {
        if (CancelBtn == null) return;
        CancelBtn.gameObject.SetActive(isShow);
        if (isShow == false)//取消操作，隐藏所有
        {
            ShowAttackOkBtn(false);
            ShowReconSoldierBtn(false);
            ShowDeleteBtn(false);
            ShowPercent(false);
        }
    }

    public void ShowDeleteBtn(bool isShow)
    {
        if (DeleteBtn == null) return;
        DeleteBtn.gameObject.SetActive(isShow);
    }

    public void ShowReconSoldierBtn(bool isShow)
    {
        if (ReconSoldierBtn == null) return;
        ReconSoldierBtn.gameObject.SetActive(isShow);
    }

    private void AddListeners()
    {
        EventDispatcher.AddEventListener<bool>(Events.StarUIEvent.ShowAttackOkEvent, ShowAttackOkBtn);
        EventDispatcher.AddEventListener<bool>(Events.StarUIEvent.ShowAttackCancelEvent, ShowAttackCancelBtn);
        EventDispatcher.AddEventListener<bool>(Events.StarUIEvent.ShowReconSoldierEvent, ShowReconSoldierBtn);
    }

    private void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener<bool>(Events.StarUIEvent.ShowAttackOkEvent, ShowAttackOkBtn);
        EventDispatcher.RemoveEventListener<bool>(Events.StarUIEvent.ShowAttackCancelEvent, ShowAttackCancelBtn);
        EventDispatcher.RemoveEventListener<bool>(Events.StarUIEvent.ShowReconSoldierEvent, ShowReconSoldierBtn);
    }

    private void OnComfirmBtnClickHandler(MogoUIBtn btn)
    {
        SoundManager.PlaySound("send.ogg");
        StarInfoController.getInstance().AttackOrGatherToStar();
    }

    private void OnCancelBtnClickHandler(MogoUIBtn btn)
    {
        StarInfoController.getInstance().ReturnNormalStatus();
    }

    private void OnDeleteBtnClickHandler(MogoUIBtn btn)
    {
        StarInfoController.getInstance().DeleteGatherStar();
    }

    private void OnReconSoldierBtnClickHandler(MogoUIBtn btn)
    {
        SoundManager.PlaySound("send.ogg");
        StarInfoController.getInstance().SendReconSoldier();
    }

    public void SetPosition(Vector3 targetPosition)
    {
        m_myTransform.localPosition = targetPosition;
    }

    protected override void OnRelease()
    {
        RemoveListeners();
        base.OnRelease();
    }

    public void ShowAll(bool isShow)
    {
        if (isShow)
        {
            ShowAttackCancelBtn(isShow);
            ShowAttackOkBtn(isShow);
            ShowReconSoldierBtn(isShow);
            StarInfoController.getInstance().soldierNumPercent = 0;
            ShowPercent(isShow);
        }
        else
            ShowAttackCancelBtn(isShow);
    }

    public void ShowPercent(bool isShow)
    {
        if (PercentBtn.gameObject.activeSelf != isShow)
            PercentBtn.gameObject.SetActive(isShow);
        if (!isShow) return;
        float soldierNumPercent = StarInfoController.getInstance().soldierNumPercent;
        PercentBtn.m_imgNormal.fillAmount = 1 - soldierNumPercent;
        PercentBtn.m_imgPressed.fillAmount = 1 - soldierNumPercent;
    }
}