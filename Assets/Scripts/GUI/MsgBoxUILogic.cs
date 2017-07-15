using System;
using UnityEngine;
using System.Collections;
using Mogo.UI;
using Mogo.GameData;

public class MsgBoxUILogic : UILogic
{
    #region 框架
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
            return new string[] { "MsgBoxUI.prefab" };
        }
    }

    private UILabel m_labContent;
    private UILabel m_labBtnOkText;
    private MogoUIBtn m_btnOk;
    private MogoUIBtn m_btnCancel;

    private Action okAction;
    private Action cancelAction;

    protected override void OnResourceLoaded()
    {
        var parent = MogoWorld.m_uiManager.MogoMainUIPanel;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0f, 0f, 0f);

        m_labContent = FindComponent<UILabel>("ContentText");
        m_labBtnOkText = FindComponent<UILabel>("BtnOkText");
        m_btnOk = FindComponent<MogoUIBtn>("BtnOk");
        m_labBtnOkText.text = LanguageData.GetContent(3);
        m_btnOk.ClickAction = OnClickOk;

        m_btnCancel = FindComponent<MogoUIBtn>("BtnCancel");
        FindComponent<UILabel>("BtnCancelText").text = LanguageData.GetContent(4);
        m_btnCancel.ClickAction = OnClickCanCel;
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        if (param != null && param.Length >= 1)
        {
            var result = param[0] as MsgBoxInfo;
            m_labContent.text = result.Content;
            if (result.ShowBtn)
            {
                m_labBtnOkText.text = result.BtnOkText;
                okAction = result.okAction;
                cancelAction = result.cancelAction;
                m_btnOk.gameObject.SetActive(true);
                m_btnCancel.gameObject.SetActive(true);
            }
            else
            {
                m_btnOk.gameObject.SetActive(false);
                m_btnCancel.gameObject.SetActive(false);
            }
        }

    }

    #endregion

    #region 事件

    private void OnClickOk(MogoUIBtn btn)
    {
        if (okAction!=null)
        {
            okAction();
            okAction = null;
        }
        Close();
    }

    private void OnClickCanCel(MogoUIBtn btn)
    {
        if (cancelAction != null)
        {
            cancelAction();
            cancelAction = null;
        }
        Close();
    }

    #endregion

    #region 界面

    public void UpdateContent(string content)
    {
        m_labContent.text = content;
    }

    #endregion
}

public class MsgBoxInfo
{
    public string Content { get; set; }
    public string BtnOkText { get; set; }

    public Action okAction { get; set; }
    public Action cancelAction { get; set; }

    public bool ShowBtn = true;
}
