using UnityEngine;
using System.Collections;
using Mogo.Util;

public class ControllerStick : MonoBehaviour {

    private Vector2 actualCenterBg;
    private Vector2 actualCenterStick;
    private Vector2 direction;

    private Vector3 m_tranControllerBtnOriginalPos;
    private Transform m_tranControllerBg;
    private Transform m_tranControllerButton;
    private Camera RelatedCamera;
    private Rect actualRectBg;
    private Rect actualRectStick;
    private Rect rectCanTouch;

    private UISprite m_imgControllerBg;

    private float actualRangeStick;
    private float rangeStick = 0.08f;
    private float actualRadiusBg = 0;
    private float actualRadiusStick = 0;

    private int fingerId = -100;
    private bool isDraging;

    /// <summary>
    ///     是否使用键盘操作
    /// </summary>
    private bool m_isKeyboard;

    public bool IsKeyboard
    {
        get { return m_isKeyboard; }
        set
        {
            m_isKeyboard = value;
            gameObject.SetActive(!IsKeyboard);
            //var fireUIMgr = UIManager.I.GetUILogic<FireUIMgr>();
            //if (fireUIMgr.bShow)
            //{
            //    fireUIMgr.SetOnlyPressBgFire(!IsKeyboard);
            //}
            if (!IsKeyboard)
            {
                MogoWorld.m_uiManager.ShowAttackUI1();
                MogoWorld.m_uiManager.ShowAttackUI2();
                UIManager.I.ShowUI<CameraControllerUIMgr>();
            }
            else
            {
                MogoWorld.m_uiManager.CloseAttackUI1();
                MogoWorld.m_uiManager.CloseAttackUI2();
                UIManager.I.CloseUI<CameraControllerUIMgr>();
            }
        }
    }

    void Awake()
    {
        m_tranControllerBg = transform.FindChild("ControllerBg");
        m_tranControllerButton = transform.FindChild("ControllerBtn");
        m_tranControllerBtnOriginalPos = Vector3.zero;
        RelatedCamera = MogoWorld.m_uiManager.UICamera;
        m_imgControllerBg = m_tranControllerBg.GetComponent<UISprite>();
        m_imgControllerBg.alpha = 0.6f;
        Invoke("InitSize", 0.1f);
    }

	// Use this for initialization
	void Start () {
        //MogoWorld.thePlayer.controllerInput.AutoSetStickType(true);
	}
	
	// Update is called once per frame
	void Update () {
        if (isDraging)
        {
            var touchPosition = GetTouchPosition();
            ChangeStickPositon(touchPosition);

            direction = (actualCenterStick - actualCenterBg);
            direction = direction.normalized;
            m_tranControllerButton.localPosition = m_tranControllerBtnOriginalPos + new Vector3(direction.x * 40, -direction.y * 40, 0);
            //LoggerHelper.Debug("stickType=" + MogoWorld.thePlayer.controllerInput.stickType);
            //LoggerHelper.Debug(string.Format("touchPosition={0}, m_tranControllerButton.localPosition={1}, direction={2}, actualCenterStick={3}, actualCenterBg={4}", touchPosition, m_tranControllerButton.localPosition, direction, actualCenterStick, actualCenterBg));
            //if (MogoWorld.thePlayer.controllerInput.stickType == StickType.Touch)
            //{
            //    float xSpeed = direction.x;
            //    float ySpeed = -direction.y;
            //    MogoWorld.thePlayer.controllerInput.XSpeed = xSpeed;
            //    MogoWorld.thePlayer.controllerInput.ZSpeed = ySpeed;
            //    //LoggerHelper.Debug("xSpeed=" + xSpeed);
            //    //LoggerHelper.Debug("ySpeed=" + ySpeed);
            //}
        }
	}

    void OnPress(bool isPressed)
    {
        UIUtils.ButtonIsPressing = isPressed;
        if (isPressed)
        {
            var touchPosition = GetTouchPosition(true);
            TouchBegin(touchPosition);
        }
        else
        {
            Reset();
        }
    }

