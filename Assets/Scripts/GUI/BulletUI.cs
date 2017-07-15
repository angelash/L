using UnityEngine;
using System.Collections;
using Mogo.Util;
using System;

public class BulletUI : MonoBehaviour
{
    private GameObject m_jinzhanatk;
    private GameObject m_stjinzhanatk;
    private GameObject m_yuanchengatk;
    private GameObject m_styuanchengatk;
    private GameObject m_toweratk;
    private GameObject m_gongchengatk;
    private GameObject m_anshaatk;
    private GameObject m_hit;

    private Transform m_myTrans;

    private TweenAlpha m_tweenAlpha;
    private TweenPosition m_tweenPosition;
    //private uint m_alphaTimerId;
    private float m_duration;
    public Action<BulletUI> OnFinished;

    private TweenScale m_hitScale;
    private TweenAlpha m_hitAlpha;
    private TweenRotation m_hitRot;

    public void Init()
    {
        m_myTrans = transform;
        m_tweenAlpha = m_myTrans.GetComponent<TweenAlpha>();
        m_tweenAlpha.onFinished = TweenAlphaFinished;
        m_tweenPosition = m_myTrans.GetComponent<TweenPosition>();
        m_tweenPosition.onFinished = TweenPositionFinished;

        m_jinzhanatk = m_myTrans.Find("jinzhanatk").gameObject;
        m_stjinzhanatk = m_myTrans.Find("stjinzhanatk").gameObject;
        m_yuanchengatk = m_myTrans.Find("yuanchengatk").gameObject;
        m_styuanchengatk = m_myTrans.Find("styuanchengatk").gameObject;
        m_toweratk = m_myTrans.Find("toweratk").gameObject;
        m_gongchengatk = m_myTrans.Find("gongchengatk").gameObject;
        m_anshaatk = m_myTrans.Find("anshaatk").gameObject;
        m_hit = m_myTrans.Find("hit").gameObject;

        m_hitScale = m_hit.GetComponent<TweenScale>();
        m_hitAlpha = m_hit.GetComponent<TweenAlpha>();
        m_hitRot = m_hit.GetComponent<TweenRotation>();
    }

    public void Attack(Vector3 from, Vector3 to, int type, float duration = 1.5f)
    {
        SetType(type);
        //TimerHeap.DelTimer(m_alphaTimerId);
        //m_myTrans.localPosition = from;
        m_duration = duration;

        transform.rotation = Quaternion.FromToRotation(new Vector3(to.x - from.x, from.y - to.y, from.z - to.z), Vector3.up);
        //var angel = Vector3.Angle(Vector3.zero, to - from);

        m_tweenPosition.Reset();
        m_tweenPosition.from = from;
        m_tweenPosition.to = to;
        m_tweenPosition.duration = duration;

        m_tweenPosition.Play(true);
        m_tweenAlpha.Reset();
    }

    private void SetType(int type)
    {
        SetAllDisable();
        switch (type)
        {
            case 1:
                SoundManager.PlaySound("Enhanced_enemyimpact1.ogg");
                m_jinzhanatk.SetActive(true);
                break;
            case 2:
                SoundManager.PlaySound("Enhanced_enemyimpact2.ogg");
                m_yuanchengatk.SetActive(true);
                break;
            case 3:
                SoundManager.PlaySound("LaserMine_Explode.ogg");
                m_gongchengatk.SetActive(true);
                break;
            case 4:
                SoundManager.PlaySound("Enhanced_enemyimpact3.ogg");
                m_anshaatk.SetActive(true);
                break;
            case 5:
                SoundManager.PlaySound("Enhanced_enemyimpact1.ogg");
                m_stjinzhanatk.SetActive(true);
                break;
            case 6:
                SoundManager.PlaySound("Enhanced_enemyimpact2.ogg");
                m_styuanchengatk.SetActive(true);
                break;
            case 7:
                SoundManager.PlaySound("Launch_1.ogg");
                m_toweratk.SetActive(true);
                break;
            case 8://hit
                m_hit.SetActive(true);
                m_hitScale.Reset();
                m_hitAlpha.Reset();
                m_hitRot.Reset();
                m_hitScale.Play(true);
                m_hitAlpha.Play(true);
                m_hitRot.Play(true);
                break;
            default:
                break;
        }
    }

    private void SetAllDisable()
    {
        m_jinzhanatk.SetActive(false);
        m_stjinzhanatk.SetActive(false);
        m_yuanchengatk.SetActive(false);
        m_styuanchengatk.SetActive(false);
        m_toweratk.SetActive(false);
        m_gongchengatk.SetActive(false);
        m_anshaatk.SetActive(false);
        m_hit.SetActive(false);
    }

    private void TweenPositionFinished(UITweener t)
    {
        m_tweenAlpha.Play(true);
    }

    private void TweenAlphaFinished(UITweener t)
    {
        OnFinished.SafeInvoke(this);
    }

    //void OnDestroy()
    //{
    //    TimerHeap.DelTimer(m_alphaTimerId);
    //}
}