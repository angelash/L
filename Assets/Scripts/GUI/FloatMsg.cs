using UnityEngine;
using System.Collections;
using Mogo.Util;
using System;

public class FloatMsg : MonoBehaviour
{
    UILabel m_lblText;
    public TweenAlpha m_tweenAlpha;
    public TweenPosition m_tweenPosition;

    Transform m_myTransform;
    GameObject m_myGameObject;
    private uint m_alphaTimerId;
    private float m_duration;

    public string text = string.Empty;
    public Action<FloatMsg> OnFinished;

    public void LoadResourceInsteadOfAwake()
    {
        m_myTransform = transform;
        m_myGameObject = gameObject;

        m_lblText = m_myTransform.GetComponentsInChildren<UILabel>(true)[0];
        m_tweenAlpha = m_myTransform.GetComponentsInChildren<TweenAlpha>(true)[0];
        m_tweenPosition = m_myTransform.GetComponentsInChildren<TweenPosition>(true)[0];

        m_tweenAlpha.enabled = false;
        m_tweenAlpha.Reset();

        m_tweenPosition.onFinished = TweenPositionFinished;
    }

    public void SetActive(bool isActive)
    {
        m_myGameObject.SetActive(isActive);
    }

    public void Show(string text, Vector3 pos, Color textColor, Color outlineColor, float duration = 1.5f)
    {
        TimerHeap.DelTimer(m_alphaTimerId);
        m_myTransform.localPosition = pos;
        m_lblText.text = text;
        m_lblText.color = textColor;
        m_lblText.effectColor = outlineColor;
        m_duration = duration;

        m_tweenPosition.enabled = true;
        m_tweenPosition.Reset();

        m_tweenPosition.Play(true);
    }

    private void TweenPositionFinished(UITweener t)
    {
        m_alphaTimerId = TimerHeap.AddTimer((uint)(m_duration * 1000), 0, OnFinished.SafeInvoke, this);
    }

    void OnDestroy()
    {
        TimerHeap.DelTimer(m_alphaTimerId);
    }
}
