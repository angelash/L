using UnityEngine;
using System.Collections;
using System.IO;
using Mogo.UI;
using Mogo.GameData;

public class ServerInfoUI : UILogic {

    private UILabel m_lblIP;

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
            return new string[] { "ServerInfoUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.BattleUI.FindChild("TopRight");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        Transform bg = FindTransform("Bg");
        float diff = 5f;
        m_myTransform.localPosition = new Vector3(-bg.localScale.x * 0.5f - diff, -bg.localScale.y * 0.5f - diff, 0);
        m_lblIP = FindComponent<UILabel>("LblIP");

        FindComponent<MogoUIBtn>("Panel").ClickEvent = CreateQrCode;
    }

    private void CreateQrCode()
    {
        if (UIManager.I.GetUILogic<QRCodeUIMgr>().MyGameObject != null && UIManager.I.GetUILogic<QRCodeUIMgr>().MyGameObject.activeSelf)
        {
            UIManager.I.GetUILogic<QRCodeUIMgr>().MyGameObject.SetActive(false);
            return;
        }

        var path = Application.persistentDataPath + "/QRCode.png";
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call("generateQRCode", MogoWorld.ServerIP.ToString(), path,200,200);
        }
#endif

    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        string ip = MogoWorld.ServerIP.ToString();
        m_lblIP.text = LanguageData.GetContent(8, ip);
    }
}
