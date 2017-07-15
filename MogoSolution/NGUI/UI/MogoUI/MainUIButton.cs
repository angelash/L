/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：MainUIButton
// 创建者：MaiFeo
// 修改者列表：
// 创建日期：
// 模块描述：主界面按钮
//----------------------------------------------------------------*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Mogo.Util;

public class MainUIButton : MonoBehaviour 
{
    List<Vector3> m_listInputPos = new List<Vector3>();

    void Awake()
    {
        gameObject.AddComponent<MogoFakeClick>().ReletedClassType = ReleadClassType.Type_BattleMainUI;
    }

    void OnPress(bool isPressed)
    {
        if (isPressed)
        {
            EventDispatcher.TriggerEvent(MainUIDict.ButtonTypeToEventUp[transform.name]);
        }
        UIUtils.ButtonIsPressing = isPressed;
    }

    public void FakePress(bool isPressed)
    {
        if (MainUIDict.ButtonTypeToEventUp[transform.name] == null)
        {
            LoggerHelper.Error("No ButtonTypeToEventUp Info");
            return;
        }
        EventDispatcher.TriggerEvent(MainUIDict.ButtonTypeToEventUp[transform.name]);
    }
}
