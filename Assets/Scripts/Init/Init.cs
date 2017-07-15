using UnityEngine;
using System.Collections;
using System.Net;
using Mogo.Util;
using System;
using Mogo.GameData;
using Mogo.Game;

public class Init : MonoBehaviour
{
    public static GameObject goDiver;
    private bool hasInitFinish;

    void Start()
    {
        goDiver = gameObject;
        Application.targetFrameRate = 30;
        Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;
        gameObject.AddComponent<LoadResources>();
        var gameData = GameDataControler.Instance;

        if (Application.platform == RuntimePlatform.Android)
        {
            gameObject.AddComponent<PlatformMessageReceiver>();
        }

        SystemConfig.Init((flag) =>
        {
            serverIp = SystemConfig.GetCfgInfoUrl("serverlist");
            gameData.Init(null, () =>
            {
                MogoWorld.Init(MogoWorld.StartGame);
                hasInitFinish = true;
            }, null);
        });
    }

    void Update()
    {
        TimerHeap.Tick();
        FrameTimerHeap.Tick();
        if (hasInitFinish)
        {
            MogoWorld.Process();

            if (MogoWorld.thePlayer != null && MogoWorld.thePlayer.ViewTransform != null)
            {
                transform.localPosition = MogoWorld.thePlayer.ViewTransform.localPosition;
            }
        }
    }

    string serverIp = "";
    bool m_switch;


    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 10, 10), ""))
        {
            m_switch = !m_switch;
        }

        if (!m_switch)
            return;

        if (GUI.Button(new Rect(10, 0, 90, 100), "touch"))
        {
            foreach (var item in MogoWorld.m_dataMapManager.ReachStarList)
            {
                foreach (var soliders in item.SoldierDic.Values)
                {
                    foreach (var solider in soliders.Values)
                    {
                        solider.OnHit();
                    }
                }
            }
            //MogoWorld.thePlayer.controllerInput.SetStickType(StickType.Touch);
        }

        serverIp = GUI.TextArea(new Rect(0, 100, 150, 50), serverIp);
        if (GUI.Button(new Rect(100, 0, 100, 100), "CreateServer"))
        {
            serverIp = MogoWorld.CreateServer();
        }
        if (GUI.Button(new Rect(200, 0, 100, 100), "login"))
        {
            MogoWorld.ServerIP = serverIp;
            MogoWorld.ConnectServer(serverIp, 43998);
            MogoWorld.TmpPlayerName = UtilsEx.GetRandomName();
            MogoWorld.Login();
        }
        if (GUI.Button(new Rect(300, 0, 100, 100), "createAvatar"))
        {
            MogoWorld.thePlayer.RpcCall("CreateAvatarReq");
        }
        if (GUI.Button(new Rect(400, 0, 100, 100), "vrWidth+"))
        {
            MogoWorld.MainCamera.SetVRWidth(true);
        }
        if (GUI.Button(new Rect(500, 0, 100, 100), "vrWidth-"))
        {
            MogoWorld.MainCamera.SetVRWidth(false);
        }
        if (GUI.Button(new Rect(600, 0, 100, 100), "vrRot+"))
        {
            MogoWorld.MainCamera.SetVRRotation(true);
        }
        if (GUI.Button(new Rect(700, 0, 100, 100), "vrRot-"))
        {
            MogoWorld.MainCamera.SetVRRotation(false);
        }
        if (GUI.Button(new Rect(800, 0, 100, 100), "vr"))
        {
            MogoWorld.MainCamera.SwitchVR();
        }
        if (GUI.Button(new Rect(0, 100, 100, 100), "into main"))
        {
            MogoWorld.EnterMainEufloria(1);
        }
        if (GUI.Button(new Rect(100, 100, 100, 100), "init map"))
        {
            UIManager.I.GetUILogic<MapUIMgr>().InitStarMap(1);
        }
        if (GUI.Button(new Rect(200, 100, 100, 100), "remove soldier"))
        {
            MogoWorld.thePlayer.RpcCall("RemoveSoldier", MogoWorld.thePlayer.ID, 0, 1, 100,1);
        }
        if (GUI.Button(new Rect(300, 100, 100, 100), "ShowSoldierIntrusionStar"))
        {
            //MogoWorld.m_dataMapManager.addSoldierToStar(MogoWorld.thePlayer.ID, 1, 1);
            //MogoWorld.thePlayer.RpcCall("ArrayedSoldier", 1, 1);
           // MogoWorld.m_dataMapManager.ShowSoldierIntrusionStar(MogoWorld.thePlayer.ID, 1, 1, 150);
        }
        if (GUI.Button(new Rect(400, 100, 100, 100), "stop game"))
        {
            //MogoWorld.m_dataMapManager.addSoldierToStar(MogoWorld.thePlayer.ID, 1, 1);
            //MogoWorld.thePlayer.RpcCall("ArrayedSoldier",1, 2);
            //UIManager.I.GetUILogic<MapUIMgr>().EraserTexture();
            //MogoWorld.IsInGame = false;
            MogoWorld.thePlayer.RpcCall("GameOver", 1);
        }
        if (GUI.Button(new Rect(500, 100, 100, 100), "OpenRangleCheck"))
        {
            //MogoWorld.m_dataMapManager.addSoldierToStar(MogoWorld.thePlayer.ID, 1, 1);
            //MogoWorld.thePlayer.RpcCall("ArrayedSoldier",1, 2);
            //UIManager.I.GetUILogic<MapUIMgr>().EraserTexture();
            UIManager.I.GetUILogic<StarInfoUIMgr>().IsOpenRangleCheck = !UIManager.I.GetUILogic<StarInfoUIMgr>().IsOpenRangleCheck;
        }

    }
    private int i = 0;

    void OnApplicationQuit()
    {
        LoggerHelper.Release();
        MogoWorld.Quit();
    }
}
