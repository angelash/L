using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections;
using UnityEngine;

public class MogoUIManager
{
    public Camera UICamera;
    public Camera TopUICamera;
    public Camera BillBoardCamera;
    public Transform billboardList;
    public Transform MogoMainUIPanel;
    public Transform MogoTopUIPanel;
    public Transform BattleUI;

    public GameObject m_goControllerUI;
    private GameObject m_goFlyControllerUI;
    private GameObject m_goRecoverEnergyUI;
    private GameObject m_goFPSUI;
    private GameObject m_goAttackUI1;
    private GameObject m_goAttackUI2;

    public ControllerStick StickMgr
    {
        get;
        private set;
    }

    public void LoadMainUI()
    {
        AssetCacheMgr.GetUIInstance("MogoMainUI.prefab", (prefab, guid, go) =>
        {
            var mainUI = go as GameObject;
            mainUI.transform.localPosition = new Vector3(5000, 5000, 0);
            MogoMainUIPanel = mainUI.transform.FindChild("Camera/Anchor/MogoMainUIPanel");
            UICamera = mainUI.transform.FindChild("Camera").GetComponent<Camera>();
            BillBoardCamera = mainUI.transform.FindChild("BillboardCamera").GetComponent<Camera>();
            billboardList = mainUI.transform.FindChild("Camera/Anchor/MogoMainUIPanel/BillboardList");
            BattleUI = MogoMainUIPanel.FindChild("BattleUI");
            MogoWorld.OnMainUILoaded();
        });
    }

    public void LoadTopUI()
    {
        AssetCacheMgr.GetUIInstance("MogoTopUI.prefab", (prefab, guid, go) =>
        {
            var topUI = go as GameObject;
            topUI.transform.localPosition = new Vector3(5000, 5000, 0);
            MogoTopUIPanel = topUI.transform.FindChild("Camera/Anchor/MogoTopUIPanel");
            TopUICamera = topUI.transform.FindChild("Camera").GetComponent<Camera>();
        });
    }

    public void ShowBattleController(Action callback = null)
    {
        if (m_goControllerUI != null)
        {
            m_goControllerUI.SetActive(true);
            if (callback != null)
            {
                callback();
            }
            return;
        }
        AssetCacheMgr.GetUIInstance("MainControllerUI.prefab", (prefab, guid, go) =>
        {
            m_goControllerUI = go as GameObject;
            m_goControllerUI.transform.parent = MogoMainUIPanel.transform.FindChild("BattleUI/BottomLeft");
            m_goControllerUI.transform.localPosition = new Vector3(150, 150, 0);
            m_goControllerUI.transform.localScale = Vector3.one;
            StickMgr = m_goControllerUI.AddComponent<ControllerStick>();
            m_goControllerUI.SetActive(true);
            if (callback != null)
            {
                callback();
            }
        });
    }

    public void ShowRecoverEnergyUI(Action callback = null)
    {
        if (m_goRecoverEnergyUI != null)
        {
            m_goRecoverEnergyUI.SetActive(true);
            if (callback != null)
            {
                callback();
            }
            return;
        }
        AssetCacheMgr.GetUIInstance("RecoverEnergyUI.prefab", (prefab, guid, go) =>
        {
            m_goRecoverEnergyUI = go as GameObject;
            m_goRecoverEnergyUI.transform.parent = MogoMainUIPanel.transform.FindChild("BattleUI/Center");
            m_goRecoverEnergyUI.transform.localPosition = new Vector3(0, -180, 0);
            m_goRecoverEnergyUI.transform.localScale = Vector3.one;
            m_goRecoverEnergyUI.AddComponent<RecoverEnergyUIMgr>();
            m_goRecoverEnergyUI.SetActive(true);
            if (callback != null)
            {
                callback();
            }
        });
    }

    public void CloseRecoverEnergyUI()
    {
        if (m_goRecoverEnergyUI != null)
        {
            m_goRecoverEnergyUI.SetActive(false);
        }
    }

    public bool IsRecoverEnergyUIShow()
    {
        if (m_goRecoverEnergyUI != null)
        {
            return m_goRecoverEnergyUI.activeSelf;
        }
        else
        {
            return false;
        }
    }

    public void ShowFlyControllerUI()
    {
        if (m_goFlyControllerUI != null)
        {
            m_goFlyControllerUI.SetActive(true);
            return;
        }
    }

    public void CloseFlyControllerUI()
    {
        if (m_goFlyControllerUI != null)
        {
            m_goFlyControllerUI.SetActive(false);
        }
    }

    public void ShowRebornNotice()
    {
        int seconds = 5;
        uint timerId = 0;
        MsgBoxInfo info = new MsgBoxInfo();
        info.Content = LanguageData.GetContent(122, seconds);
        info.ShowBtn = false;
        UIManager.I.ShowUI<MsgBoxUILogic>(info, delegate()
        {
            BattleUI.gameObject.SetActive(false);
            var mgr = UIManager.I.GetUILogic<MsgBoxUILogic>();
            timerId = TimerHeap.AddTimer(1000, 1000, delegate()
            {
                LoggerHelper.Debug("seconds=" + seconds);
                if (seconds < 0)
                {
                    TimerHeap.DelTimer(timerId);
                    mgr.Close();
                    BattleUI.gameObject.SetActive(true);
                    MogoWorld.thePlayer.RpcCall("Reborn", MogoWorld.thePlayer.ID);
                    return;
                }
                mgr.UpdateContent(LanguageData.GetContent(122, seconds));
                seconds--;
            });
        });
    }

    public void ShowFPSUI()
    {
        if (m_goFPSUI != null)
        {
            m_goFPSUI.SetActive(true);
            return;
        }
        AssetCacheMgr.GetUIInstance("FPSUI.prefab", (prefab, guid, go) =>
        {
            m_goFPSUI = go as GameObject;
            m_goFPSUI.transform.parent = MogoMainUIPanel.FindChild("TopLeft");
            m_goFPSUI.transform.localPosition = new Vector3(50, -140, 0);
            m_goFPSUI.transform.localScale = Vector3.one;
            m_goFPSUI.AddComponent<FPSUIMgr>();
            m_goFPSUI.SetActive(true);
        });
    }

    public void ShowBattleUI()
    {
        BattleUI.gameObject.SetActive(true);
        UIManager.I.CloseUI<GameStartUIMgr>();
        UIManager.I.CloseUI<WaitingUIMgr>();
        UIManager.I.ShowUI<PlayerHeadMgr>();
        UIManager.I.ShowUI<RightControllerUIMgr>();
        UIManager.I.ShowUI<SkillUIMgr>();
        UIManager.I.ShowUI<TargetLockUIMgr>();
        UIManager.I.ShowUI<ServerInfoUI>();
        UIManager.I.ShowUI<BattleScoreUIMgr>();
    }

    public void ShowAttackUI1()
    {
        if (m_goAttackUI1 != null)
        {
            m_goAttackUI1.SetActive(true);
            return;
        }
    }

    public void CloseAttackUI1()
    {
        if (m_goAttackUI1 != null)
        {
            m_goAttackUI1.SetActive(false);
        }
    }

    public void ShowAttackUI2()
    {
        if (m_goAttackUI2 != null)
        {
            m_goAttackUI2.SetActive(true);
            return;
        }
    }

    public void CloseAttackUI2()
    {
        if (m_goAttackUI2 != null)
        {
            m_goAttackUI2.SetActive(false);
        }
    }
}