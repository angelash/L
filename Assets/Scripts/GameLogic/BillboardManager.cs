using System;
using UnityEngine;
using System.Collections.Generic;
using Mogo.Game;
using Mogo.Util;
using Mogo.GameData;

public class BillboardManager
{
    private static BillboardManager m_instance;
    public static BillboardManager I { get { return m_instance; } }

    public Camera RelatedCamera;
    public Camera ViewCamera;

    private Dictionary<uint, BillboardInfoUIView> m_billBoardDic;
    private Transform m_floatTextTrans;

    public Transform FloatTextTrans
    {
        get { return m_floatTextTrans; }
        set
        {
            m_floatTextTrans = value;
            m_floatTextPool.ParentTrans = value;
            m_bulletUIPool.ParentTrans = value;
        }
    }

    private MogoComponentPool<FloatMsg> m_floatTextPool;
    private MogoComponentPool<BulletUI> m_bulletUIPool;

    #region 框架

    public BillboardManager()
    {
        m_instance = this;
        m_billBoardDic = new Dictionary<uint, BillboardInfoUIView>();
        m_floatTextPool = new MogoComponentPool<FloatMsg>();
        m_floatTextPool.ResourceName = "MsgBoxFbFloat.prefab";
        m_floatTextPool.DoGetCacheObject = (floatMsg) =>
        {
            floatMsg.LoadResourceInsteadOfAwake();
            floatMsg.SetActive(false);
            floatMsg.OnFinished = m_floatTextPool.ReleaseInstance;
        };

        m_bulletUIPool = new MogoComponentPool<BulletUI>();
        m_bulletUIPool.ResourceName = "BulletUI.prefab";
        m_bulletUIPool.DoGetCacheObject = (bulletUI) =>
        {
            bulletUI.Init();
            bulletUI.OnFinished = m_bulletUIPool.ReleaseInstance;
        };

        AddListeners();
    }

    public void Release()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
    }

    public void RemoveListeners()
    {
    }

    #endregion

    #region BulletUI

    public void Attack(Vector3 from, Vector3 to, int type, float duration = 0.1f)
    {
        m_bulletUIPool.GetCacheObject((bullet) =>
        {
            bullet.Attack(from, to, type, duration);
        });
    }

    #endregion

    #region FloatText

    public void FloatText(string text, Vector3 pos, int colorId, float duration = 1.5f)
    {
        if (ColorData.dataMap.ContainsKey(colorId))
        {
            FloatText(text, pos, ColorData.dataMap[colorId].FontColor, ColorData.dataMap[colorId].OutlineColor, duration);
        }
        else
        {
            FloatText(text, pos, duration);
        }
    }

    public void FloatText(string text, Vector3 pos, float duration = 1.5f)
    {
        FloatText(text, pos, new Color(248, 226, 193), duration);
    }

    public void FloatText(string text, Vector3 pos, Color textColor, float duration = 1.5f)
    {
        FloatText(text, pos, textColor, Color.black, duration);
    }

    public void FloatText(string text, Vector3 pos, Color textColor, Color outlineColor, float duration = 1.5f)
    {
        m_floatTextPool.GetCacheObject((floatMsg) => { floatMsg.Show(text, pos, textColor, outlineColor, duration); });
    }

    #endregion

    #region billboard

    public void AddBillBoard(uint pid, Transform trans, EntityParent self, string name = "")
    {
        //LoggerHelper.Debug("AddBillBoard: " + pid + " trans " + trans);

        if (m_billBoardDic.ContainsKey(pid))
        {
            return;
        }
        else
        {
            var go = AssetCacheMgr.SynGetInstance("BillBoardInfoUI.prefab");
            var item = go as GameObject;
            item.name = pid.ToString();
            item.transform.parent = MogoWorld.m_uiManager.billboardList;
            item.transform.localPosition = Vector3.one;
            item.transform.localScale = Vector3.one;
            BillboardInfoUIView view = item.AddComponent<BillboardInfoUIView>();
            view.SetPlayerId(pid);
            view.SetName(name);
            m_billBoardDic.Add(pid, view);
        }
    }

    /// <summary>
    /// 移除Billboard
    /// </summary>
    /// <param name="playerid"></param>
    public void RemoveBillboard(uint pid)
    {
        if (!m_billBoardDic.ContainsKey(pid))
        {
            return;
        }
        m_billBoardDic[pid].Release();
        m_billBoardDic.Remove(pid);
    }

    /// <summary>
    /// 统一刷新billboard
    /// </summary>
    public void UpdateBillboard()
    {
    }

    #endregion
}

public class MogoComponentPool<T> where T : Component
{
    private List<T> m_noUse = new List<T>();
    private List<T> m_inUse = new List<T>();

    public Action<T> DoGetCacheObject;
    public string ResourceName;
    public Transform ParentTrans;


    public void GetCacheObject(Action<T> callback)
    {
        if (m_noUse.Count != 0)
        {
            var index = m_noUse.Count - 1;
            var obj = m_noUse[index];
            m_noUse.RemoveAt(index);

            SetToInUse(obj, callback);
        }
        else
        {
            AssetCacheMgr.GetUIInstance(ResourceName, (prefab, guid, o) =>
            {
                var obj = o as GameObject;
                var tran = obj.transform;
                obj.name = prefab;
                obj.SetActive(false);
                tran.parent = ParentTrans;
                InitGameObject(obj);
                var com = obj.AddComponent<T>();
                if (DoGetCacheObject != null)
                    DoGetCacheObject(com);
                SetToInUse(com, callback);
            });
        }
    }

    public void ReleaseInstance(T obj)
    {
        if (m_inUse.Count == 0)
        {
            return;
        }
        if (obj)
        {
            m_inUse.Remove(obj);
            SetToNoUse(obj);
        }
    }

    /// <summary>
    /// 初始化GameObject的位置 缩放和旋转
    /// </summary>
    /// <param name="g"></param>
    public static void InitGameObject(GameObject g)
    {
        g.transform.localPosition = Vector3.zero;
        g.transform.localScale = Vector3.one;
        g.transform.localRotation = Quaternion.identity;
    }

    public void Clear()
    {
        ClearCache(m_inUse);
        ClearCache(m_noUse);
    }

    private void ClearCache(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var obj = list[i].gameObject;
            AssetCacheMgr.SynReleaseInstance(obj);
            list[i] = null;
        }
        list.Clear();
    }

    private void SetToInUse(T obj, Action<T> callback)
    {
        m_inUse.Add(obj);

        if (callback != null)
            callback(obj);
        obj.gameObject.SetActive(true);
    }

    private void SetToNoUse(T obj)
    {
        m_noUse.Add(obj);
        obj.gameObject.SetActive(false);
    }
}