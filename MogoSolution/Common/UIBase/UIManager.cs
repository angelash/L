/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：游戏UI全局管理器
// 创建者：Zeng Dexuan
// 修改者列表：
// 创建日期：2015-1-26
// 模块描述：此类为UI公共类，用于UI模块的解耦，获取独立UI的UILogic，继而控制这个UI的show、hide、update操作。
//          思路源自杨振。
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Mogo.Util;
using Mogo.UI;
using UnityEngine;

public class UIManager
{
    private static UIManager m_instance;
    public static UIManager I { get { return m_instance; } }

    static UIManager()
    {
        m_instance = new UIManager();
    }

    #region Component

    #region 没有使用新框架

    /// <summary>
    /// 父亲标识、组件Id、组件对象
    /// </summary>
    Dictionary<Transform, Dictionary<string, UIComponent>> m_componentList = new Dictionary<Transform, Dictionary<string, UIComponent>>();

    /// <summary>
    /// 获取组件实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public T GetUICom<T>(Transform parent, String id = "") where T : UIComponent, new()
    {
        if (m_componentList.ContainsKey(parent) == false)
            m_componentList[parent] = new Dictionary<string, UIComponent>();
        if (String.IsNullOrEmpty(id))
            id = typeof(T).Name;
        if (m_componentList[parent].ContainsKey(id) == false)
            m_componentList[parent][id] = new T();

        m_componentList[parent][id].TransParent = parent;
        m_componentList[parent][id].Id = id;

        return m_componentList[parent][id] as T;
    }

    /// <summary>
    /// 显示组件内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="id"></param>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    public void ShowCom<T>(Transform parent, String id = "", object[] param = null, Action callback = null) where T : UIComponent, new()
    {
        showCom<T>(parent, id, param, callback);
    }

    /// <summary>
    /// 显示组件内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="id"></param>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    void showCom<T>(Transform parent, String id, object[] param, Action callback) where T : UIComponent, new()
    {
        var com = GetUICom<T>(parent, id);
        if (com != null)
        {
            com.ShowCom(param, callback);
        }
    }

    /// <summary>
    /// 刷新组件内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    public void UpdateCom<T>(Transform parent, String id = "", object[] param = null, Action callback = null) where T : UIComponent, new()
    {
        updateCom<T>(parent, id, param, callback);
    }

    /// <summary>
    /// 刷新组件内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    void updateCom<T>(Transform parent, String id, object[] param, Action callback) where T : UIComponent, new()
    {
        var parentName = parent.GetType().Name;
        if (m_componentList.ContainsKey(parent) == true)
        {
            if (String.IsNullOrEmpty(id))
                id = typeof(T).Name;
            if (m_componentList[parent].ContainsKey(id) == true)
            {
                m_componentList[parent][id].UpdateCom(param, callback);
            }
        }
    }

    /// <summary>
    /// 预加载组件资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callback"></param>
    public void PreloadCom<T>(Transform parent, String id = "", Action callback = null) where T : UIComponent, new()
    {
        var com = GetUICom<T>(parent, id);
        if (com != null)
        {
            com.PreloadResources(callback);
        }
    }

    /// <summary>
    /// 释放组件资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ReleaseCom<T>(Transform parent, String id) where T : UIComponent, new()
    {
        var parentName = parent.GetType().Name;
        if (m_componentList.ContainsKey(parent) == true)
        {
            if (String.IsNullOrEmpty(id))
                id = typeof(T).Name;
            if (m_componentList[parent].ContainsKey(id) == true)
            {
                m_componentList[parent][id].ReleaseResources();
                m_componentList[parent].Remove(id);
            }
        }
    }

    #endregion

    #region 使用新框架

    /// <summary>
    /// 获取组件实例
    /// </summary>
    /// <typeparam name="TCom"></typeparam>
    /// <param name="parent"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public TCom GetUICom<TCom, TParent>(String id = "")
        where TCom : UIComponent, new()
        where TParent : UILogic, new()
    {
        var parent = GetUILogic<TParent>();
        if (string.IsNullOrEmpty(id))
            id = typeof(TCom).Name;
        if (!parent.HasComChild(id))
        {
            var com = new TCom();
            com.Id = id;
            parent.AddComChild(com);
        }

        return parent.GetComChild(id) as TCom;
    }

    /// <summary>
    /// 显示组件内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="id"></param>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    public void ShowCom<TCom, TParent>(String id = "", object[] param = null, Action callback = null)
        where TCom : UIComponent, new()
        where TParent : UILogic, new()
    {
        showCom<TCom, TParent>(id, param, callback);
    }

    /// <summary>
    /// 显示组件内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="id"></param>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    void showCom<TCom, TParent>(String id, object[] param, Action callback)
        where TCom : UIComponent, new()
        where TParent : UILogic, new()
    {
        var com = GetUICom<TCom, TParent>(id);
        if (com != null)
        {
            com.ShowCom(param, callback);
        }
    }

    /// <summary>
    /// 刷新组件内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    public void UpdateCom<TCom, TParent>(String id = "", object[] param = null, Action callback = null)
        where TCom : UIComponent, new()
        where TParent : UILogic, new()
    {
        updateCom<TCom, TParent>(id, param, callback);
    }

