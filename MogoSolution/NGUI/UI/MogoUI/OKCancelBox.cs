using UnityEngine;
using System.Collections;
using Mogo.Util;
using System;

public class OKCancelBox : MonoBehaviour
{

    public UILabel m_lblBoxText;

    public MogoUIBtn m_okButton;
    public MogoUIBtn m_cancelButton;
    //UILabel m_lblOKBtnText;
    //UILabel m_lblCancelBtnText;

    public UISprite m_spOKBgUp;
    public UISprite m_spOKBgDown;
    public UISprite m_spCancelBgUp;
    public UISprite m_spCancelBgDown;
    public bool m_isInForwardLoading = false;

    Transform m_myTransform;

    Action<bool> m_actCallback;

    GameObject m_goMessageBoxCamera;

    public void ShowAsOK()
    {
        //m_goMessageBoxCamera.SetActive(true);
        if (m_cancelButton && m_cancelButton.gameObject.activeSelf)
            m_cancelButton.gameObject.SetActive(false);
        if (m_okButton)
            m_okButton.transform.localPosition = new Vector3(0, -100.0f, 0);
        //m_lblCancelBtnText.transform.parent.gameObject.SetActive(false);
        //m_lblOKBtnText.transform.parent.localPosition = new Vector3(0, -100.0f, 0);
    }

    public void ShowAsOKCancel()
    {
        //m_goMessageBoxCamera.SetActive(true);
        if (m_cancelButton && !m_cancelButton.gameObject.activeSelf)
            m_cancelButton.gameObject.SetActive(true);
        if (m_okButton)
            m_okButton.transform.localPosition = new Vector3(150.0f, -100.0f, 0);
        //m_lblCancelBtnText.transform.parent.gameObject.SetActive(true);
        //m_lblOKBtnText.transform.parent.localPosition = new Vector3(150.0f, -100.0f, 0);
    }

    void Awake()
    {

        m_myTransform = transform;

        //Initialize();
        if (!m_lblBoxText)
            m_lblBoxText = m_myTransform.FindChild("OKCancelText").GetComponentsInChildren<UILabel>(true)[0];
        if (!m_okButton)
            m_okButton = m_myTransform.FindChild("OKButton").GetComponent<MogoUIBtn>();
        if (!m_cancelButton)
            m_cancelButton = m_myTransform.FindChild("CancelButton").GetComponent<MogoUIBtn>();

        m_okButton.ClickEvent = OnOKButtonUp;
        m_cancelButton.ClickEvent = OnCancelButtonUp;

        //m_lblOKBtnText = m_myTransform.FindChild("OKButton/OKButtonText").GetComponentsInChildren<UILabel>(true)[0];
        //m_lblCancelBtnText = m_myTransform.FindChild("CancelButton/CancelButtonText").GetComponentsInChildren<UILabel>(true)[0];

        if (!m_spOKBgUp)
            m_spOKBgUp = m_myTransform.FindChild("OKButton/ImgNormal").GetComponentsInChildren<UISprite>(true)[0];
        if (!m_spOKBgDown)
            m_spOKBgDown = m_myTransform.FindChild("OKButton/ImgPressed").GetComponentsInChildren<UISprite>(true)[0];
        if (!m_spCancelBgUp)
            m_spCancelBgUp = m_myTransform.FindChild("CancelButton/ImgNormal").GetComponentsInChildren<UISprite>(true)[0];
        if (!m_spCancelBgDown)
            m_spCancelBgDown = m_myTransform.FindChild("CancelButton/ImgPressed").GetComponentsInChildren<UISprite>(true)[0];

        //Debug.LogError("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ awake");
        //if (!m_isInForwardLoading)
        //{
        //    m_goMessageBoxCamera = m_myTransform.parent.parent.parent.parent.FindChild("MessageBoxCamera").gameObject;
        //    m_goMessageBoxCamera.SetActive(false);
        //}

        if (!m_isInForwardLoading)
        {
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        m_lblBoxText = null;
        m_okButton = null;
        m_cancelButton = null;
        //m_lblOKBtnText = null;
        //m_lblCancelBtnText = null;
        m_spOKBgUp = null;
        m_spOKBgDown = null;
        m_spCancelBgUp = null;
        m_spCancelBgDown = null;
        m_myTransform = null;
        m_goMessageBoxCamera = null;
    }

    void OnOKButtonUp()
    {
        if (m_actCallback != null)
        {
            m_actCallback(true);
        }
    }

    void OnCancelButtonUp()
    {
        if (m_actCallback != null)
        {
            m_actCallback(false);
        }
    }

    //void Initialize()
    //{
    //    EventDispatcher.AddEventListener("OKButtonUp", OnOKButtonUp);
    //    EventDispatcher.AddEventListener("CancelButtonUp", OnCancelButtonUp);
    //}

    //void Release()
    //{
    //    EventDispatcher.RemoveEventListener("OKButtonUp", OnOKButtonUp);
    //    EventDispatcher.RemoveEventListener("CancelButtonUp", OnCancelButtonUp);
    //}

    public void SetBoxText(string text)
    {
        m_lblBoxText.text = text;
    }

    public void SetOKBtnText(string text)
    {
        m_okButton.Text = text;
    }

    public void SetCancelBtnText(string text)
    {
        m_cancelButton.Text = text;
    }

    public void SetCallback(Action<bool> callback)
    {
        m_actCallback = callback;
    }

    void OnDisable()
    {
        if (!m_isInForwardLoading)
        {
            EventDispatcher.TriggerEvent("OKCancelBoxClose");
            //m_goMessageBoxCamera.SetActive(false);
        }
    }
}
