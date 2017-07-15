using UnityEngine;
using System.Collections;
using Mogo.Util;

using TDBID = System.UInt64;

public class EnemyUIButton : MonoBehaviour {

    public TDBID id;
    public Camera RelatedCamera;

    void Awake()
    {
        gameObject.AddComponent<MogoFakeClick>().ReletedClassType = ReleadClassType.Type_EnemyUI;
    }

  


    void OnPress(bool isOver)
    {
        if (isOver)
        {
        }
        else
        {
            Camera camera;

            if (RelatedCamera == null)
            {
                camera = GameObject.Find("MogoMainUI").transform.GetChild(0).GetComponentInChildren<Camera>();
            }
            else
            {
                camera = RelatedCamera;
            }

            BoxCollider bc = transform.GetComponentInChildren<BoxCollider>();

            RaycastHit hit = new RaycastHit();

            if (bc.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 10000.0f))
            {
                if (EnemyUIDict.ButtonTypeToEventUp[transform.name] == null)
                {
                    LoggerHelper.Error("No ButtonTypeToEventUp Info");
                    return;
                }

                EnemyUIDict.ButtonTypeToEventUp[transform.name](id);
            }
        }

    }

    public void FakePress(bool isPressed)
    {
        if (EnemyUIDict.ButtonTypeToEventUp[transform.name] == null)
        {
            LoggerHelper.Error("No ButtonTypeToEventUp Info");
            return;
        }

        EnemyUIDict.ButtonTypeToEventUp[transform.name](id);
    }
}
