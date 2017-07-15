using Mogo.Game;
using System;
using System.Collections;
using Mogo.FSM;
using Mogo.GameData;
using UnityEngine;
using Mogo.Util;

public class ActorParent : MonoBehaviour
{
    private EntityParent m_entity;

    public EntityParent Entity
    {
        get { return m_entity; }
        set { m_entity = value; }
    }

    bool needDelaySetAction = false;
    /// <summary>
    /// 延迟多少帧处理Action设置，默认3
    /// </summary>
    int frameFlag = 3;
    /// <summary>
    /// 帧数计数器
    /// </summary>
    int frameCounter;
    /// <summary>
    /// 目标设置的动作值
    /// </summary>
    int targetAction;

    void Update()
    {
    }

    void FixedUpdate()
    {
        if (!needDelaySetAction)
            return;
        if (frameCounter < frameFlag)
        {
            frameCounter += 1;
            return;
        }
    }


    #region 包装基于帧的回调函数

    // 基于帧的回调函数。 用于处理必须在异帧 完成的事情。
    public void AddCallbackInFrames(Action callback, int inFrames = 3)
    {
        if (this && this.gameObject.activeSelf)
            StartCoroutine(CallBackInFrames(callback, inFrames));
    }

    public void AddCallbackInFrames<U>(Action<U> callback, U arg1, int inFrames = 3)
    {
        if (this && this.gameObject.activeSelf)
            StartCoroutine(CallBackInFrames(callback, arg1, inFrames));
    }

    public void AddCallbackInFrames<U, V>(Action<U, V> callback, U arg1, V arg2, int inFrames = 3)
    {
        if (this && this.gameObject.activeSelf)
            StartCoroutine(CallBackInFrames(callback, arg1, arg2, inFrames));
    }

    public void AddCallbackInFrames<U, V, T>(Action<U, V, T> callback, U arg1, V arg2, T arg3, int inFrames = 3)
    {
        if (this && this.gameObject.activeSelf)
            StartCoroutine(CallBackInFrames(callback, arg1, arg2, arg3, inFrames));
    }

    public void AddCallbackInFrames<U, V, T, W>(Action<U, V, T, W> callback, U arg1, V arg2, T arg3, W arg4, int inFrames = 3)
    {
        if (this && this.gameObject.activeSelf)
            StartCoroutine(CallBackInFrames(callback, arg1, arg2, arg3, arg4, inFrames));
    }

    IEnumerator CallBackInFrames(Action callback, int inFrames)
    {
        int n = 0;
        while (n < inFrames)
        {
            yield return new WaitForFixedUpdate();
            n += 1;
        }
        callback();
    }

    IEnumerator CallBackInFrames<U>(Action<U> callback, U arg1, int inFrames)
    {
        int n = 0;
        while (n < inFrames)
        {
            yield return new WaitForFixedUpdate();
            n += 1;
        }
        callback(arg1);
    }

    IEnumerator CallBackInFrames<U, V>(Action<U, V> callback, U arg1, V arg2, int inFrames)
    {
        int n = 0;
        while (n < inFrames)
        {
            yield return new WaitForFixedUpdate();
            n += 1;
        }
        callback(arg1, arg2);
    }

    IEnumerator CallBackInFrames<U, V, T>(Action<U, V, T> callback, U arg1, V arg2, T arg3, int inFrames)
    {
        int n = 0;
        while (n < inFrames)
        {
            yield return new WaitForFixedUpdate();
            n += 1;
        }
        callback(arg1, arg2, arg3);
    }

    IEnumerator CallBackInFrames<U, V, T, W>(Action<U, V, T, W> callback, U arg1, V arg2, T arg3, W arg4, int inFrames)
    {
        int n = 0;
        while (n < inFrames)
        {
            yield return new WaitForFixedUpdate();
            n += 1;
        }
        callback(arg1, arg2, arg3, arg4);
    }

    #endregion

}