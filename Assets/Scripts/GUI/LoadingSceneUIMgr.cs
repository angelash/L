using UnityEngine;
using System.Collections;
using Mogo.UI;

public class LoadingSceneUIMgr : UILogic {

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
            return new string[] { "LoadingSceneUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
    }
}
