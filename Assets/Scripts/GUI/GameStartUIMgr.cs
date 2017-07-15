using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.GameData;
using Mogo.Util;
using Mogo.Game;
using System;

public class GameStartUIMgr : UILogic
{
    private UIInput m_inputPlayerName;
    private UIInput m_inputServerIP;

    private uint m_timerIdWaiting;

    private const int NAME_MAX_LENGTH = 4;

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
            return new string[] { "GameStartUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0f, -30f, 0f);

        var btnCreate = FindComponent<MogoUIBtn>("BtnCreateServer");
        btnCreate.SetText(LanguageData.GetContent(1));
        btnCreate.ClickAction = OnBtnCreate;

        var btnFind = FindComponent<MogoUIBtn>("BtnFindServer");
        btnFind.SetText(LanguageData.GetContent(2));
        btnFind.ClickAction = OnBtnFind;

        var btnJoin = FindComponent<MogoUIBtn>("BtnJoin");
        btnJoin.SetText(LanguageData.GetContent(11));
        btnJoin.ClickAction = OnBtnJoin;

        var btnScanJoin = FindComponent<MogoUIBtn>("BtnScanJoin");
        btnScanJoin.SetText(LanguageData.GetContent(12));
        btnScanJoin.ClickAction = OnBtnScanJoin;

        m_inputPlayerName = FindComponent<UIInput>("PlayerName");
        m_inputPlayerName.text = UtilsEx.GetRandomName();
        m_inputPlayerName.maxChars = NAME_MAX_LENGTH;
        m_inputPlayerName.onSubmit = OnSubmitName;
        m_inputPlayerName.lostFocusAction = OnLostFocusInputName;
        m_inputPlayerName.functionName = "OnSubmitName";
        FindComponent<UILabel>("LblPlayerName").text = LanguageData.GetContent(10);

        m_inputServerIP = FindComponent<UIInput>("ServerIP");
        m_inputServerIP.text = SystemConfig.GetCfgInfoUrl("serverlist");
        m_inputServerIP.onSubmit = OnSubmitName;
        FindComponent<UILabel>("LblIP").text = LanguageData.GetContent(9);

        var btnGuide = FindComponent<MogoUIBtn>("BtnWenHao");
        btnGuide.ClickAction = OnBtnGuide;
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        base.OnShow(param, callback);
        EventDispatcher.AddEventListener<string>(FindServerEvent.FindServerSucc, FindServerSucc);
    }

    protected override void OnHide(System.Action callback)
    {
        base.OnHide(callback);
        EventDispatcher.RemoveEventListener<string>(FindServerEvent.FindServerSucc, FindServerSucc);
    }

    private void OnBtnCreate(MogoUIBtn btn)
    {
        if (CheckNameError())
        {
            return;
        }
        string serverIp = MogoWorld.CreateServer();
        MogoWorld.IsSingleMatch = true;
        MogoWorld.ConnectServer(serverIp, 43998);
        MogoWorld.Login();
        MogoWorld.ServerIP = serverIp;
    }

    private void OnBtnFind(MogoUIBtn btn)
    {
        if (CheckNameError())
        {
            return;
        }
        //test
        if (UIManager.I.GetUILogic<WaitingUIMgr>().bShow)
        {
            UIManager.I.CloseUI<WaitingUIMgr>();
            EventDispatcher.TriggerEvent(FindServerEvent.FindServerStop);
        }
        else
        {
            UIManager.I.ShowUI<WaitingUIMgr>();
            EventDispatcher.TriggerEvent(FindServerEvent.FindServerStart);
        }
    }

    private void OnBtnJoin(MogoUIBtn btn)
    {
        if (CheckNameError())
        {
            return;
        }
        MogoWorld.ConnectServer(m_inputServerIP.text, 43998);
        MogoWorld.Login();
        MogoWorld.ServerIP = m_inputServerIP.text;
    }

    public void SetServerIP(string ip)
    {
        if(m_inputServerIP != null)
            m_inputServerIP.text = ip;
    }

    private void OnBtnScanJoin(MogoUIBtn btn)
    {
        if (CheckNameError())
        {
            return;
        }

#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call("OpenQrCodeScanActivity");
        }
#endif
    }

    private void FindServerSucc(string ip)
    {
        UIManager.I.CloseUI<WaitingUIMgr>();
        MsgBoxInfo info = new MsgBoxInfo();
        if (string.IsNullOrEmpty(ip))
        {
            info.Content = LanguageData.GetContent(6);
            info.BtnOkText = LanguageData.GetContent(5);
            info.okAction = () => 
            { 
                UIManager.I.CloseUI<MsgBoxUILogic>();
                UIManager.I.ShowUI<WaitingUIMgr>();
                EventDispatcher.TriggerEvent(FindServerEvent.FindServerStart);
            };
            info.cancelAction = () => { UIManager.I.CloseUI<MsgBoxUILogic>(); };
        }
        else
        {
            info.Content = LanguageData.GetContent(7, ip);
            info.BtnOkText = LanguageData.GetContent(3);
            info.okAction = () =>
            {
                Close();
                MogoWorld.ConnectServer(ip, 43998);
                MogoWorld.Login();
            };
            info.cancelAction = () => { UIManager.I.CloseUI<MsgBoxUILogic>(); };
        }
        UIManager.I.ShowUI<MsgBoxUILogic>(info);

        MogoWorld.ServerIP = ip;
    }

    private void OnLostFocusInputName()
    {
        OnSubmitName(m_inputPlayerName.text);
    }

    private void OnSubmitName(string str)
    {
        CheckNameError();
    }

    private void OnSubmitIP(string str)
    {

    }

    private bool CheckNameError()
    {
        string str = m_inputPlayerName.text;
        if (str.Length > 0 && str.Length <= NAME_MAX_LENGTH)
        {
            MogoWorld.TmpPlayerName = m_inputPlayerName.text;
            return false;
        }
        else
        {
            MsgBoxInfo info = new MsgBoxInfo();
            info.Content = LanguageData.GetContent(123, NAME_MAX_LENGTH);
            info.BtnOkText = LanguageData.GetContent(3);
            UIManager.I.ShowUI<MsgBoxUILogic>(info);
            return true;
        }
    }

    private void OnBtnGuide(MogoUIBtn _btn)
    {
        UIManager.I.ShowUI<NoviceGuideUIMgr>(true);
        UIManager.I.CloseUI<GameStartUIMgr>();
    }
}
