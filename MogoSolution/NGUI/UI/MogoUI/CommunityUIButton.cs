using UnityEngine;
using System.Collections;
using Mogo.Util;

public class CommunityUIButton : MonoBehaviour 
{
    public int ID;

    void Awake()
    {
        gameObject.AddComponent<MogoFakeClick>().ReletedClassType = ReleadClassType.Type_CommunityUI;
    }

    void OnClick()
    {
        if(!CommunityUIDict.ButtonTypeToEventUp.ContainsKey(transform.name))
        {
            return;
        }
        if (CommunityUIDict.ButtonTypeToEventUp[transform.name] == null)
        {
            LoggerHelper.Warning("Dictionary no contain the key : " + transform.name);
            return;
        }

        CommunityUIDict.ButtonTypeToEventUp[transform.name](ID);
    }

    public void FakePress(bool isPressed)
    {
        if (CommunityUIDict.ButtonTypeToEventUp[transform.name] == null)
        {
            LoggerHelper.Error("No ButtonTypeToEventUp Info");
            return;
        }

        CommunityUIDict.ButtonTypeToEventUp[transform.name](ID);
    }
}
