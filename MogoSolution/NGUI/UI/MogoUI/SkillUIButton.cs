using UnityEngine;
using System.Collections;
using Mogo.Util;

public class SkillUIButton : MonoBehaviour {

    public int ID = 0;

    void Awake()
    {
        gameObject.AddComponent<MogoFakeClick>().ReletedClassType = ReleadClassType.Type_SkillUI;
    }

    void OnClick()
    {
        if (SkillUIDict.ButtonTypeToEventUp[transform.name] == null)
        {
            LoggerHelper.Error("No ButtonTypeToEventUp Info");
            return;
        }

        SkillUIDict.ButtonTypeToEventUp[transform.name](ID);
    }

    public void FakePress(bool isPressed)
    {
        if (SkillUIDict.ButtonTypeToEventUp[transform.name] == null)
        {
            LoggerHelper.Error("No ButtonTypeToEventUp Info");
            return;
        }

        SkillUIDict.ButtonTypeToEventUp[transform.name](ID);
    }
}
