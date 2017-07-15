using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.Util;
using Mogo.GameData;

public class BattleScoreUIMgr : UILogic {

    private UILabel m_lblScoreRed;
    private UILabel m_lblScoreBlue;
    private UILabel m_lblTime;
    private UISprite m_imgTime;
    private UISprite m_imgMaskScoreRed;
    private UISprite m_imgMaskScoreBlue;
    private TweenScale m_tScaleScoreRed;
    private TweenScale m_tScaleScoreBlue;

    private uint m_timerIdTimeCount;
    private int m_curTimeLeft;
    private int m_totalTime;

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
            return new string[] { "BattleScoreUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        //var parent = MogoWorld.m_uiManager.BattleUI.FindChild("Top");
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/Center");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_lblScoreRed = FindComponent<UILabel>("LblScoreRed");
        m_lblScoreBlue = FindComponent<UILabel>("LblScoreBlue");
        m_lblTime = FindComponent<UILabel>("LblTime");
        m_imgTime = FindComponent<UISprite>("ImgTime");
        m_imgMaskScoreRed = FindComponent<UISprite>("MaskScoreRed");
        m_imgMaskScoreBlue = FindComponent<UISprite>("MaskScoreBlue");
        m_imgMaskScoreRed.fillAmount = 0f;
        m_imgMaskScoreBlue.fillAmount = 0f;
        m_tScaleScoreRed = FindComponent<TweenScale>("LblScoreRed");
        m_tScaleScoreRed.enabled = false;
        m_tScaleScoreBlue = FindComponent<TweenScale>("LblScoreBlue");
        m_tScaleScoreBlue.enabled = false;
        m_totalTime = GlobalData.dataMap[0].BattleTime;
        SetScoreRed(0);
        SetScoreBlue(0);
        SetTimeLeft(0);

        var btnGuide = FindComponent<MogoUIBtn>("BtnWenHao");
        btnGuide.ClickAction = OnBtnGuide;

        AddListeners();
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        if (m_timerIdTimeCount == 0)
        {
            m_curTimeLeft = m_totalTime;
            m_timerIdTimeCount = TimerHeap.AddTimer(0, 1000, delegate()
            {
                if (m_curTimeLeft < 0)
                {
                    MogoWorld.thePlayer.RpcCall("GameOver", 1);
                    DelTimerTimeCount();
                    return;
                }
                SetTimeLeft(m_curTimeLeft);
                m_curTimeLeft--;
                MogoWorld.m_dataMapManager.CulPlayerScroe();
            });
        }
    }

    protected override void OnRelease()
    {
        DelTimerTimeCount();
        RemoveListeners();
        base.OnRelease();
    }

    private void AddListeners()
    {
        EventDispatcher.AddEventListener(GameConst.Event.TeamOneScoreChanged, OnTeamOneScoreChanged);
        EventDispatcher.AddEventListener(GameConst.Event.TeamOneScoreChanged, OnTeamTwoScoreChanged);
    }

    private void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener(GameConst.Event.TeamOneScoreChanged, OnTeamOneScoreChanged);
        EventDispatcher.RemoveEventListener(GameConst.Event.TeamOneScoreChanged, OnTeamTwoScoreChanged);
    }

    private void OnTeamOneScoreChanged()
    {
    }

    private void OnTeamTwoScoreChanged()
    {
    }

    public void SetScoreRed(int value)
    {
        m_lblScoreRed.text = value.ToString();
    }

    public void SetScoreBlue(int value)
    {
        m_lblScoreBlue.text = value.ToString();
    }

    private void UpdateScorePercent()
    {
    }

    private void SetTimeLeft(int leftSeconds)
    {
        int minute = leftSeconds / 60;
        int second = leftSeconds % 60;
        string strTime = string.Format("{0:00}:{1:00}", minute, second);
        m_lblTime.text = strTime;

        m_imgTime.fillAmount = (float)leftSeconds / m_totalTime;
    }

    private void DelTimerTimeCount()
    {
        TimerHeap.DelTimer(m_timerIdTimeCount);
        m_timerIdTimeCount = 0;
    }

    private void OnBtnGuide(MogoUIBtn _btn)
    {
        UIManager.I.ShowUI<NoviceGuideUIMgr>();
    }
}
