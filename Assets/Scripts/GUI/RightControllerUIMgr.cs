using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.Util;
using Mogo.GameData;

public class RightControllerUIMgr : UILogic
{
    private MogoUIBtn m_btnFly;
    private MogoUIBtn m_btnJump;
    private GameObject m_goKeyCodeXuqi;
    private GameObject m_goKeyCodeFlyState;
    private GameObject m_goKeyCodeJump;

    private void SetFlying(bool value)
    {
        if (value)
        {
            m_btnFly.SetText(LanguageData.GetContent(101));
            MogoWorld.m_uiManager.ShowFlyControllerUI();
            m_btnJump.gameObject.SetActive(false);
        }
        else
        {
            m_btnFly.SetText(LanguageData.GetContent(102));
            MogoWorld.m_uiManager.CloseFlyControllerUI();
            m_btnJump.gameObject.SetActive(true);
        }
    }

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
            return new string[] { "RightControllerUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/Right");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(-70, 100, 0);

        var btnXuQi = FindComponent<MogoUIBtn>("BtnXuQi");
        btnXuQi.SetText(LanguageData.GetContent(100));
        var pressedMgr = btnXuQi.gameObject.AddComponent<BtnPressedMgr>();
        pressedMgr.ActionPressed = OnPressXuQi;
        pressedMgr.ActionUnPressed = OnUnPressXuQi;

        m_btnFly = FindComponent<MogoUIBtn>("BtnFly");
        m_btnFly.ClickAction = OnBtnFly;
        m_btnJump = FindComponent<MogoUIBtn>("BtnJump");
        m_btnJump.ClickAction = OnBtnJump;
        m_btnJump.SetText(LanguageData.GetContent(105));

        m_goKeyCodeXuqi = FindTransform("KeyCodeXuqi").gameObject;
        m_goKeyCodeXuqi.transform.FindChild("LblKeyCodeXuqi").GetComponent<UILabel>().text = LanguageData.GetContent(151);
        m_goKeyCodeFlyState = FindTransform("KeyCodeFly").gameObject;
        m_goKeyCodeFlyState.transform.FindChild("LblKeyCodeFly").GetComponent<UILabel>().text = LanguageData.GetContent(152);
        m_goKeyCodeJump = FindTransform("KeyCodeJump").gameObject;
        m_goKeyCodeJump.transform.FindChild("LblKeyCodeJump").GetComponent<UILabel>().text = LanguageData.GetContent(154);
        SetKeyCodeActive();

        AddListeners();
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        SetCurrentMode();
    }

    protected override void OnRelease()
    {
        RemoveListeners();
        base.OnRelease();
    }

    private void AddListeners()
    {
        EventDispatcher.AddEventListener(GameConst.Event.IsFlyChanged, SetCurrentMode);
    }

    private void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener(GameConst.Event.IsFlyChanged, SetCurrentMode);
    }

    private void OnBtnFly(MogoUIBtn btn)
    {
    }

    private void OnBtnJump(MogoUIBtn btn)
    {
    }

    private void SetCurrentMode()
    {
    }

    private void OnPressXuQi()
    {
    }

    private void OnUnPressXuQi()
    {
    }

    private void SetKeyCodeActive()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                m_goKeyCodeXuqi.SetActive(false);
                m_goKeyCodeFlyState.SetActive(false);
                m_goKeyCodeJump.SetActive(false);
                break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                m_goKeyCodeXuqi.SetActive(true);
                m_goKeyCodeFlyState.SetActive(true);
                m_goKeyCodeJump.SetActive(true);
                break;
        }
    }
}
