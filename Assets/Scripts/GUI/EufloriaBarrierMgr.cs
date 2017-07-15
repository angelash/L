using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.Util;
using Mogo.GameData;

public class EufloriaBarrierMgr : UILogic
{


    private MogoUIBtn m_btnStart;


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
            return new string[] { "EufloriaBarrierUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/BottomRight");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(-70f, 22f, 0);
        m_myTransform.localScale = new Vector3(1.0f, 1.0f, 1f);


        var btnXuQi = FindComponent<MogoUIBtn>("EufloriaBarrierUIStartBtn");
        btnXuQi.SetText(LanguageData.GetContent(155));

        m_btnStart = FindComponent<MogoUIBtn>("EufloriaBarrierUIStartBtn");
        m_btnStart.ClickAction = OnBtnStart;

        AddListeners();
    }


    private void AddListeners()
    {

    }

    private void OnBtnStart(MogoUIBtn btn)
    {
        LoggerHelper.Debug("OnBtnStart");
        //MogoWorld.m_uiManager.LoadEufloriaBarrierUI();
        //UIManager.I.ShowUI<MapMgr>();
        //UIManager.I.ShowUI<MapControllerMgr>();
    }
}
