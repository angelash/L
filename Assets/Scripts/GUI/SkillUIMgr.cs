using UnityEngine;
using System.Collections.Generic;
using Mogo.UI;
using Mogo.Util;
using Mogo.GameData;

public class SkillUIMgr : UILogic {

    private GameObject m_skillIcon;

    private float m_iconWidth;
    private const float m_diffIcon = 20f;
    private Dictionary<int, GameObject> m_dicIcons = new Dictionary<int,GameObject>();

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
            return new string[] { "SkillUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/Bottom");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0, 60, 0);
        m_skillIcon = FindTransform("SkillIcon").gameObject;
        m_skillIcon.SetActive(false);
        m_iconWidth = m_skillIcon.transform.FindChild("ImgNormal").localScale.x;

        LoadSkillData();
        AddListeners();
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
    }

    protected override void OnHide(System.Action callback)
    {
        //ClearSkillIcons();
    }

    protected override void OnRelease()
    {
        RemoveListeners();
        base.OnRelease();
    }

    private void AddListeners()
    {
        EventDispatcher.AddEventListener(GameConst.Event.CurSkillChanged, OnCurSkillChanged);
    }

    private void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener(GameConst.Event.CurSkillChanged, OnCurSkillChanged);
    }

    private void OnCurSkillChanged()
    {
    }

    private void SetSkillIcons(List<int> listSkillId)
    {
        int skillCount = listSkillId.Count;
        float maxWidth = (m_iconWidth + m_diffIcon) * skillCount;
        float startPosX = -maxWidth * 0.5f + m_iconWidth * 0.5f;
        for (int i = 0; i < skillCount; i++)
        {
            int skillId = listSkillId[i];
            GameObject iconClone = GameObject.Instantiate(m_skillIcon) as GameObject;
            iconClone.transform.parent = m_skillIcon.transform.parent;
            iconClone.transform.localScale = Vector3.one;
            float posX = startPosX + (m_iconWidth + m_diffIcon) * i;
            iconClone.transform.localPosition = new Vector3(posX, m_skillIcon.transform.localPosition.y, m_skillIcon.transform.localPosition.z);
            iconClone.transform.FindChild("ImgIcon").GetComponent<UISprite>().spriteName = string.Concat("skill_", skillId + 1);
            iconClone.SetActive(true);
            var btnMgr = iconClone.GetComponent<MogoUIBtn>();
            btnMgr.IDUint64 = (ulong)skillId;
            btnMgr.ClickActionUInt64 = OnBtnIcon;

            var goKeyCode = iconClone.transform.FindChild("KeyCode").gameObject;
            iconClone.transform.FindChild("KeyCode/LblKeyCode").GetComponent<UILabel>().text = (skillId + 1).ToString();
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.Android:
                    goKeyCode.SetActive(false);
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    goKeyCode.SetActive(true);
                    break;
            }

            m_dicIcons[skillId] = iconClone;
        }
    }

    private void ClearSkillIcons()
    {
        var etor = m_dicIcons.GetEnumerator();
        while (etor.MoveNext())
        {
            GameObject.Destroy(etor.Current.Value);
        }
        m_dicIcons.Clear();
    }

    private void OnBtnIcon(MogoUIBtn btn, ulong id)
    {
    }

    private void SetCurrentSkillIcon(int skillId)
    {
        //LoggerHelper.Debug("skillId=" + skillId);
        if (!m_dicIcons.ContainsKey(skillId))
        {
            LoggerHelper.Debug("m_dicIcons does not contains skillId=" + skillId);
            return;
        }

        string imgName = "ImgLight";
        var etor = m_dicIcons.GetEnumerator();
        while (etor.MoveNext())
        {
            GameObject go = etor.Current.Value;
            go.transform.FindChild(imgName).gameObject.SetActive(false);
        }
        m_dicIcons[skillId].transform.FindChild(imgName).gameObject.SetActive(true);
    }

    private void LoadSkillData()
    {
        List<int> listSkillId = new List<int>();
        var etor = SkillData.dataMap.GetEnumerator();
        while (etor.MoveNext())
        {
            listSkillId.Add(etor.Current.Key);
        }
        SetSkillIcons(listSkillId);
    }

}
