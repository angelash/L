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

using Mogo.UI;
using System.Collections;
using UnityEngine;

public class TipsUIMgr : UILogic
{
    public override UIProperties Properties
    {
        get
        {
            return UIProperties.DontShowLoadingTip;
        }
    }

    private UILabel ContentTxt;//展示文本
    private UILabel TitleTxt;//标题

    protected override string[] Resources
    {
        get
        {
            return new string[] { "TipsUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoTopUIPanel.FindChild("TopUI/Center"); ;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        ContentTxt = FindTransform("ContentTxt").GetComponent<UILabel>();
        TitleTxt = FindTransform("TitleTxt").GetComponent<UILabel>();
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        base.OnShow(param, callback);
        TitleTxt.text = (string)param[0];
        ContentTxt.text = (string)param[1];
        m_myTransform.localPosition = (Vector3)param[2];
    }
}