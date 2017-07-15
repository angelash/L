/*********************************************************
 * Copyright (C) 2013 广州，爱游
 *
 * 模块名：MogoUIListener
 * 创建者：李建辉
 * 修改者列表：
 * 创建日期：2016/4/18 14:57:49
 * 模块描述：
 * 用法实例：
 *
 * *******************************************************/

using System;
using UnityEngine;

public class RangleCircle : MonoBehaviour
{
    public Transform m_Transform;
    public float m_Radius = 0.5f; // 圆环的半径
    public float m_Theta = 0.1f; // 值越低圆环越平滑
    public Color m_Color = Color.green; // 线框颜色
    public bool IsDrawed = false;//是否已经画成

    private void Start()
    {
        if (m_Transform == null)
        {
            throw new Exception("Transform is NULL.");
        }
    }

    //public void DrawCricle(int radius)
    //{
    //    IsDrawed = true;
    //    m_Radius = radius;
    //    OnDrawGizmos();
    //}

    private void OnDrawGizmos()
    {
        Debug.Log("绘制圆圈");
        //if (IsDrawed == true) return;
        if (m_Transform == null) return;
        if (m_Theta < 0.0001f) m_Theta = 0.0001f;

        // 设置矩阵
        Matrix4x4 defaultMatrix = Gizmos.matrix;
        Gizmos.matrix = m_Transform.localToWorldMatrix;

        // 设置颜色
        Color defaultColor = Gizmos.color;
        Gizmos.color = m_Color;

        // 绘制圆环
        Vector3 beginPoint = Vector3.zero;
        Vector3 firstPoint = Vector3.zero;
        for (float theta = 0; theta < 2 * Mathf.PI; theta += m_Theta)
        {
            float x = m_Radius * Mathf.Cos(theta);
            float y = m_Radius * Mathf.Sin(theta);
            Vector3 endPoint = new Vector3(x, y, 0);
            //Debug.Log("x:" + x + "   y:" + y);
            if (theta == 0)
            {
                firstPoint = endPoint;
            }
            else
            {
                Gizmos.DrawLine(beginPoint, endPoint);
            }
            beginPoint = endPoint;
        }

        // 绘制最后一条线段
        Gizmos.DrawLine(firstPoint, beginPoint);

        // 恢复默认颜色
        Gizmos.color = defaultColor;

        // 恢复默认矩阵
        Gizmos.matrix = defaultMatrix;

        IsDrawed = true;
    }
}