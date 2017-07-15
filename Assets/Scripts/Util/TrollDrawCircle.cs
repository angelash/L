using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TrollDrawCircle : MonoBehaviour
{

    public int PointCount = 50;
    public float Radius = 10f;
    private float m_angle;
    private List<Vector3> m_points = new List<Vector3>();
    private LineRenderer m_renderer;

    private Vector3 m_lastPos;
    // Use this for initialization
    void Start()
    {
        m_lastPos = transform.position;
        m_angle = 360f / PointCount;
        m_renderer = GetComponent<LineRenderer>();
        if (!m_renderer)
        {
            m_renderer = gameObject.AddComponent<LineRenderer>();
        }
        m_renderer.castShadows = false;
        m_renderer.receiveShadows = false;
        DrawLines();
    }

    public void SetPointCount(int pointCount)
    {
        PointCount = pointCount;
        m_angle = 360f / PointCount;
        DrawLines();
    }

    public void SetRadius(float radius)
    {
        Radius = radius;
        DrawLines();
    }

    public void DrawLines()
    {
        if (m_renderer == null)
            return;
        m_renderer.SetVertexCount(PointCount + 1);  ///这里是设置圆的点数，加1是因为加了一个终点（起点）
        CalculationPoints();
        DrowPoints();
        ClearPoints();
    }

    void CalculationPoints()
    {
        Vector3 v = transform.position + transform.up * Radius;
        m_points.Add(v);
        Quaternion r = transform.rotation;
        for (int i = 1; i < PointCount; i++)
        {
            Quaternion q = Quaternion.Euler(r.eulerAngles.x, r.eulerAngles.y, r.eulerAngles.z - (m_angle * i));
            v = transform.position + (q * Vector3.up) * Radius;
            m_points.Add(v);
        }
    }
    void DrowPoints()
    {
        for (int i = 0; i < m_points.Count; i++)
        {
            //  Debug.DrawLine(transform.position, points[i], Color.green);
            m_renderer.SetPosition(i, m_points[i]);  //把所有点添加到positions里
        }
        if (m_points.Count > 0)   //这里要说明一下，因为圆是闭合的曲线，最后的终点也就是起点，
            m_renderer.SetPosition(PointCount, m_points[0]);
    }
    void ClearPoints()
    {
        m_points.Clear();  ///清除所有点
    }

    //// Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(m_lastPos, transform.position) > 0.01f)
        {
            m_lastPos = transform.position;
            DrawLines();
        }
    }
}