using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class UIBase
{
    /// <summary>
    /// 本UI所使用的所有UI资源，填上去了会在打开前全部加载，要用的时候直接可直接实例对象
    /// </summary>
    protected virtual string[] Resources
    {
        get { return null; }
    }

    public GameObject MyGameObject
    {
        get { return m_myGameObject; }
    }

    public Transform MyTransform
    {
        get { return m_myTransform; }
    }

    /// <summary>
    /// 是否打开显示
    /// </summary>
    public bool bShow { get { return m_bShow; } }

    protected GameObject m_myGameObject;
    protected Transform m_myTransform;
    protected bool m_bShow = false;
    protected bool m_bInited = false;
    protected bool m_bLoading = false;
    protected Action m_loadedCallback;

    protected bool m_inited;

    protected Dictionary<string, Transform> m_name2transform;
    protected Dictionary<string, MogoUIBtn> m_name2MogoUIBtn;

    private Dictionary<string, UIComponent> m_comChildren = new Dictionary<string, UIComponent>();

    internal void AddComChild(UIComponent com)
    {
        if (m_comChildren.ContainsKey(com.Id))
        {
            LoggerHelper.Error("Same id in m_comChildren: " + com.Id + " " + MyTransform);
            return;
        }
        com.Parent = this;
        m_comChildren.Add(com.Id, com);
    }

    internal void RemoveComChild(UIComponent com)
    {
        if (!m_comChildren.ContainsKey(com.Id))
        {
            LoggerHelper.Error("Id not in m_comChildren: " + com.Id + " " + MyTransform);
            return;
        }
        com.Parent = null;
        m_comChildren.Remove(com.Id);
    }

    internal bool HasComChild(string id)
    {
        return m_comChildren.ContainsKey(id);
    }

    internal UIComponent GetComChild(string id)
    {
        if (!m_comChildren.ContainsKey(id))
        {
            LoggerHelper.Error("Id not in m_comChildren: " + id + " " + MyTransform);
            return null;
        }
        return m_comChildren[id];
    }

    #region 查找组件

    private void FillNameData(Transform rootTransform)
    {
        for (int i = 0; i < rootTransform.childCount; i++)
        {
            //var sb = new StringBuilder();
            Transform childTransform = rootTransform.GetChild(i);
            var name = childTransform.name;
            if (!m_name2transform.ContainsKey(name))
            {
                m_name2transform.Add(name, childTransform);
                //LoggerHelper.Error(string.Concat(rootTransform.name, " contains ", childTransform.name, " repeatly."));
            }
            //sb.AppendFormat("{0}/{1}", parentName, name);
            //var fullName = sb.ToString();
            //if (!m_fullName2transform.ContainsKey(fullName))
            //{
            //    m_fullName2transform.Add(fullName, childTransform);
            //    //LoggerHelper.Error(string.Concat(rootTransform.name, " contains ", childTransform.name, " repeatly."));
            //}

            FillNameData(childTransform);
        }
    }

    protected Transform FindTransform(string transformName)
    {
        if (m_name2transform.ContainsKey(transformName) == true)
        {
            return m_name2transform[transformName];
        }
        else
        {
            LoggerHelper.Error(string.Concat("cannot find ", transformName));
            return null;
        }
    }

    protected T FindComponent<T>(string name) where T : Component
    {
        Transform tf = FindTransform(name);
        if (tf == null)
            return null;
        return tf.GetComponent<T>();
    }

    protected T FindComponentInChildren<T>(string name) where T : Component
    {
        Transform tf = FindTransform(name);
        if (tf == null)
            return null;
        var children = tf.GetComponentsInChildren<T>(true);
        if (children.Length == 0)
            return null;
        return children[0];
    }

    #endregion

    #region UI生命周期

    internal virtual void Initialize(Transform transform)
    {
        if (m_inited == true)
            return;

        m_inited = true;
        m_name2transform = new Dictionary<string, Transform>();
        //m_fullName2transform = new Dictionary<string, Transform>();
        //m_name2component = new Dictionary<Type, Dictionary<string, Component>>();
        m_myTransform = transform;
        FillNameData(m_myTransform);
    }

    internal virtual void ReleaseResources()
    {
        foreach (var item in m_comChildren)
        {
            item.Value.ReleaseResources();
        }
        m_comChildren.Clear();

        if (m_inited)
        {
            m_inited = false;
            OnRelease();
            ClearComponent();
            m_myTransform = null;
        }

        if (m_bInited == true)
        {
            AssetCacheMgr.SynReleaseInstance(m_myGameObject);
            m_myGameObject = null;
            AssetCacheMgr.ReleasesResource(Resources);
            m_bInited = false;
            m_bShow = false;
            m_bLoading = false;
            m_loadedCallback = null;
        }
    }

    protected virtual void OnBeforeResourceLoad() { }
    protected virtual void OnResourceLoaded() { }
    protected virtual void OnShow(object[] param, Action callback) { }
    protected virtual void OnHide(Action callback) { }
    protected virtual void OnRelease() { }
    protected virtual void OnUpdate(object[] param, Action callback) { }

    private void ClearComponent()
    {
        if (m_name2MogoUIBtn != null)
            m_name2MogoUIBtn.Clear();
        m_name2transform.Clear();
    }

    #endregion

    #region UI组件访问方法

    protected GameObject SyncCreateUIInstanceWithRootTransform(string prefabName, Transform parentTransform = null)
    {
        GameObject temp = AssetCacheMgr.SynGetInstance(prefabName) as GameObject;
        m_myGameObject = temp;
        var tran = temp.transform;
        Initialize(tran);
        tran.parent = parentTransform;
        tran.localPosition = Vector3.zero;
        tran.localScale = Vector3.one;
        temp.name = prefabName;// Mogo.Util.Utils.GetFilePathWithoutExtention(prefabName);//去文件后缀比较高效的方法

        return temp;
    }

    protected GameObject SyncCreateNewGameObjectWithRootTransform(string prefabName, Transform parentTransform = null)
    {
        GameObject temp = new GameObject();
        m_myGameObject = temp;
        var tran = temp.transform;
        Initialize(temp.transform);
        temp.name = prefabName;
        tran.parent = parentTransform;
        tran.localPosition = Vector3.zero;
        tran.localScale = Vector3.one;
        return temp;
    }

    protected void SetActive(string name, bool value)
    {
        FindTransform(name).SafeSetActive(value);
    }

    protected void SetImg(string textureName, string imgName)
    {
        AssetCacheMgr.GetUIResource(imgName, (obj) =>
        {
            var texture = FindComponent<UITexture>(textureName);
            if (texture)
                texture.mainTexture = (Texture)obj;
        });
    }

    protected void RemoveImg(string textureName)
    {
        var texture = FindComponent<UITexture>(textureName);
        if (texture)
            texture.mainTexture = null;
    }

    protected void SetAtlasSpriteImage(string spriteName, string imgName)
    {
        var sprite = FindComponent<UISprite>(spriteName);
        if (sprite)
        {
            //sprite.atlas = MogoUIManagerEx.Instance.GetAtlasByIconName(imgName);
            sprite.spriteName = imgName;
        }
    }

    protected void SetSpriteImage(string spriteName, string imgName)
    {
        var sprite = FindComponent<UISprite>(spriteName);
        if (sprite)
        {
            sprite.spriteName = imgName;
        }
    }

    protected void SetLabelText(string labelName, string content)
    {
        var label = FindComponent<UILabel>(labelName);
        if (label)
        {
            label.text = content;
        }
    }

    protected void SetLabelText(string labelName, params object[] content)
    {
        var label = FindComponent<UILabel>(labelName);
        if (label && content != null)
        {
            label.text = string.Format("", content);
        }
    }

    protected void SetLabelText(string labelName, int id)
    {
        var label = FindComponent<UILabel>(labelName);
        if (label)
        {
            label.text = LanguageData.GetContent(id);
        }
    }

    protected void SetLabelText(string labelName, int id, params object[] args)
    {
        var label = FindComponent<UILabel>(labelName);
        if (label)
        {
            label.text = LanguageData.GetContent(id, args);
        }
    }

    /// <summary>
    /// 赋值按钮点击事件
    /// 此方法的事件直接赋值，销毁UI的时候会自动释放，不用退订
    /// </summary>
    /// <param name="btnName"></param>
    /// <param name="clickHandler"></param>
    protected void SetClickHandler(string btnName, Action clickHandler)
    {
        if (m_name2MogoUIBtn != null && m_name2MogoUIBtn.ContainsKey(btnName) && m_name2MogoUIBtn[btnName])
        {
            m_name2MogoUIBtn[btnName].ClickEvent = clickHandler;
            return;
        }

        var btn = FindComponent<MogoUIBtn>(btnName);
        if (btn)
        {
            btn.ClickEvent = clickHandler;
            if (m_name2MogoUIBtn == null)
                m_name2MogoUIBtn = new Dictionary<string, MogoUIBtn>();
            m_name2MogoUIBtn[btnName] = btn;
        }
    }

    /// <summary>
    /// 订阅按钮点击事件
    /// </summary>
    /// <param name="btnName"></param>
    /// <param name="clickHandler"></param>
    protected void AddClickHandler(string btnName, Action clickHandler)
    {
        var btn = FindComponent<MogoUIBtn>(btnName);
        if (btn)
            btn.ClickEvent += clickHandler;
    }

    /// <summary>
    /// 退订按钮点击事件
    /// </summary>
    /// <param name="btnName"></param>
    /// <param name="clickHandler"></param>
    protected void RemoveClickHandler(string btnName, Action clickHandler)
    {
        var btn = FindComponent<MogoUIBtn>(btnName);
        if (btn)
            btn.ClickEvent -= clickHandler;
    }

    #endregion
}