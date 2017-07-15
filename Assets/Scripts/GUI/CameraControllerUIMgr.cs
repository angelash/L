using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.Util;

public class CameraControllerUIMgr : UILogic
{

    private BoxCollider m_bc;

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
            return new string[] { "CameraControllerUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/BottomRight");
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        //float xAspect = (float)Screen.width / 1280;
        //float yAspect = (float)Screen.height / 720;
        Vector3 bgSize = FindTransform("Bg").localScale;
        m_myTransform.localPosition = new Vector3(-bgSize.x * 0.5f, bgSize.y * 0.5f, 0);
        m_bc = FindComponent<BoxCollider>("Bg");
        var pressedMgr = FindTransform("Bg").gameObject.AddComponent<BtnPressedMgr>();
        pressedMgr.dragHandler = OnDrag;
    }

    private void OnDrag(Vector2 dragDelta)
    {
        MogoWorld.MainCamera.MoveRotation(dragDelta);
    }

}
