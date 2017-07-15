using UnityEngine;
using System.Collections;
using Mogo.Util;

public class RecoverEnergyUIMgr : MonoBehaviour {

    private UISprite m_imgBar;
    private UISprite m_imgAnim;
    private Vector3 m_posLeft;
    private Transform m_goMinPos;

    private float m_curValue;
    private float m_maxValue = 100f;

    public const string SetBarPercentEvent = "RecoverEnergyUIMgr.SetBarPercent";
    public const string SetMinPercentEvent = "RecoverEnergyUIMgr.SetMinPercentEvent";

    void Awake()
    {
        m_imgBar = transform.FindChild("Panel/Bar").GetComponent<UISprite>();
        m_imgBar.pivot = UIWidget.Pivot.Left;
        m_imgBar.fillAmount = 0f;
        m_imgAnim = transform.FindChild("Panel/Anim").GetComponent<UISprite>();
        m_imgAnim.gameObject.SetActive(false);
        m_goMinPos = transform.FindChild("Panel/MinPos");
        m_goMinPos.GetComponent<UILabel>().text = "I";

        m_posLeft = m_imgBar.transform.localPosition;

        EventDispatcher.AddEventListener<float>(SetBarPercentEvent, SetBarPercent);
        EventDispatcher.AddEventListener<float>(SetMinPercentEvent, SetMinPercent);
    }

    void OnEnable()
    {
        SetBarPercent(0);
    }

    void OnDisable()
    {
        SetBarPercent(0);
    }

    private void SetBarPercent(float curValue)
    {
        m_curValue = curValue;
        float percent = curValue / m_maxValue;
        m_imgBar.fillAmount = percent;
        float posX = m_posLeft.x + m_imgBar.transform.localScale.x * percent;
        m_imgAnim.transform.localPosition = new Vector3(posX, m_posLeft.y, m_posLeft.z);
        m_imgAnim.gameObject.SetActive(true);
    }

    /// <summary>
    /// 可释放的最小百分比
    /// </summary>
    /// <param name="percent"></param>
    private void SetMinPercent(float percent)
    {
        float posX = m_posLeft.x + m_imgBar.transform.localScale.x * percent;
        m_goMinPos.localPosition = new Vector3(posX, m_goMinPos.localPosition.y, m_goMinPos.localPosition.z);
    }
}