    private void InitSize()
    {
        actualCenterBg.x = m_tranControllerBg.localPosition.x * Screen.width / RelatedCamera.orthographicSize;
        actualCenterBg.y = m_tranControllerBg.localPosition.y * Screen.height / (RelatedCamera.orthographicSize / RelatedCamera.aspect);

        float size = Mathf.Max(Screen.width, Screen.height);
        actualRangeStick = rangeStick * size;

        //坐标原点转换为左下角,因为摇杆通常设置在左下角
        actualCenterBg = new Vector2(actualCenterBg.x, Screen.height - actualCenterBg.y);
        actualCenterStick = new Vector2(actualCenterStick.x, Screen.height - actualCenterStick.y);

        actualRectBg = new Rect(actualCenterBg.x - actualRadiusBg, actualCenterBg.y - actualRadiusBg,
            actualRadiusBg * 2, actualRadiusBg * 2);

        actualRectStick = new Rect(actualCenterStick.x - actualRadiusStick, actualCenterStick.y - actualRadiusStick,
            actualRadiusStick * 2, actualRadiusStick * 2);

        var box = GetComponent<BoxCollider>();
        var xAspect = (float)Screen.width / 1280;
        var yAspect = (float)Screen.height / 720;
        float left = 0f;
        float top = Screen.height - transform.localPosition.y * yAspect - box.size.y * 0.5f;
        rectCanTouch = new Rect(left, top, box.size.x, box.size.y);
        isDraging = false;
    }

    private Vector2 GetTouchPosition(bool touchBegin = false)
    {
        var touchPosition = Vector2.zero;
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                {
                    if (touchBegin)
                    {
                        for (var i = 0; i < Input.touchCount; i++)
                        {
                            var touch = Input.GetTouch(i);
                            if (touch.phase == TouchPhase.Began)
                            {
                                var temp = new Vector2(touch.position.x, Screen.height - touch.position.y);
                                if (rectCanTouch.Contains(temp))
                                {
                                    touchPosition = temp;
                                    fingerId = touch.fingerId;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (var i = 0; i < Input.touchCount; i++)
                        {
                            var touch = Input.GetTouch(i);
                            if (touch.fingerId == fingerId)
                            {
                                var temp = new Vector2(touch.position.x, Screen.height - touch.position.y);
                                touchPosition = temp;
                                break;
                            }
                        }
                    }
                    break;
                }
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                {
                    touchPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    touchPosition.y = Screen.height - touchPosition.y;
                    break;
                }
            default:
                break;
        }
        return touchPosition;
    }

    public void Reset()
    {
        actualRectStick.center = actualRectBg.center;
        actualCenterStick = actualRectBg.center;
        isDraging = false;
        direction = Vector2.zero;
        fingerId = -100;
        try
        {
            m_tranControllerButton.localPosition = m_tranControllerBtnOriginalPos;
            //if (MogoWorld.thePlayer.controllerInput.stickType == StickType.Touch)
            //{
            //    MogoWorld.thePlayer.controllerInput.XSpeed = direction.x;
            //    MogoWorld.thePlayer.controllerInput.ZSpeed = direction.y;
            //}
        }
        catch
        {
        }
        m_imgControllerBg.alpha = 0.6f;
    }

    private void TouchBegin(Vector2 touchPosition)
    {
        isDraging = true;

        actualRectBg.center = touchPosition;
        actualCenterBg = touchPosition;

        actualRectStick.center = touchPosition;
        actualCenterStick = touchPosition;

        m_imgControllerBg.alpha = 1f;
    }

    private void ChangeStickPositon(Vector2 touchPosition)
    {
        var v = touchPosition - actualRectBg.center;
        if (v.magnitude > actualRangeStick)
        {
            v = v.normalized;
            v = v * actualRangeStick;
            v = actualRectBg.center + v;
            actualRectStick.center = v;
            actualCenterStick = v;
        }
        else
        {
            actualRectStick.center = touchPosition;
            actualCenterStick = touchPosition;
        }
    }
}
