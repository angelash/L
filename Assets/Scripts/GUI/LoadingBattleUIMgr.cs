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
using UnityEngine;

public class LoadingBattleUIMgr : UILogic
{
    public override UIProperties Properties
    {
        get
        {
            return UIProperties.DontShowLoadingTip;
        }
    }

    private UILabel CurTimeTxt;//已加载时间
    private UILabel PreTimeTxt;//标题
    private UISprite LoadingBg;
    private uint timeId;

    protected override string[] Resources
    {
        get
        {
            return new string[] { "LoadingBattleUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoTopUIPanel.FindChild("TopUI/Center"); ;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        CurTimeTxt = FindTransform("CurTimeTxt").GetComponent<UILabel>();
        PreTimeTxt = FindTransform("PreTimeTxt").GetComponent<UILabel>();
        LoadingBg = FindTransform("LoadingBg").GetComponent<UISprite>();

        CurTimeTxt.text = LanguageData.GetContent(189);

        timeId = TimerHeap.AddTimer(100, 100, ShowLoading);
        TimerHeap.Tick();

        MogoUIBtn UIBtn = FindTransform("BtnCancel").GetComponent<MogoUIBtn>();
        UIBtn.Text = LanguageData.GetContent(4);
        UIBtn.ClickAction = OnClickCancel;
    }

    private float s = 0;

    private void ShowLoading()
    {
        s += 0.01f;
        LoadingBg.fillAmount = s;
        if (s >= 1f) s = 0;
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        TimerHeap.DelTimer(timeId);
    }

    private void OnClickCancel(MogoUIBtn btn)
    {
        UIManager.I.CloseUI<LoadingBattleUIMgr>();
        UIManager.I.ShowUI<GameStartUIMgr>();
        MogoWorld.DisConnectServer();
    }
}