using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.Util;

public class PlayerHeadMgr : UILogic
{

    private UILabel m_lblmp;
    private UILabel m_lblHp;
    private UILabel m_lblName;
    private Transform m_mpBar;
    private Transform m_hpBar;

    private float m_mpMaxLength;
    private float m_hpMaxLength;

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
            return new string[] { "PlayerHeadUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/TopLeft");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(180f, -65f, 0);
        m_myTransform.localScale = new Vector3(1.3f, 1.3f, 1f);
        m_mpBar = FindTransform("MpBar");
        m_mpMaxLength = m_mpBar.localScale.x;

        m_hpBar = FindTransform("HpBar");
        m_hpMaxLength = m_hpBar.localScale.x;

        m_lblmp = FindComponent<UILabel>("LblMp");
        m_lblHp = FindComponent<UILabel>("LblHp");
        m_lblName = FindComponent<UILabel>("LblName");

        AddListeners();
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        SetMpBar();
        SetHpBar();
        m_lblName.text = MogoWorld.thePlayer.Name;
    }

    private void AddListeners()
    {
        EventDispatcher.AddEventListener(GameConst.Event.HpChanged, SetHpBar);
        EventDispatcher.AddEventListener(GameConst.Event.EnergyChanged, SetMpBar);
    }

    private void SetMpBar()
    {
    }

    private void SetHpBar()
    {
    }
}
