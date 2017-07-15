using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIComponent : UIBase
{
    public string Id { get; set; }

    private Transform m_transParent;

    public Transform TransParent
    {
        get
        {
            if (m_transParent == null && Parent != null)
                return Parent.MyTransform;
            else
                return m_transParent;
        }
        set { m_transParent = value; }
    }

    private UIBase m_parent;

    public UIBase Parent
    {
        get { return m_parent; }
        set
        {
            m_parent = value;
            if (m_parent != null)
            {
                m_transParent = m_parent.MyTransform;
            }
        }
    }

    protected GameObject SyncCreateUIInstanceWithTransParent(string prefabName)
    {
        return SyncCreateUIInstanceWithRootTransform(prefabName, TransParent);
    }

    #region 对外使用UI控制方法

    public void Show()
    {
        ShowCom(null, null);
    }

    public void Show(Action callback)
    {
        ShowCom(null, callback);
    }

    public void Show(object[] param)
    {
        ShowCom(param, null);
    }

    public void Show(Action callback, object[] param)
    {
        ShowCom(param, callback);
    }

    public void Hide()
    {
        HideCom(null);
    }

    public void Hide(Action callback)
    {
        HideCom(callback);
    }

    public void Update()
    {
        UpdateCom(null, null);
    }

    public void Update(Action callback)
    {
        UpdateCom(null, callback);
    }

    public void Update(object[] param)
    {
        UpdateCom(param, null);
    }

    public void Update(Action callback, object[] param)
    {
        UpdateCom(param, callback);
    }

    #endregion

    #region UI生命周期

    internal sealed override void ReleaseResources()
    {
        if (m_parent != null)
            m_parent.RemoveComChild(this);
        base.ReleaseResources();
    }

    /// <summary>
    /// 在加载主界面之前要做的操作
    /// </summary>
    /// <param name="callback"></param>
    private void ActionBeforePanelLoad(Action callback)
    {
        OnBeforeResourceLoad();
        callback.SafeInvoke();
    }

    private void OnLoaded(Action callback)
    {
        OnResourceLoaded();
        callback.SafeInvoke();
    }

    private void OnShowWindow(object[] param, Action callback)
    {
        //将UI加入到管理器中
        OnShow(param, callback);
        callback.SafeInvoke();
    }

    private void OnUpdateWindow(object[] param, Action callback)
    {
        OnUpdate(param, callback);
        callback.SafeInvoke();
    }

    private void OnHideWindow(Action callback)
    {
        OnHide(callback);
        callback.SafeInvoke();
    }

    #endregion

    #region UI内部控制逻辑

    internal void ShowCom(object[] param, Action callback)
    {
        ActionBeforePanelLoad(() =>
        {
            m_bShow = true;
            PreloadResources(() =>
            {
                if (m_bShow == true)
                {
                    m_myGameObject.SafeSetActive(true);
                    OnShowWindow(param, callback);
                    OnUpdateWindow(null, null);
                }
            });
        });
    }

    internal void HideCom(Action callback = null)
    {
        if (m_bInited == true)
        {
            m_myGameObject.SafeSetActive(false);
            OnHideWindow(callback);
        }
        m_bShow = false;
    }

    internal void UpdateCom(object[] param, Action callback)
    {
        if (m_bShow && m_bInited)
        {
            OnUpdate(param, callback);
        }
    }

    internal void PreloadResources(Action callback = null)
    {
        if (m_bInited == false)
        {
            m_loadedCallback += callback;
            if (m_bLoading == false)
            {
                m_bLoading = true;
                AssetCacheMgr.GetUIResources(Resources, (objs) =>
                {
                    m_bInited = true;
                    OnLoaded(() =>
                    {
                        if (m_bShow == false)
                        {
                            m_myGameObject.SafeSetActive(false);
                        }
                        m_loadedCallback.SafeInvoke();
                    });
                });
            }
        }
        else
        {
            callback.SafeInvoke();
        }
    }

    #endregion
}