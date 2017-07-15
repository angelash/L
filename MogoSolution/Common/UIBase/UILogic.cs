/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：游戏UI控制器
// 创建者：Zeng Dexuan
// 修改者列表：
// 创建日期：2015-1-26
// 模块描述：由于进入游戏后，很多UI都不是一开始就加载好了，这些UI并不能直接访问到，所以需要UILogic来控制UI的加载、显示和关闭。
//          定义UILogic的操作有5种，show、hide、update和preload、release。其中update可用事件或自定义类代替。
//          定义UILogic的状态有6种，0-未创建，1-已预加载，2-打开显示，3-后台存在但未显示，4-已关闭（状态同1）,5-已卸载。
//          思路源自杨振。
//----------------------------------------------------------------*/

using Mogo.GameData;
using Mogo.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class UILogic : UIBase
{
    /// <summary>
    /// UI在对应类型场景中才能打开，否则会推入UI队列，直到玩家跳转到对应场景，该UI才会被打开
    /// </summary>
    public virtual MapType UIMapType
    {
        get { return MapType.Normal; }
    }

    /// <summary>
    /// 当前UI打开时，UI摄像机的CameraClearFlags应该调整成这个，通常有场景的UI都要为Depth;
    /// </summary>
    public virtual CameraClearFlags CameraClearFlag
    {
        get { return CameraClearFlags.Depth; }
    }

    /// <summary>
    /// 当前UI需要的camera，不需要的camera会关闭
    /// </summary>
    public virtual CameraType UsingCameras
    {
        get
        {
            return CameraType.MogoUICamera;
        }
    }

    /// <summary>
    /// UILogic的属性
    /// </summary>
    public virtual UIProperties Properties
    {
        get
        {
            return UIProperties.MinimizeByOther | UIProperties.DontRelease;
        }
    }

    protected virtual string CloseButtonName
    {
        get { return "BtnClose"; }
    }

    public bool needMogoFXCamera = true;
    public bool needMogoMainCamera = true;
    public bool needMogoMainScene = false;
    public bool closeWhenback = false;          //因打开其他界面或跳转场景而隐藏界面后，是否可以在无界面显示时再自动打开。

    #region UILogic父子关系

    List<UILogic> m_childList = new List<UILogic>();

    UILogic m_parent;

    public UILogic Parent
    {
        set
        {
            if (m_parent != null)
                m_parent.RemoveChild(this);

            m_parent = value;

            if (m_parent != null)
                m_parent.AddChild(this);
        }
        get
        {
            return m_parent;
        }
    }

    internal void AddChild(UILogic child)
    {
        if (m_childList.Contains(child) == false)
            m_childList.Add(child);
    }

    internal void RemoveChild(UILogic child)
    {
        if (m_childList.Contains(child) == true)
            m_childList.Remove(child);
    }

    #endregion

    #region UI生命周期

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
        //if (!Properties.HasProperty(UIProperties.DontUseMogoUIManager))
        //    MogoUIManagerEx.Instance.PushView(m_myGameObject);
        callback.SafeInvoke();
        //当前面板显示完成后，对所有的面板检测一下，用于释放资源
        PopupManager.CheckRelease(this);
    }

    private void OnUpdateWindow(object[] param, Action callback)
    {
        OnUpdate(param, callback);
        callback.SafeInvoke();
    }

    private void OnHideWindow(Action callback)
    {
        OnHide(callback);
        //清空UI显示
        //if (!Properties.HasProperty(UIProperties.DontUseMogoUIManager))
        //    MogoUIManagerEx.Instance.PopAllUI();
        callback.SafeInvoke();
    }

    #endregion

    #region 对外使用UI控制方法

    public void Show()
    {
        PopupManager.ShowUI(this, null, null);
    }

    public void Show(Action callback)
    {
        PopupManager.ShowUI(this, callback, null);
    }

    public void Show(object[] param)
    {
        PopupManager.ShowUI(this, null, param);
    }

    public void Show(Action callback, object[] param)
    {
        PopupManager.ShowUI(this, callback, param);
    }

    public void Close()
    {
        PopupManager.CloseUI(this, null);
    }

    public void Close(Action callback)
    {
        PopupManager.CloseUI(this, callback);
    }

    public void Update()
    {
        UpdateWindow(null, null);
    }

    public void Update(Action callback)
    {
        UpdateWindow(null, callback);
    }

    public void Update(object[] param)
    {
        UpdateWindow(param, null);
    }

    public void Update(Action callback, object[] param)
    {
        UpdateWindow(param, callback);
    }

    #endregion

    #region UI内部控制逻辑

    internal sealed override void Initialize(Transform transform)
    {
        base.Initialize(transform);

        if (m_name2transform.ContainsKey(CloseButtonName))
            SetClickHandler(CloseButtonName, Close);
    }

    internal sealed override void ReleaseResources()
    {
        if (m_bInited == true)
        {
            if (m_name2transform.ContainsKey(CloseButtonName))
                RemoveClickHandler(CloseButtonName, Close);
        }
        base.ReleaseResources();
    }

    internal void ShowWindow(object[] param, Action callback)
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

    internal void HideWindow(Action callback = null)
    {
        if (m_bInited == true)
        {
            m_myGameObject.SafeSetActive(false);
            OnHideWindow(callback);
        }
        m_bShow = false;
    }

    internal void UpdateWindow(object[] param, Action callback)
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