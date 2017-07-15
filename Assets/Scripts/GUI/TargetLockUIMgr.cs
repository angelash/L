using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.Util;
using Mogo.GameData;

public class TargetLockUIMgr : UILogic
{

    private MogoUIBtn m_btnLock;
    private GameObject m_goKeyCode;

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
            return new string[] { "TargetLockUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.BattleUI.FindChild("Left");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(50, 60, 0);
        m_btnLock = FindComponent<MogoUIBtn>("BtnLock");
        m_btnLock.ClickAction = OnBtnLock;

        m_goKeyCode = FindTransform("KeyCode").gameObject;
        m_goKeyCode.transform.FindChild("LblKeyCode").GetComponent<UILabel>().text = LanguageData.GetContent(150);
        SetKeyCodeActive();

        AddListeners();
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        //SetBtnLockState(MogoWorld.thePlayer.IsTargetLock);
    }

    protected override void OnRelease()
    {
        RemoveListeners();
        base.OnRelease();
    }

    private void AddListeners()
    {
        EventDispatcher.AddEventListener(GameConst.Event.IsTargetLockChanged, OnTargetLockChanged);
    }

    private void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener(GameConst.Event.IsTargetLockChanged, OnTargetLockChanged);
    }

    private void OnTargetLockChanged()
    {
        //SetBtnLockState(MogoWorld.thePlayer.IsTargetLock);
    }

    private void SetBtnLockState(bool isLock)
    {
        if (isLock)
        {
            m_btnLock.SetText(LanguageData.GetContent(103));
        }
        else
        {
            m_btnLock.SetText(LanguageData.GetContent(104));
        }
    }

    private void OnBtnLock(MogoUIBtn btn)
    {
    }

    private void SetKeyCodeActive()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                m_goKeyCode.SetActive(false);
                break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                m_goKeyCode.SetActive(true);
                break;
        }
    }
}
