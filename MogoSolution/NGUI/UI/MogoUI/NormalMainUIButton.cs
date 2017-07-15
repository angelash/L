using UnityEngine;
using System.Collections;
using Mogo.Util;
using System.Collections.Generic;

public class NormalMainUIButton : MonoBehaviour 
{
    List<Vector3> m_listInputPos = new List<Vector3>();
    public Camera RelatedCam;

    void Awake()
    {
        gameObject.AddComponent<MogoFakeClick>().ReletedClassType = ReleadClassType.Type_NormalMainUI;
    }


    void OnClick()
    {
        if (NormalMainUIDict.ButtonTypeToEventUp.ContainsKey(transform.name))
            NormalMainUIDict.ButtonTypeToEventUp[transform.name]();
    }

    public void FakePress(bool isOver)
    {
        if (NormalMainUIDict.ButtonTypeToEventUp[transform.name] == null)
        {
            LoggerHelper.Error("No ButtonTypeToEventUp Info");
            return;
        }
        NormalMainUIDict.ButtonTypeToEventUp[transform.name]();
    }
}
