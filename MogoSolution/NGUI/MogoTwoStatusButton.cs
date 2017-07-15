using UnityEngine;
using System.Collections;
using Mogo.Util;

public enum ButtonClickSoundType
{
    None=0,
    Weak=1,
    Middle,
    Strong,
    Open,
    Close
}

public class MogoTwoStatusButton : MonoBehaviour
{
    Transform m_transform;
    public GameObject m_bgDown;
    public GameObject m_bgUp;
    private string m_bgUpSpriteName = null;
    UILabel m_lblText;
    private bool m_clickable = true;
    public bool IsTab = false;
    public bool IsSmart = false;
    //public Vector3 Vec3Offest = new Vector3(0, 0, 0);
    //public Vector3 Vec3DownScale = new Vector3(0.95f, 0.95f, 1f);
    [HideInInspector]
    public Vector3 OrgScale { get; set; }
    [HideInInspector]
    public Vector3 PressScale { get; set; }
    /// <summary>
    /// 按钮点击音效类型
    /// 1：弱
    /// 2：中
    /// 3：强
    /// 4：打开
    /// 5：关闭
    /// </summary>
    public ButtonClickSoundType buttonType = ButtonClickSoundType.Weak;

    public bool Clickable
    {
        get { return m_clickable; }
        set
        {
            m_clickable = value;
            if (value)
            {
                if (string.IsNullOrEmpty(m_bgUpSpriteName))
                    m_bgUp.transform.GetComponentsInChildren<UISprite>(true)[0].spriteName = "btn_03up";
                else
                    m_bgUp.transform.GetComponentsInChildren<UISprite>(true)[0].spriteName = m_bgUpSpriteName;
            }
            else
            {
                m_bgUp.transform.GetComponentsInChildren<UISprite>(true)[0].spriteName = "btn_03hui";
            }
        }
    }

    void Awake()
    {
        m_transform = transform;
        var ssList = m_transform.GetComponentsInChildren<UISprite>(true);
        if (m_bgUp == null)
        {
            m_bgUp = ssList[0].gameObject;
        }
        if (m_bgUp != null)
        {
            m_bgUpSpriteName = m_bgUp.transform.GetComponentsInChildren<UISprite>(true)[0].spriteName;
        }
        if (m_bgDown == null)
        {
            m_bgDown = ssList[1].gameObject;
        }
        m_lblText = m_transform.GetComponentInChildren<UILabel>();
        if (IsSmart)
        {
            m_transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            BoxCollider bc = m_transform.GetComponent<BoxCollider>();
            bc.center = Vector3.zero;
            m_bgUp.transform.localScale = new Vector3(bc.size.x, bc.size.y, 1.0f);
            m_bgDown.transform.localScale = new Vector3(bc.size.x, bc.size.y, 1.0f);
        }
        if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Scale))
        {
            OrgScale = transform.localScale;
            PressScale = new Vector3(OrgScale.x * UIUtils.ButtonPressScale.x, OrgScale.y * UIUtils.ButtonPressScale.y, OrgScale.z * UIUtils.ButtonPressScale.z);
        }
    }

    void OnEnable()
    {
        //显示的时候设置按下状态为false，避免多点触碰造成的bug
        if (m_bgDown != null) m_bgDown.SetActive(false);
    }

    void OnPress(bool isPressed)
    {
        UIUtils.ButtonIsPressing = isPressed;
        if (Clickable)
        {
            if (isPressed)
            {
                m_bgDown.SetActive(true);
                //EventDispatcher.TriggerEvent(SettingEvent.UIDownPlaySound, gameObject.name);
                EventDispatcher.TriggerEvent("Click_Button", buttonType);
                if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Scale))
                    TweenScale.Begin(gameObject, 0.1f, PressScale);
                if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Offet))
                    transform.localPosition += UIUtils.ButtonPressOffset;
            }
            else
            {
                if (!IsTab)
                {
                    m_bgDown.SetActive(false);

                    if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Scale))
                        TweenScale.Begin(gameObject, 0.1f, OrgScale);
                    if (UIUtils.CurBtnPressType.HasType(ButtonPressType.Offet))
                        transform.localPosition -= UIUtils.ButtonPressOffset;
                    //EventDispatcher.TriggerEvent(SettingEvent.UIUpPlaySound, gameObject.name);
                }
            }
        }

    }

    public void SetButtonDown(bool down)
    {
        m_bgDown.SetActive(down);
    }
    public void SetButtonText(string text)
    {
        if (m_lblText)
            m_lblText.text = text;
    }


    public void SetButtonEnable(bool isEnable,  string disableSprite = "btn_03hui",string enableSprite = "btn_04up")
    {
        if (isEnable)
        {
            if (string.IsNullOrEmpty(m_bgUpSpriteName))
                m_bgUp.transform.GetComponentsInChildren<UISprite>(true)[0].spriteName = enableSprite;
            else
                m_bgUp.transform.GetComponentsInChildren<UISprite>(true)[0].spriteName = m_bgUpSpriteName;
            GetComponent<BoxCollider>().enabled = true;
        }
        else
        {
            m_bgUp.GetComponentsInChildren<UISprite>(true)[0].spriteName = disableSprite;
            GetComponent<BoxCollider>().enabled = false;
        }

    }
};
