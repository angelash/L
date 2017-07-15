/*----------------------------------------------------------------
// Copyright (C) 2013 ���ݣ�����
//
// ģ������MainUIButton
// �����ߣ�MaiFeo
// �޸����б�
// �������ڣ�
// ģ�������������水ť
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