    /// <summary>
    /// 刷新组件内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    void updateCom<TCom, TParent>(String id, object[] param, Action callback)
        where TCom : UIComponent, new()
        where TParent : UILogic, new()
    {
        var parent = GetUILogic<TParent>();
        if (string.IsNullOrEmpty(id))
            id = typeof(TCom).Name;
        if (parent.HasComChild(id))
        {
            parent.GetComChild(id).UpdateCom(param, callback);
        }
    }

    /// <summary>
    /// 预加载组件资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callback"></param>
    public void PreloadCom<TCom, TParent>(String id = "", Action callback = null)
        where TCom : UIComponent, new()
        where TParent : UILogic, new()
    {
        var parent = GetUILogic<TParent>();
        if (string.IsNullOrEmpty(id))
            id = typeof(TCom).Name;
        if (parent.HasComChild(id))
        {
            parent.GetComChild(id).PreloadResources(callback);
        }
    }

    /// <summary>
    /// 释放组件资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ReleaseCom<TCom, TParent>(String id)
        where TCom : UIComponent, new()
        where TParent : UILogic, new()
    {
        var parent = GetUILogic<TParent>();
        if (string.IsNullOrEmpty(id))
            id = typeof(TCom).Name;
        if (parent.HasComChild(id))
        {
            var com = parent.GetComChild(id);
            com.ReleaseResources();
            parent.RemoveComChild(com);
        }
    }

    #endregion

    #endregion

    #region UILogic

    /// <summary>
    /// UILogic单例缓存
    /// </summary>
    Dictionary<string, UILogic> m_uiList = new Dictionary<string, UILogic>();

    /// <summary>
    /// 获取UI Logic实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetUILogic<T>() where T : UILogic, new()
    {
        var uiName = typeof(T).Name;
        if (m_uiList.ContainsKey(uiName) == false)
            m_uiList[uiName] = new T();

        return m_uiList[uiName] as T;
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callback"></param>
    /// <param name="isShowWaitingTip"></param>
    public void ShowUI<T>(Action callback = null, bool isShowWaitingTip = true) where T : UILogic, new()
    {
        showUI<T>(null, callback, isShowWaitingTip);
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    public void ShowUI<T>(params object[] param) where T : UILogic, new()
    {
        showUI<T>(param, null, true);
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    /// <param name="isShowWaitingTip"></param>
    public void ShowUI<T>(System.Object param, Action callback, bool isShowWaitingTip = true) where T : UILogic, new()
    {
        showUI<T>(new object[] { param }, callback, isShowWaitingTip);
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    /// <param name="isShowWaitingTip"></param>
    public void ShowUI<T>(object[] param, Action callback, bool isShowWaitingTip = true) where T : UILogic, new()
    {
        showUI<T>(param, callback, isShowWaitingTip);
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    /// <param name="isShowWaitingTip"></param>
    void showUI<T>(object[] param, Action callback, bool isShowWaitingTip) where T : UILogic, new()
    {
        var ui = GetUILogic<T>();
        if (ui != null)
        {
            PopupManager.ShowUI(ui, callback, param, isShowWaitingTip);
        }
    }

    /// <summary>
    /// 关闭UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callback"></param>
    public void CloseUI<T>(Action callback = null) where T : UILogic, new()
    {
        var uiName = typeof(T).Name;
        if (m_uiList.ContainsKey(uiName) == true)
            PopupManager.CloseUI(m_uiList[uiName], callback);
    }

    /// <summary>
    /// 刷新UI内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callback"></param>
    public void UpdateUI<T>(Action callback = null) where T : UILogic, new()
    {
        updateUI<T>(null, callback);
    }

    /// <summary>
    /// 刷新UI内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    public void UpdateUI<T>(params object[] param) where T : UILogic, new()
    {
        updateUI<T>(param, null);
    }

    /// <summary>
    /// 刷新UI内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    public void UpdateUI<T>(System.Object param, Action callback) where T : UILogic, new()
    {
        updateUI<T>(new object[] { param }, callback);
    }

    /// <summary>
    /// 刷新UI内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    /// <param name="callback"></param>
    void updateUI<T>(object[] param, Action callback) where T : UILogic, new()
    {
        var uiName = typeof(T).Name;
        if (m_uiList.ContainsKey(uiName) == true)
            m_uiList[uiName].UpdateWindow(param, callback);
    }

    /// <summary>
    /// 预加载UI资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callback"></param>
    public void PreloadUI<T>(Action callback = null) where T : UILogic, new()
    {
        var ui = GetUILogic<T>();
        if (ui != null)
        {
            ui.PreloadResources(callback);
        }
    }

    /// <summary>
    /// 释放UI资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void ReleaseUI<T>() where T : UILogic, new()
    {
        var uiName = typeof(T).Name;
        if (m_uiList.ContainsKey(uiName) == true)
        {
            m_uiList[uiName].ReleaseResources();
            m_uiList.Remove(uiName);
        }
    }

    /// <summary>
    /// 释放UI资源
    /// </summary>
    /// <param name="uiName"></param>
    public void ReleaseUI(string uiName)
    {
        if (m_uiList.ContainsKey(uiName) == true)
        {
            m_uiList[uiName].ReleaseResources();
            m_uiList.Remove(uiName);
        }
    }

    #endregion
}