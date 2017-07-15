using Mogo.Util;
///////////////////////////////////////////
//Copyright (C): 4399
//文件描述：UI按钮
//创建者：hongjie
//创建日期: 
///////////////////////////////////////////
using System;
using UnityEngine;
using TDBID = System.UInt64;

public class MogoUIBtn : MonoBehaviour
{
    public TDBID IDUint64 = 0;
    //public int PressOffset = -2;
    //public Vector3 Vec3PressScale = new Vector3(0.95f, 0.95f, 1f);
    private bool m_autoFixBoxCollider = true;

    public bool AutoFixBoxCollider
    {
        get { return m_autoFixBoxCollider; }
        set { m_autoFixBoxCollider = value; }
    }
    public Action ClickEvent;
    public Action<MogoUIBtn> ClickAction;
    public Action<MogoUIBtn, TDBID> ClickActionUInt64; //支持64位整数的Action

    public const int OFFSET_BOXCOLLIDER = 10; //BoxCollider大小偏移量，为了在手机上更容易点中

    public UISprite m_imgNormal;
    public UISprite m_imgPressed;
    public UILabel m_lblText;
    public BoxCollider m_boxCollider;
    private bool m_useShowAsWhiteBlack = true;

    public bool UseShowAsWhiteBlack
    {
        get { return m_useShowAsWhiteBlack; }
        set { m_useShowAsWhiteBlack = value; }
    }
    private bool m_isEnabled = true;
    private bool m_isActive = true;

    public bool IsActive
    {
        get { return m_isActive; }
    }

    public string Text
    {
        get
        {
            if (m_lblText == null)
                return string.Empty;
            return m_lblText.text;
        }
        set
        {
            if (m_lblText)
                m_lblText.text = value;
        }
    }

    /// <summary>
    /// 是否可按
    /// </summary>
    public bool IsEnabled
    {
        get { return m_isEnabled; }
        set
        {
            if (value == m_isEnabled)
            {
                return;
            }
            m_isEnabled = value;
            if (UseShowAsWhiteBlack && m_imgNormal)
            {
                m_imgNormal.gameObject.SetActive(false);
                m_imgNormal.ShowAsWhiteBlack(!m_isEnabled);
                m_imgNormal.gameObject.SetActive(true);
            }
        }
    }
    //public Vector3 Vec3PressOffset { get; set; }
    [HideInInspector]
    public Vector3 OrgScale { get; set; }
    [HideInInspector]
    public Vector3 PressScale { get; set; }
    public Action<Vector2> dragHandler;
    public Action<Vector2> dragOverHandler;
    public Action<GameObject> dropHandler;
    public Action<bool> pressHandler;

    private bool isDragging = false;
    private Vector2 dragDelta;

    /// <summary>
    /// 按钮点击音效类型
    /// 1：弱
    /// 2：中
    /// 3：强
    /// 4：打开
    /// 5：关闭
    /// </summary>
    public ButtonClickSoundType buttonType = ButtonClickSoundType.Weak;

    void Awake()
    {
        if (!m_imgNormal)
        {
            var go = transform.FindChild("ImgNormal");
            if (go)
                m_imgNormal = go.GetComponent<UISprite>();
        }
        if (!m_imgPressed)
        {
            var go = transform.FindChild("ImgPressed");
            if (go)
                m_imgPressed = go.GetComponent<UISprite>();
        }
        if (!m_lblText)
        {
            var go = transform.FindChild("LblText");
            if (go)
                m_lblText = go.GetComponent<UILabel>();
        }
        if (!m_boxCollider)
            m_boxCollider = transform.GetComponent<BoxCollider>();
        if (m_imgNormal)
        {
            m_boxCollider.size = m_imgNormal.transform.localScale + new Vector3(OFFSET_BOXCOLLIDER, OFFSET_BOXCOLLIDER, 1.0f);
            m_imgNormal.gameObject.SetActive(true);
        }
        if (m_imgPressed)
        {
            m_imgPressed.gameObject.SetActive(false);
        }
        if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Scale))
        {
            OrgScale = transform.localScale;
            PressScale = new Vector3(OrgScale.x * UIUtils.ButtonPressScale.x, OrgScale.y * UIUtils.ButtonPressScale.y, OrgScale.z * UIUtils.ButtonPressScale.z);
        }
        //Vec3PressOffect = new Vector3(0, PressOffset, 0);
        m_isActive = gameObject.activeSelf;
    }

    void OnPress(bool isPressed)
    {
        if (!m_isEnabled)
            return;

        UIUtils.ButtonIsPressing = isPressed;
        if (isPressed)
        {
            EventDispatcher.TriggerEvent("Click_Button", buttonType);
            if (m_imgPressed)
            {
                m_imgPressed.gameObject.SetActive(true);
            }
            if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Scale))
                TweenScale.Begin(gameObject, 0.1f, PressScale);
            if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Offet))
                transform.localPosition += UIUtils.ButtonPressOffset;
        }
        else
        {
            if (m_imgPressed)
            {
                m_imgPressed.gameObject.SetActive(false);
            }
            if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Scale))
                TweenScale.Begin(gameObject, 0.1f, OrgScale);
            if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Offet))
                transform.localPosition -= UIUtils.ButtonPressOffset;
            //按钮复位时才触发
            if (isDragging)
            {
                if (pressHandler != null)
                {
                    pressHandler(isPressed);
                }
                if (dragOverHandler != null)
                {
                    dragOverHandler(dragDelta);
                }
            }
            else
            {
                if (pressHandler != null)
                {
                    pressHandler(isPressed);
                }
                if (ClickActionUInt64 != null)
                {
                    ClickActionUInt64(this, IDUint64);
                }
                if (ClickAction != null)
                {
                    ClickAction(this);
                }
                if (ClickEvent != null)
                {
                    ClickEvent();
                }
            }
            isDragging = false;
        }
    }

    void OnDrag(Vector2 delta)
    {
        isDragging = true;
        dragDelta = delta;
        if (dragHandler != null)
        {
            dragHandler(delta);
        }
    }

    void OnDrop(GameObject drop)
    {
        if (dropHandler != null)
        {
            dropHandler(drop);
        }
    }

    public void SetActive(bool value)
    {
        if (gameObject)
            gameObject.SetActive(value);
        m_isActive = value;
    }

    public void SetEnabled(bool isEnabled)
    {
        if (isEnabled == m_isEnabled)
        {
            return;
        }
        m_isEnabled = isEnabled;
        if (m_boxCollider)
        {
            m_boxCollider.enabled = m_isEnabled;
        }
        if (m_imgNormal)
        {
            m_imgNormal.gameObject.SetActive(false);
            m_imgNormal.ShowAsWhiteBlack(!m_isEnabled);
            m_imgNormal.gameObject.SetActive(true);
        }
    }

    public void SetText(string text)
    {
        if (m_lblText)
        {
            m_lblText.text = text;
        }
    }

    public string GetText()
    {
        if (m_lblText == null)
        {
            return string.Empty;
        }
        return m_lblText.text;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    /// <summary>
    /// 针对仅置灰但仍可点击的需求
    /// </summary>
    /// <param name="isGray"></param>
    public void SetGray(bool isGray)
    {
        m_imgNormal.gameObject.SetActive(false);
        m_imgPressed.gameObject.SetActive(false);
        m_imgNormal.ShowAsWhiteBlack(isGray);
        m_imgPressed.ShowAsWhiteBlack(isGray);
    }

}
