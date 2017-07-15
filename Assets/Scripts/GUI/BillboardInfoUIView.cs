using System;
using Mogo.Util;
using UnityEngine;
using Mogo.GameData;

public class BillboardInfoUIView : MonoBehaviour
{
    private Transform m_myTransform;
    private GameObject m_goLock;
    private GameObject m_goHeadInfo;
    private UILabel m_labName;
    private UILabel m_labKillNum;
    private UISprite m_spHPFg;

    private uint m_playerId;
    public bool isShowHP;
    public bool isLock;
    private const float HpMaxLength = 100.0f;
    private const uint SHOW_TIME = 5000;
    private uint m_hpShowTimer = uint.MaxValue;
    private int m_killCount = -1;

    void Awake()
    {
        m_myTransform = transform;
        m_goLock = m_myTransform.FindChild("BeLock").gameObject;
        m_goHeadInfo = m_myTransform.FindChild("HeadInfo").gameObject;
        m_labName = m_myTransform.FindChild("HeadInfo/HeadNameText").GetComponent<UILabel>();
        m_labKillNum = m_myTransform.FindChild("HeadInfo/HeadKillText").GetComponent<UILabel>();
        m_spHPFg = m_myTransform.FindChild("HeadInfo/BillBoardHP/BillBoardHPFG").GetComponent<UISprite>();
        m_myTransform.FindChild("BeLock/BeLockRText").GetComponent<UILabel>().text = "]";
        m_myTransform.FindChild("BeLock/BeLockLText").GetComponent<UILabel>().text = "[";
        m_myTransform.FindChild("HeadInfo/HeadKillDesc").GetComponent<UILabel>().text = LanguageData.GetContent(13);
    }

    public void SetPlayerId(uint pid)
    {
        m_playerId = pid;
    }

    public void SetName(string name)
    {
        m_labName.text = name;
    }

    public void SetKillCount(int killCount)
    {
        if (m_killCount != killCount)
        {
            m_labKillNum.text = killCount.ToString();
            m_killCount = killCount;
        }
    }

    public void ShowBeLock(bool isShow)
    {
        isLock = isShow;
        m_goLock.SetActive(isShow);
        if (isShow)
            ShowPlayerHP();
        else
            HideHP();
    }

    public void HideHP()
    {
        if (isLock)
            return;
        m_goHeadInfo.SetActive(false);
        isShowHP = false;
    }

    public void ShowPlayerHP()
    {
    }

    /// <summary>
    /// 更新Billboard的位置
    /// </summary>
    /// <param name="position"></param>
    public void UpdatePosi(Vector3 position)
    {
        if (!m_myTransform)
            return;
        m_myTransform.position = position;
    }

    /// <summary>
    /// 根据距离更新scale
    /// </summary>
    /// <param name="dis"></param>
    public void UpdateScale(float dis)
    {
        if (!m_myTransform)
            return;
        Vector3 scale;
        if (dis > 30)
            scale = new Vector3(0.6f, 0.6f, 1.0f);
        else if (dis > 25)
            scale = new Vector3(0.7f, 0.7f, 1.0f);
        else if (dis > 20)
            scale = new Vector3(0.8f, 0.8f, 1.0f);
        else if (dis > 10)
            scale = new Vector3(0.9f, 0.9f, 1.0f);
        else if (dis > 5)
            scale = Vector3.one;
        else if (dis > 2)
            scale = new Vector3(1.2f, 1.2f, 1.0f);
        else if (dis > 1)
            scale = new Vector3(1.4f, 1.4f, 1.0f);
        else
            scale = new Vector3(1.6f, 1.6f, 1.0f);

        TweenScale.Begin(gameObject, 0.1f, scale);
    }

    public void Release()
    {
        TimerHeap.DelTimer(m_hpShowTimer);
        GameObject.Destroy(gameObject);
    }
}
