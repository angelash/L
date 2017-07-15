using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TrollDrawArrow : MonoBehaviour
{
    public Vector3 SourcePos;
    public float SourceRadius;
    public Vector3 TargetPos;
    public float TargetRadius;

    public int PointCount = 2;
    public float Range = 10f;
    private List<Vector3> m_points = new List<Vector3>();
    private LineRenderer m_renderer;

    //private Vector3 m_lastPos;
    // Use this for initialization
    void Start()
    {
        //m_lastPos = transform.position;
        m_renderer = GetComponent<LineRenderer>();
        if (!m_renderer)
        {
            m_renderer = gameObject.AddComponent<LineRenderer>();
        }
        m_renderer.castShadows = false;
        m_renderer.receiveShadows = false;
        m_renderer.SetVertexCount(PointCount);
    }

    public void SetPointCount(int pointCount)
    {
        PointCount = pointCount;
        m_renderer.SetVertexCount(PointCount);
    }

    public void SetSourcePos(Vector3 pos)
    {
        SourcePos = pos;
    }

    public void SetSourceRadius(float radius)
    {
        SourceRadius = radius;
    }

    public void DrawLineToTargetPoint(Vector3 pos)
    {
        TargetPos = pos;
        TargetRadius = 0;
        DrawLines(false);
    }

    public void DrawArrowToTargetPoint(Vector3 pos)
    {
        TargetPos = pos;
        TargetRadius = 0;
        DrawLines(true);
    }

    public void DrawArrowToTargetCircle(float radius, Vector3 pos)
    {
        TargetPos = pos;
        TargetRadius = radius;
        DrawLines(true);
    }

    public void DrawLines(bool drawArrow)
    {
        if (m_renderer == null)
            return;
        DrowPoints(drawArrow);
        //ClearPoints();
    }

    void DrowPoints(bool drawArrow)
    {
        if (drawArrow)
        {
            m_renderer.SetVertexCount(5);
        }
        else
        {
            m_renderer.SetVertexCount(2);
        }
        var targetPos = TargetPos;
        m_renderer.SetPosition(0, SourcePos);
        m_renderer.SetPosition(1, targetPos);

        //Debug.Log(targetPos + " " + SourcePos + " " + Vector3.Angle(targetPos - SourcePos, Vector3.up));
        if (drawArrow)
        {
            Vector3 v = targetPos;
            Quaternion r = Quaternion.FromToRotation(new Vector3(SourcePos.x - targetPos.x, targetPos.y - SourcePos.y, targetPos.z - SourcePos.z), Vector3.up);
            Quaternion q1 = Quaternion.Euler(r.eulerAngles.x, r.eulerAngles.y, r.eulerAngles.z + 25);//箭头夹角
            v = targetPos - (q1 * Vector3.up) * 30;//箭头边长
            m_renderer.SetPosition(2, v);
            Quaternion q2 = Quaternion.Euler(r.eulerAngles.x, r.eulerAngles.y, r.eulerAngles.z - 25);
            v = targetPos - (q2 * Vector3.up) * 30;
            m_renderer.SetPosition(3, v);


            m_renderer.SetPosition(4, targetPos);
            //Debug.Log(q1.eulerAngles + " " + q2.eulerAngles);
        }
    }

    //void ClearPoints()
    //{
    //    //m_points.Clear();  ///清除所有点
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    if (Vector3.Distance(m_lastPos, transform.position) > 0.01f)
    //    {
    //        m_lastPos = transform.position;
    //        DrawLines();
    //    }
    //}
}