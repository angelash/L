using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mogo.Util;

/// <summary>
/// 使用方法：
/// 数据配置：
///     GameObject 挂上UIToggle
///     配置按下和弹起的 文字和图片
///     配置组
///     配置是否默认按下
/// 
/// 脚本中：
///     注册监听onChange事件
///     通过value接口获取当前Tab状态
///  
/// </summary>
public class UIToggle : MonoBehaviour
{
    /// <summary>
    /// 场景中所有活跃的Tab
    /// </summary>
    static List<UIToggle> list = new List<UIToggle>();
    /// <summary>
    /// Tab 所在分组
    /// </summary>
    public int group = 0;
    public GameObject activeSprite;
    public GameObject disableSprite;
    public GameObject activeLabel;
    public GameObject disableLabel;
    /// <summary>
    /// 按钮点击音效类型
    /// 1：弱
    /// 2：中
    /// 3：强
    /// 4：打开
    /// 5：关闭
    /// </summary>
    public ButtonClickSoundType buttonType = ButtonClickSoundType.Weak;

    /// <summary>
    /// 是否是默认点击
    /// </summary>
    public bool startsActive = false;
    private bool m_bIsDraging;

    /// <summary>
    /// 状态变化时候的回调
    /// </summary>
    public System.Action onChange;

    bool mIsActive = true;
    bool mStarted = false;

    /// <summary>
    /// 获得或者设置当前Toggle状态
    /// </summary>
    public bool value
    {
        get
        {
            return mStarted ? mIsActive : startsActive;
        }
        set
        {
            if (!mStarted)
            {
                startsActive = value;
            }
            else
            {
                //默认组0 或者设置为Active
                if (group == 0 || value)
                {
                    Set(value);
                }
                /*
                else
                {
                    SimpleSet(value);
                }
                */
            }
        }
    }

    /// <summary>
    /// 获得某组中的 激活的toggle
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    static public UIToggle GetActiveToggle(int group)
    {
        for (int i = 0; i < list.Count; i++)
        {
            UIToggle toggle = list[i];
            if (toggle != null && toggle.group == group && toggle.mIsActive)
            {
                return toggle;
            }
        }
        return null;
    }
    void OnEnable()
    {
        list.Add(this);
    }

    void OnDisable()
    {
        list.Remove(this);
    }

    // Use this for initialization
    void Start()
    {
        mIsActive = !startsActive;
        mStarted = true;
        Set(startsActive);
    }

    void OnDrag(Vector2 delta)
    {
        m_bIsDraging = true;
    }

    void OnPress(bool isPressed)
    {
        UIUtils.ButtonIsPressing = isPressed;
        if (isPressed)
        {
            if (UIUtils.CurTogglePressType.HasType(ButtonPressType.Offet))
                transform.localPosition += UIUtils.TogglePressOffset;

            //EventDispatcher.TriggerEvent(SettingEvent.UIDownPlaySound, gameObject.name);
            EventDispatcher.TriggerEvent("Click_Button", buttonType);
        }
        else
        {
            if (UIUtils.CurTogglePressType.HasType(ButtonPressType.Offet))
                transform.localPosition -= UIUtils.TogglePressOffset;

            //EventDispatcher.TriggerEvent(SettingEvent.UIUpPlaySound, gameObject.name);

            if (!m_bIsDraging)
            {
                if (enabled)
                {
                    value = !value;
                }
            }
            m_bIsDraging = false;
        }
    }

    void SimpleSet(bool state)
    {
        mIsActive = state;
        if (activeSprite != null)
        {
            activeSprite.SetActive(state);
        }
        if (activeLabel != null)
        {
            activeLabel.SetActive(state);
        }
        if (disableSprite != null)
        {
            disableSprite.SetActive(!state);
        }
        if (disableLabel != null)
        {
            disableLabel.SetActive(!state);
        }

        if (onChange != null)
        {
            onChange();
        }
    }
    void Set(bool state)
    {
        if (!mStarted)
        {
            mIsActive = state;
            startsActive = state;
            if (activeSprite != null)
            {
                activeSprite.SetActive(state);
            }
            if (activeLabel != null)
            {
                activeLabel.SetActive(state);
            }
            if (disableSprite != null)
            {
                disableSprite.SetActive(!state);
            }
            if (disableLabel != null)
            {
                disableLabel.SetActive(!state);
            }
        }
        else if (mIsActive != state)
        {
            if (group != 0 && state)
            {
                for (int i = 0, imax = list.Count; i < imax; )
                {
                    UIToggle cb = list[i];
                    if (cb != this && cb.group == group)
                    {
                        cb.Set(false);
                    }
                    //Set(false) callback Remove Item 
                    if (list.Count != imax)
                    {
                        imax = list.Count;
                        i = 0;
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
            mIsActive = state;
            if (activeSprite != null)
            {
                activeSprite.SetActive(state);
            }
            if (activeLabel != null)
            {
                activeLabel.SetActive(state);
            }
            if (disableSprite != null)
            {
                disableSprite.SetActive(!state);
            }
            if (disableLabel != null)
            {
                disableLabel.SetActive(!state);
            }
            if (onChange != null)
            {
                onChange();
            }
        }
    }
}
