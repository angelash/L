using UnityEngine;
using System.Collections;
using Mogo.Util;

public class Driver : MonoBehaviour
{
    void Start()
    {
        LoggerHelper.Info("--------------------------------------Game Start!-----------------------------------------");
        gameObject.AddComponent("Init");
        gameObject.AddComponent<DriverLib>();
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            Screen.SetResolution(720, 1280, false);
        }
    }
}
