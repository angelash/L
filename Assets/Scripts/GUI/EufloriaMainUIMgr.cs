using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.Util;
using Mogo.GameData;

public class EufloriaMainUIMgr : UILogic
{


    private MogoUIBtn m_btnJoin;

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
            return new string[] { "EufloriaMainUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        /*var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/BottomRight");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(-70f, 22f, 0);
        m_myTransform.localScale = new Vector3(1.0f, 1.0f, 1f);
        */

        var parent = MogoWorld.m_uiManager.MogoMainUIPanel;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0f, 0f, 0f);

        m_btnJoin = FindComponent<MogoUIBtn>("EufloriaBtnJoin");
        m_btnJoin.SetText(LanguageData.GetContent(11));
        m_btnJoin.ClickAction = OnBtnJoin;

        AddListeners();
    }


    private void AddListeners()
    {

    }

    private void OnBtnJoin(MogoUIBtn btn)
    {
        LoggerHelper.Debug("OnBtnJoin");
        UIManager.I.CloseUI<EufloriaMainUIMgr>();
        //UIManager.I.ShowUI<MapUIMgr>();
        //UIManager.I.ShowUI<StarInfoUIMgr>();

        //MogoWorld.m_sceneManager.LoadMapScene(); 
    }
}