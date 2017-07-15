using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Mogo.Util;

public class MogoPager : MonoBehaviour
{
    public int PageID;
    private Transform m_transform;
    private GameObject m_bgUp;
    private GameObject m_bgDown;

    void Awake()
    {
        m_transform = transform;
        var ssList = m_transform.GetComponentsInChildren<UISlicedSprite>(true);
        m_bgUp = ssList[0].gameObject;
        m_bgDown = ssList[1].gameObject;
        EventDispatcher.AddEventListener<byte,byte>(UIEvent.ChangePage, OnPageChange);
    }

    void OnPageChange(byte toPage,byte maxPage)
    {
        if (m_bgUp != null && m_bgDown != null)
        {
            if (maxPage == 0 || PageID > maxPage)
            {
                m_bgUp.SetActive(false);
                m_bgDown.SetActive(false);
            }
            else
            {
                m_bgDown.SetActive(true);
                if (PageID == toPage)
                {
                    m_bgUp.SetActive(true);
                }
                else
                {
                    m_bgUp.SetActive(false);
                }
            }
        }
    }
}
