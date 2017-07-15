using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.Util;
using Mogo.GameData;

public class NoviceGuideUIMgr : UILogic
{
    private MogoUIBtn m_btnClose;
    private bool m_isShowGameStart = false;

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
            return new string[] { "NoviceGuideUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {

        var parent = MogoWorld.m_uiManager.MogoMainUIPanel;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0f, 0f, 0f);

        m_btnClose = FindComponent<MogoUIBtn>("CloseBtn");
        m_btnClose.ClickAction = OnBtnClose;

        AddListeners();
    }


    private void AddListeners()
    {
        
    }

    private void OnBtnClose(MogoUIBtn btn)
    {
        UIManager.I.CloseUI<NoviceGuideUIMgr>();
        if (m_isShowGameStart)
        {
            UIManager.I.ShowUI<GameStartUIMgr>();
        }
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        if (param != null && (param.Length > 0))
        {
            m_isShowGameStart = (bool)param[0];
        }
        else
        {
            m_isShowGameStart = false;
        }

        PlayerPrefs.SetInt("NoviceGuide",1);
    }
}