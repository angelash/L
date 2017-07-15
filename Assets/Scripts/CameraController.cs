using Mogo.Util;
using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static float MaxScale = 2;
    public static float MinScale = 0.6f;

    //public static string CameraScaleChanged = "CameraScaleChanged";
    private float m_currentScale = 1.0f;

    private bool m_isTouchScaling;

    public OptDragListener.VectorDelegate onDrag;
    public UIEventListener.FloatDelegate onScale;

    private int[] LimitPos = new int[] { -70, 50, 260, -250 };//左右上下

    private void Start()
    {
        var lis = OptDragListener.Get(gameObject);
        lis.onDrag = OnThisDrag;
    }

    private void OnThisDrag(GameObject go, Vector3 offset, Vector3 currentPos)
    {
        if (m_isTouchScaling)//缩放的时候不响应拖拽
            return;
        if (offset.x < -100 || offset.x > 100 || offset.y < -100 || offset.y > 100)//限制手机上误操作坐标突变
            return;
        transform.position += offset;
        if (onDrag != null) onDrag(go, offset, currentPos);
    }

    public float CurrentScale
    {
        get { return m_currentScale; }
        set
        {
            m_currentScale = value;
            if (onScale != null) onScale(gameObject, m_currentScale);
            //EventDispatcher.TriggerEvent<float>(CameraScaleChanged, m_currentScale);
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                transform.localScale = new Vector3(m_currentScale, m_currentScale, 1);
            else
                TweenScale.Begin(gameObject, 0.2f, new Vector3(m_currentScale, m_currentScale, 1));
        }
    }

    //记录上一次手机触摸位置判断用户是在左放大还是缩小手势
    private Vector2 oldPosition1;

    private Vector2 oldPosition2;

    //记录上一次按下屏幕的位置
    private Vector2 oldPositoin3;

    private bool m_isMoving;
    private Vector3 m_lastMovingPos;
    public void MoveTo(Vector3 targetPos, float duration = 0.2f)
    {
        m_isMoving = true;
        m_lastMovingPos = transform.localPosition;
        var tPos = TweenPosition.Begin(gameObject, duration, targetPos);
        tPos.onFinished = OnFinished;
    }

    private void OnFinished(UITweener tween)
    {
        if (onDrag != null) onDrag(gameObject, transform.localPosition - m_lastMovingPos, transform.localPosition);
        m_isMoving = false;
    }

    private void Update()
    {
        if (m_isMoving)
        {
            var pos = transform.localPosition;
            if (onDrag != null) onDrag(gameObject, pos - m_lastMovingPos, pos);
            m_lastMovingPos = pos;
        }
        if (Input.touchCount == 1)
        {
            m_isTouchScaling = false;
        }
        if (Input.touchCount > 1)
        {
            //前两只手指触摸类型都为移动触摸
            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                m_isTouchScaling = true;
                //计算出当前两点触摸点的位置
                var tp1 = Input.GetTouch(0).position;
                var tp2 = Input.GetTouch(1).position;

                //函数返回真为放大，返回假为缩小
                var value = IsEnlarge(oldPosition1, oldPosition2, tp1, tp2);
                if (value > 0)
                {
                    //缩小系数返回MaxScale后不允许继续缩小
                    if (CurrentScale < MaxScale)
                    {
                        CurrentScale *= 1.03f;
                    }
                }
                else if (value < 0)
                {
                    //放大系数超过MinScale以后不允许继续放大
                    if (CurrentScale > MinScale)
                    {
                        CurrentScale *= 0.97f;
                    }
                }
                //备份上一次触摸点的位置，用于对比
                oldPosition1 = tp1;
                oldPosition2 = tp2;
            }
        }
        else
        {
            m_isTouchScaling = false;
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (CurrentScale > MinScale)
                {
                    CurrentScale *= 0.8f;
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (CurrentScale < MaxScale)
                {
                    CurrentScale *= 1.2f;
                }
            }
            //else if (Input.GetMouseButton(0))
            //{
            //    OnMousePress();
            //}

            //if (Input.GetMouseButtonUp(0))
            //{
            //    OnMouseUp();
            //}
        }
    }

    /// <summary>
    /// 左键放开
    /// </summary>
    private void OnMouseUp()
    {
        if (this.gameObject.transform.localPosition.x >= LimitPos[1])
        {
            //Debug.Log("重置坐标");
            this.gameObject.transform.localPosition = new Vector3(LimitPos[1], this.gameObject.transform.localPosition.y, 0);
        }
        if (this.gameObject.transform.localPosition.x <= LimitPos[0])
        {
            this.gameObject.transform.localPosition = new Vector3(LimitPos[0], this.gameObject.transform.localPosition.y, 0);
        }
        if (this.gameObject.transform.localPosition.y >= LimitPos[2])
        {
            this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.y, LimitPos[2], 0);
        }
        if (this.gameObject.transform.localPosition.y <= LimitPos[3])
        {
            this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.y, LimitPos[3], 0);
        }
    }

    /// <summary>
    /// 左键按下
    /// </summary>
    private void OnMousePress()
    {
        Vector2 tp3 = Input.mousePosition;
        //Debug.Log("左键按下:" + tp3);
        //dragObj.enabled = true;
        if (tp3.x - oldPositoin3.x > 0)//向右移动
        {
            if (this.gameObject.transform.localPosition.x >= LimitPos[1])
            {
                //DragObj.enabled = false;
            }
            else
            {
                //DragObj.enabled = true;
            }
        }
        if (tp3.x - oldPositoin3.x < 0)//向左移动
        {
            if (this.gameObject.transform.localPosition.x <= LimitPos[0])
            {
                //DragObj.enabled = false;
            }
            else
            {
                //DragObj.enabled = true;
            }
        }

        if (tp3.y - oldPositoin3.y > 0)//向上移动
        {
            if (this.gameObject.transform.localPosition.y >= LimitPos[2])
            {
                //DragObj.enabled = false;
            }
            else
            {
                //DragObj.enabled = true;
            }
        }

        if (tp3.y - oldPositoin3.y < 0)//向下移动
        {
            if (this.gameObject.transform.localPosition.y <= LimitPos[3])
            {
                //DragObj.enabled = false;
            }
            else
            {
                //DragObj.enabled = true;
            }
        }

        oldPositoin3 = tp3;
    }

    //函数返回真为放大，返回假为缩小
    private int IsEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //函数传入上一次触摸两点的位置与本次触摸两点的位置计算出用户的手势
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        //MogoMsgBox.Instance.ShowFloatingText(string.Concat(leng1, "-", leng2, "=", leng1 - leng2), 3);
        if (Math.Abs(leng1 - leng2) < 5)//降低灵敏度，此处的魔法数值根据感受调整
            return 0;
        else if (leng1 < leng2)
        {
            //放大手势
            return 1;
        }
        else
        {
            //缩小手势
            return -1;
        }
    }
}