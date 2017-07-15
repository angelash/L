using UnityEngine;
using System.Collections;
using System.IO;
using Mogo.GameData;
using Mogo.UI;

public class QRCodeUIMgr : UILogic
{
    private UITexture qrCodeTexture;

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
            return new string[] { "QRCodeUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        SyncCreateUIInstanceWithRootTransform(Resources[0], MogoWorld.m_uiManager.BattleUI);
        qrCodeTexture = FindComponent<UITexture>("Bg");
        qrCodeTexture.gameObject.SetActive(false);
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        qrCodeTexture.gameObject.SetActive(false);
        if (param != null && param.Length > 0)
        {
            DriverLib.Instance.StartCoroutine(LoadQRCode(param[0] as string));
        }
    }

    private IEnumerator LoadQRCode(string path)
    {
        WWW www = new WWW("file://" + path); ;
        yield return www;
        qrCodeTexture.mainTexture = www.texture;
        qrCodeTexture.gameObject.SetActive(true);
    }
}
