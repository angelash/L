using Mogo.Game;
using Mogo.GameData;
using Mogo.UI;
using Mogo.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceUIMgr : UILogic
{
    private List<List<UILabel>> ScoreList;

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
            return new string[] { "BalanceUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/Center");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0, 0, 0);
        m_myTransform.localScale = new Vector3(1.0f, 1.0f, 1f);

        MogoUIBtn _btn = FindTransform("BtnOk").GetComponent<MogoUIBtn>();
        _btn.SetText(LanguageData.GetContent(3));
        _btn.ClickAction = OnCloseUI;
        //LoggerHelper.Error(LanguageData.GetContent(3));

        ScoreList = new List<List<UILabel>>();
        for (int i = 0; i < 2; i++)
        {
            ScoreList.Add(new List<UILabel>());
            for (int j = 0; j < 5; j++)
            {
                ScoreList[i].Add(FindTransform(string.Concat("Score", i, "Text", j)).GetComponent<UILabel>());
                FindTransform(string.Concat("ScoreName", i, "Text", j)).GetComponent<UILabel>().text = LanguageData.GetContent(173 + j);
            }
        }

        ShowScore();
    }

    private void OnCloseUI(MogoUIBtn btn)
    {
        MogoWorld.CloseUI();
        //MogoWorld.StartGame();
        //MogoWorld.m_sceneManager.LoadLoginScene();
        //MogoWorld.m_uiManager.LoadMainUI();
        //MogoWorld.DisConnectServer();
        MogoWorld.OnMainUILoaded();
        MogoWorld.m_sceneManager.LoadLoginScene();
        MogoWorld.Quit();
    }

    private void ShowScore()
    {
        SetLabelText("Score0Text4", MogoWorld.thePlayer.Name);
        MogoWorld.m_dataMapManager.GetScore(MogoWorld.thePlayer.ID, ScoreList[0]);
        foreach (var _item in MogoWorld.Entities)
        {
            SetLabelText("Score1Text4", _item.Value.Name);
            MogoWorld.m_dataMapManager.GetScore(_item.Value.ID, ScoreList[1]);
        }

        if (MogoWorld.m_dataMapManager.IsWin)
        {
            SetLabelText("TitleText", 178);
        }
        else
        {
            SetLabelText("TitleText", 179);
        }
    }
}