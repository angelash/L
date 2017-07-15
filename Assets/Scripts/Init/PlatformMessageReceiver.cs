using UnityEngine;

public class PlatformMessageReceiver : MonoBehaviour
{

    void QRCodeScanResult(string ip)
    {
        UIManager.I.GetUILogic<GameStartUIMgr>().SetServerIP(ip);
    }
    
    void LoadQrCode(string path)
    {
        UIManager.I.ShowUI<QRCodeUIMgr>(path);
    }
}
