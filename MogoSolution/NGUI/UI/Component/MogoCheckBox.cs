///////////////////////////////////////////
//Copyright (C): 4399 YiDao studio
//All rights reserved
//文件描述：如果有多组以上的checkbox group设置的值要不一样。用一个group的值设置要一样；
//如果需要多个checkbox 都可选 设置group 的值为 0；
//创建者：hwl
//创建日期: 
///////////////////////////////////////////
using UnityEngine;
using System.Collections;
using Mogo.Util;
using System;

public class MogoCheckBox : MonoBehaviour
{
    public GameObject m_bgDown;
    public Vector3 Vec3Offset = new Vector3(0, 2, 0);
    private bool m_checked = true;
    public Action<bool> OnChanged;
    public Action<int> ClickHandler;
    public bool ClickNotChangeState = false; //点击不直接切换Check状态
    public int id = 0;
    static public BetterList<MogoCheckBox> checkBoxContainer = new BetterList<MogoCheckBox>();
    static public MogoCheckBox current;
    public int group = 0;

    public bool Checked
    {
        get { return m_checked; }
        set
        {
            if (this.group == 0)
            {
                m_checked = value;
                m_bgDown.SetActive(value);
            }else
            {
                for (int i = 0; i < checkBoxContainer.size; i++)
                {
                    MogoCheckBox cb = checkBoxContainer[i];
                    if (cb != this && cb.group == this.group)
                    {
                        cb.m_bgDown.SetActive(false);
                    }
                }
                m_checked = true;
                m_bgDown.SetActive(true);
            }
            if (OnChanged != null)
                OnChanged(m_checked);
        }
    }

    void Start()
    {
        var ssList = transform.GetComponentsInChildren<UISprite>(true);
        m_bgDown = ssList[1].gameObject;
        SetFristSelect();
    }
    void OnEnable() { checkBoxContainer.Add(this); }
    void OnDisable() { checkBoxContainer.Remove(this); }

    void OnClick()
    {
        if (!ClickNotChangeState)
        {
            if (Checked)
            {
                m_bgDown.SetActive(false);
                Checked = false;
            }
            else
            {
                m_bgDown.SetActive(true);
                Checked = true;
            }
        }
        if (ClickHandler != null)
            ClickHandler(id);
    }

    void OnPress(bool isPressed)
    {
        UIUtils.ButtonIsPressing = isPressed;
        if (isPressed)
        {

            m_bgDown.transform.parent.localPosition -= Vec3Offset;
            //EventDispatcher.TriggerEvent(SettingEvent.UIDownPlaySound, gameObject.name);

        }
        else
        {
            m_bgDown.transform.parent.localPosition += Vec3Offset;
            //EventDispatcher.TriggerEvent(SettingEvent.UIUpPlaySound, gameObject.name);
        }
    }

    private int lastGroup = -1;
    private int curGroup = -1;

    private void SetFristSelect()
    {
        for (int i = 0; i < checkBoxContainer.size; i++)
        {
            MogoCheckBox cb = checkBoxContainer[i];
            if (cb.group > 0 && cb.group == this.group)
            {
                curGroup = cb.group;
                if ( i == 0)
                {
                    lastGroup = cb.group;
                    cb.m_bgDown.SetActive(true);
                    cb.m_checked = true;
                }
                else
                {
                    if (curGroup != lastGroup)
                    {
                        lastGroup = curGroup;
                        cb.m_bgDown.SetActive(true);
                        cb.m_checked = true;
                    }
                    else
                    {
                        cb.m_bgDown.SetActive(false);
                        cb.m_checked = false;
                    }
                    
                }
            }
        }
    }
};
