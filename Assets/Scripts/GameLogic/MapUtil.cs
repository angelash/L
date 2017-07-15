/*********************************************************
 * Copyright (C) 2016 广州，爱游
 *
 * 模块名：MogoUIListener
 * 创建者：李建辉
 * 修改者列表：
 * 创建日期：2016/4/16 16:30:05
 * 模块描述：辅助地图功能实现
 * 用法实例：
 *
 * *******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MapUtil
{
    private static MapUtil instance;

    public static MapUtil getInstance()
    {
        if (instance == null)
        {
            instance = new MapUtil();
        }
        return instance;
    }

    private MapUtil()
    {
    }

    #region 最短路径(两个星球之间)

    /// <summary>
    /// 有向边
    /// </summary>
    public class Edge
    {
        public int StartNodeID;
        public int EndNodeID;
        public double Weight; //权值，代价
    }

    /// <summary>
    /// PassedPath 用于缓存计算过程中的到达某个节点的权值最小的路径
    /// </summary>
    public class PassedPath
    {
        public int CurNodeID;
        public bool BeProcessed;   //是否已被处理
        public double Weight;        //累积的权值
        public List<int> PassedIDList; //路径

        public PassedPath(int ID)
        {
            this.CurNodeID = ID;
            this.Weight = double.MaxValue;
            this.PassedIDList = new List<int>();
            this.BeProcessed = false;
        }
    }

    /// <summary>
    /// PlanCourse 缓存从源节点到其它任一节点的最小权值路径＝》路径表
    /// </summary>
    public class PlanCourse
    {
        private Hashtable htPassedPath;

        #region ctor

        public PlanCourse(List<UnitStar> nodeList, int originID)
        {
            this.htPassedPath = new Hashtable();

            UnitStar originNode = null;
            foreach (UnitStar node in nodeList)
            {
                if (node.UnitId == originID)
                {
                    originNode = node;
                }
                else
                {
                    PassedPath pPath = new PassedPath(node.UnitId);
                    this.htPassedPath.Add(node.UnitId, pPath);//保存源节点跟其他节点的路径
                }
            }

            if (originNode == null)
            {
                throw new Exception("The origin node is not exist !");
            }

            this.InitializeWeight(originNode);
        }

        private void InitializeWeight(UnitStar originNode)
        {
            if ((originNode.EdgeList == null) || (originNode.EdgeList.Count == 0))
            {
                return;
            }

            foreach (Edge edge in originNode.EdgeList)
            {
                PassedPath pPath = this[edge.EndNodeID];
                if (pPath == null)
                {
                    continue;
                }

                pPath.PassedIDList.Add(originNode.UnitId);//找出跟源节点联通的点
                pPath.Weight = edge.Weight;
            }
        }

        #endregion ctor

        public PassedPath this[int nodeID]
        {
            get
            {
                return (PassedPath)this.htPassedPath[nodeID];
            }
        }
    }

    /// <summary>
    /// 最短路径算法实现
    /// </summary>
    public class RoutePlanner
    {
        public RoutePlanner()
        {
        }

        #region Paln

        //获取权值最小的路径
        public RoutePlanResult Paln(List<UnitStar> nodeList, int originID, int destID)
        {
            PlanCourse planCourse = new PlanCourse(nodeList, originID);

            UnitStar curNode = this.GetMinWeightRudeNode(planCourse, nodeList, originID);

            #region 计算过程

            while (curNode != null)
            {
                PassedPath curPath = planCourse[curNode.UnitId];//获取当前计算节点的路径
                foreach (Edge edge in curNode.EdgeList)
                {
                    //Debug.Log("curPath:" + curPath.CurNodeID + "    targetPath:" + edge.EndNodeID);
                    PassedPath targetPath = planCourse[edge.EndNodeID];//获取目标计算节点的路径
                    if (targetPath == null)//回溯到原点，没有意义
                    {
                        continue;
                    }
                    double tempWeight = curPath.Weight + edge.Weight;//算出当前计算节点的权重加上边的权重

                    if (tempWeight < targetPath.Weight)//如果小于直接到目标节点的权重，则更新
                    {
                        targetPath.Weight = tempWeight;//
                        targetPath.PassedIDList.Clear();

                        for (int i = 0; i < curPath.PassedIDList.Count; i++)
                        {
                            targetPath.PassedIDList.Add(curPath.PassedIDList[i]);
                        }

                        targetPath.PassedIDList.Add(curNode.UnitId);
                    }
                }

                //标志为已处理
                planCourse[curNode.UnitId].BeProcessed = true;
                //获取下一个未处理节点
                curNode = this.GetMinWeightRudeNode(planCourse, nodeList, originID);
            }

            #endregion 计算过程

            //表示规划结束
            return this.GetResult(planCourse, destID);
        }

        #endregion Paln

        #region private method

        #region GetResult

        //从PlanCourse表中取出目标节点的PassedPath，这个PassedPath即是规划结果
        private RoutePlanResult GetResult(PlanCourse planCourse, int destID)
        {
            PassedPath pPath = planCourse[destID];

            if (pPath.Weight == int.MaxValue)
            {
                RoutePlanResult result1 = new RoutePlanResult();
                result1.Weight = int.MaxValue;
                return result1;
            }

            List<UnitStar> passedStars = new List<UnitStar>();
            for (int i = 0; i < pPath.PassedIDList.Count; i++)
            {
                passedStars.Add(MogoWorld.m_dataMapManager.GetUnitStarById(pPath.PassedIDList[i]));
            }
            RoutePlanResult result = new RoutePlanResult();
            result.PassedStars = passedStars;
            result.Weight = pPath.Weight;
            return result;
        }

        #endregion GetResult

        #endregion private method

        #region GetMinWeightRudeNode

        //从PlanCourse取出一个当前累积权值最小，并且没有被处理过的节点
        private UnitStar GetMinWeightRudeNode(PlanCourse planCourse, List<UnitStar> nodeList, int originID)
        {
            double weight = double.MaxValue;
            UnitStar destNode = null;

            foreach (UnitStar node in nodeList)
            {
                if (node.UnitId == originID)
                {
                    continue;
                }

                PassedPath pPath = planCourse[node.UnitId];
                if (pPath.BeProcessed)
                {
                    continue;
                }

                if (pPath.Weight < weight)
                {
                    weight = pPath.Weight;
                    destNode = node;
                }
            }

            return destNode;
        }

        #endregion GetMinWeightRudeNode
    }

    public class RoutePlanResult
    {
        public List<UnitStar> PassedStars = new List<UnitStar>();

        public double Weight;

        /// <summary>
        ///计划结果
        /// </summary>
        /// <param name="passedStars">经过的星球</param>
        /// <param name="d">总路径的长度</param>
        public RoutePlanResult()
        {
        }

        public void SetResult(List<UnitStar> passedStars, double d)
        {
            this.PassedStars = passedStars;
            this.Weight = d;
        }
    }

    /// <summary>
    /// 从源星球到目标星球的最短路径(待优化：保存源星球的最短路径表，下次寻找不必再次运算算法)
    /// </summary>
    /// <param name="StartStar">源星球</param>
    /// <param name="EndStar">目标星球</param>
    /// <returns>所有经过的星球（包括目标星球）</returns>
    public static RoutePlanResult Plan(UnitStar StartStar, UnitStar EndStar)
    {
        RoutePlanResult result = new RoutePlanResult();
        //在调用最短路径算法前， Node.CheckNodeCanReach先判断目标节点是否在源节点移动半径内，是的话直接返回源节点（两点之间，直线最短）
        if (StartStar.CheckStarReachByDistance(EndStar) == true)
        {
            result.PassedStars.Add(StartStar);
            result.PassedStars.Add(EndStar);
            result.Weight = StartStar.GetTargetStarDistance(EndStar);
        }
        else
        {
            RoutePlanner planner = new RoutePlanner();
            result = planner.Paln(MogoWorld.m_dataMapManager.GetStarList(), StartStar.UnitId, EndStar.UnitId);
            result.PassedStars.Add(EndStar);
            planner = null;
        }
        return result;
    }

    #endregion 最短路径(两个星球之间)

    #region 根据建筑数量求星球建筑坐标

    /// <summary>
    /// 建筑按扇形等分建立在星球上
    /// </summary>
    /// <param name="star">星球类</param>
    /// <param name="num">建筑数量</param>
    /// /// <param name="radiusPlus">附加半径</param>
    /// <returns>建筑坐标列表</returns>
    public static List<Vector2> GetBuildingPosList(UnitStar star, int num, float plusRadius = 0)
    {
        float avgAngle = Mathf.PI * 2 / num;//每个建筑平均角度
        float angle = 0;
        Vector2 pos;
        List<Vector2> list = new List<Vector2>();
        for (int i = 0; i < num; i++)
        {
            pos = new Vector2();
            pos.x = star.PositionX + (star.BaseData.radius + plusRadius) * Mathf.Sin(angle);
            pos.y = star.PositionY + (star.BaseData.radius + plusRadius) * Mathf.Cos(angle);
            list.Add(pos);
            angle += avgAngle;
        }
        return list;
    }

    #endregion 根据建筑数量求星球建筑坐标

    #region 圆形

    public static void DrawCircle(GameObject canvas, MeshFilter filter, float raidus, float angle, Vector3 position)
    {
        int ANGLE_STEP = 15;
        var mesh = new Mesh();
        int len = (int)Math.Floor(angle / ANGLE_STEP);
        len = len + 2;
        Vector3[] vs = new Vector3[len];
        //第一个为圆心
        vs[0] = position;
        for (int i = 1; i < len; i++)
        {
            canvas.transform.position = position;
            //canvas.transform.rotation = theOwner.Transform.rotation;
            canvas.transform.Rotate(new Vector3(0, -angle * 0.5f, 0));
            if (i != len - 1)
            {//非最后一个点
                canvas.transform.Rotate(new Vector3(0, ANGLE_STEP * i, 0));
                var v = canvas.transform.position + canvas.transform.forward * raidus;
                vs[i] = v;
            }
            else
            {//最后一个顶点
                //float r = angle - ANGLE_STEP * (i - 1);
                canvas.transform.Rotate(new Vector3(0, angle, 0));
                var v = canvas.transform.position + canvas.transform.forward * raidus;
                vs[i] = v;
            }
        }
        //三角形数
        int tc = len - 2;
        int[] triangles = new int[tc * 3];
        for (int j = 0; j < tc; j++)
        {
            triangles[j * 3] = 0;
            triangles[j * 3 + 1] = j + 1;
            if (j != 23)
            {
                triangles[j * 3 + 2] = j + 2;
            }
            else
            {
                triangles[j * 3 + 2] = 1;
            }
        }

        canvas.transform.position = Vector3.zero;
        canvas.transform.rotation = new Quaternion();
        mesh.vertices = vs;
        mesh.triangles = triangles;
        filter.mesh = mesh;
    }

    #endregion 圆形
}