/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：游戏UI显示控制器
// 创建者：Zeng Dexuan
// 修改者列表：
// 创建日期：2015-1-26
// 模块描述：此模块为公共类，统一管理UILogic的显示关系，如：UI共同显示，UI显示互斥，UI关联显示，UI显示时场景内各Camera的状态控制等。
//          思路源自杨振。
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Mogo.Util;
using UnityEngine;
using Mogo.GameData;

namespace Mogo.UI
{
    public class PopupManager
    {
        class MapTypeUIContainer
        {
            /// <summary>
            /// UI栈，缓存住一些被最小化的UI
            /// </summary>
            public Stack<UILogic> UIStack = new Stack<UILogic>();
            /// <summary>
            /// UI队列，缓存住一些想打开，但是未到开启条件的UI
            /// </summary>
            public Queue<Action> UIQueue = new Queue<Action>();
        }

        class PopupManagerData
        {
            public MapType CurrentMapType = MapType.Normal;
            public bool bInstance = false;
            public UILogic CurrentMainUI;
        }

        static HashSet<UILogic> m_uiHandler = new HashSet<UILogic>();
        static Dictionary<MapType, MapTypeUIContainer> m_uiCache = new Dictionary<MapType, MapTypeUIContainer>();
        static PopupManagerData m_data = new PopupManagerData();
        static GameObject m_mainCamera;

        public static GameObject MainCamera
        {
            get { return m_mainCamera; }
            set { m_mainCamera = value; }
        }

        static PopupManager()
        {
            EventDispatcher.AddEventListener<int, bool>(Events.InstanceEvent.InstanceLoaded, SceneLoaded);
            EventDispatcher.AddEventListener<int, bool>(Events.InstanceEvent.BeforeInstanceLoaded, BeforeInstanceLoaded);
        }

        /// <summary>
        /// 场景切换后处理（未完成）
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="isInstance"></param>
        static void SceneLoaded(int sceneID, bool isInstance)
        {
            //m_data.CurrentMapType = MapData.dataMap.Get(sceneID).type;
            //m_data.bInstance = isInstance;

            //TimerHeap.AddTimer(1000, 0, () =>
            //{
            //    if (UICommonData.MogoMainUICamera != null)
            //    {
            //        ShowCachedUI();
            //    }
            //    else
            //    {
            //        SceneLoaded(sceneID, isInstance);
            //    }
            //});
        }

        /// <summary>
        /// 场景切换前处理
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="isInstance"></param>
        static void BeforeInstanceLoaded(int sceneID, bool isInstance)
        {
            if (m_data.CurrentMainUI != null && !m_data.CurrentMainUI.Properties.HasProperty(UIProperties.DontCloseWhenSwitchScene))
            {
                m_data.CurrentMainUI.Close();
                m_data.CurrentMainUI = null;
            }

            if (m_uiCache.ContainsKey(m_data.CurrentMapType) == true)
            {
                var uiStack = m_uiCache[m_data.CurrentMapType].UIStack;
                while (uiStack.Count > 0)
                {
                    var ui = uiStack.Pop();
                    ui.Close();
                }
            }
        }

        /// <summary>
        /// 未完成
        /// </summary>
        static void ShowCachedUI()
        {
            if (m_uiCache.ContainsKey(m_data.CurrentMapType) == true)
            {
            }

            //if (m_data.CurrentMainUI == null)
            //{
            //    ShowMainUI();
            //}
        }

        static bool checkShowUIHasProperty(UIProperties property)
        {
            foreach (var item in m_uiHandler)
            {
                if (item.Properties.HasProperty(property) == true)
                    return true;
            }
            return false;
        }

        static MapTypeUIContainer getContainer(UILogic ui)
        {
            MapType maptype;

            if (ui.Properties.HasProperty(UIProperties.UseMapType))
                maptype = ui.UIMapType;
            else
                maptype = MapType.Normal;

            if (m_uiCache.ContainsKey(maptype) == false)
                m_uiCache.Add(maptype, new MapTypeUIContainer());

            return m_uiCache[maptype];
        }

        public static void ShowUI(UILogic ui, Action callback, object[] param, bool isShowWaitingTip = true)
        {
            if (ui.bShow == true)
            {
                ui.ShowWindow(param, callback);
                return;
            }

            if (ui.Parent != null)
            {
                if (ui.Parent.bShow == true)
                    ui.ShowWindow(param, callback);

                return;
            }

            if (isShowWaitingTip && ui.Properties.HasProperty(UIProperties.DontShowLoadingTip))
            {
                isShowWaitingTip = false;
            }

            if (ui.Properties.HasProperty(UIProperties.UseMapType) == true)
            {
                if (ui.UIMapType != m_data.CurrentMapType)
                {
                    getContainer(ui).UIQueue.Enqueue(() => ShowUI(ui, callback, param));
                    return;
                }
            }

            if (checkShowUIHasProperty(UIProperties.UseMapType) == true)
            {
                getContainer(ui).UIQueue.Enqueue(() => ShowUI(ui, callback, param));
                return;
            }

            InternalShowUI(ui, callback, param, isShowWaitingTip);
        }

        static void InternalShowUI(UILogic ui, Action callback, object[] param, bool isShowWaitingTip)
        {
            if (ui.Properties.HasProperty(UIProperties.FullScreen) == true)
            {
                if (m_data.CurrentMainUI != null)
                {
                    var item = m_data.CurrentMainUI;

                    item.HideWindow(null);

                    if (item.Properties.HasProperty(UIProperties.MinimizeByOther) == true)
                        getContainer(item).UIStack.Push(item);

                    m_uiHandler.Remove(item);
                }

                UICommonData.MogoMainUICamera.clearFlags = ui.CameraClearFlag;
            }

            ShowUIHandler(ui, callback, param, isShowWaitingTip);
        }

        static void ShowUIHandler(UILogic ui, Action callback, object[] param, bool isShowWaitingTip)
        {
            m_uiHandler.Add(ui);

            CheckRelease(ui);
            UpdateCameraStatus();
            //if (isShowWaitingTip)
            //    EventDispatcher.TriggerEvent(MogoGlobleUIManagerEventsEx.ShowWaitingTip, true);
            ui.ShowWindow(param,
                () =>
                {
                    m_data.CurrentMainUI = ui;
                    callback.SafeInvoke();
                    //if (isShowWaitingTip)
                    //    EventDispatcher.TriggerEvent(MogoGlobleUIManagerEventsEx.ShowWaitingTip, false);
                });
        }

        static void UpdateCameraStatus()
        {
            CameraType flag = 0;

            foreach (var item in m_uiHandler)
            {
                flag |= item.UsingCameras;
            }

            if (flag.HasCamera(CameraType.MogoMainCamera) == true)
            {
                MainCamera.SafeSetActive(true);
                //Debug.LogError("show main camera.");
            }
            else
            {
                MainCamera.SafeSetActive(false);
                //Debug.LogError("hide main camera.");
            }
        }

        public static void CloseUI(UILogic ui, Action callback = null)
        {
            if (ui == null || ui.bShow == false)
                return;

            ui.HideWindow(callback);

            if (ui.Parent != null)
                return;

            m_uiHandler.Remove(ui);
            if (m_data.CurrentMainUI == ui)
                m_data.CurrentMainUI = null;
            ShowCachedUI();
        }

        //static void ShowMainUI()
        //{
        //}

        private static Dictionary<UILogic, int> checkReleaseDict = new Dictionary<UILogic, int>();
        private static Dictionary<UILogic, int> addDict = new Dictionary<UILogic, int>();
        private static List<UILogic> removeList = new List<UILogic>();
        /// <summary>
        /// 检测释放面板资源
        /// </summary>
        internal static void CheckRelease(UILogic uiLogic)
        {
            if (uiLogic.Properties.HasProperty(UIProperties.DontRelease) || null != uiLogic.Parent)
            {
                return;
            }

            removeList.Clear();
            addDict.Clear();
            if (false == checkReleaseDict.ContainsKey(uiLogic))
            {
                checkReleaseDict[uiLogic] = 0;
            }

            foreach (KeyValuePair<UILogic, int> item in checkReleaseDict)
            {
                CheckUILogicCanRelease(item.Key);
            }

            foreach (KeyValuePair<UILogic, int> item in addDict)
            {
                checkReleaseDict[item.Key] = item.Value;
            }

            Release();
        }

        static void Release()
        {
            //卸载被其他手段删除的界面
            UILogic tempUILogic;
            for (int i = 0; i < removeList.Count; i++)
            {
                tempUILogic = removeList[i];
                ReleaseUILogic(tempUILogic);
                checkReleaseDict.Remove(tempUILogic);
            }
        }

        static void ReleaseUILogic(UILogic tempUILogic)
        {
            //if (null != tempUILogic.mainPanel)
            //{
            tempUILogic.ReleaseResources();
            //}
        }

        static void CheckUILogicCanRelease(UILogic item)
        {
            GameObject go = item.MyGameObject;
            if (null != go)
            {
                if (false == go.activeSelf)
                {
                    addDict.Add(item, checkReleaseDict[item] + 1);
                    if (addDict[item] > 2)
                    {
                        removeList.Add(item);
                    }
                }
                else
                {
                    addDict.Add(item, 0);//重置
                    removeList.Remove(item);
                }
            }
            else
            {
                //走到这里证明面板已经被其他手段给卸载掉了
                removeList.Add(item);
            }
        }
    }

    public enum MapType : byte
    {
        /// <summary>
        ///     普通地图
        /// </summary>
        Normal = 0,

        /// <summary>
        ///     副本地图
        /// </summary>
        Special = 1,

        /// <summary>
        ///     试炼之塔
        /// </summary>
        ClimbTower = 2,

        /// <summary>
        ///     多人非组队
        /// </summary>
        MULTIPLAYER = 3,

        /// <summary>
        ///     世界boss
        /// </summary>
        WORLDBOSS = 4,

        /// <summary>
        ///     湮灭之门
        /// </summary>
        BURY = 5,

        /// <summary>
        ///     竞技场地图
        /// </summary>
        ARENA = 6,

        /// <summary>
        ///     体验关
        /// </summary>
        EX_LEVEL = 7,

        /// <summary>
        ///     塔防地图
        /// </summary>
        TOWERDEFENCE = 8,

        /// <summary>
        ///     袭击地图
        /// </summary>
        ASSAULT = 9,

        /// <summary>
        ///     随机地图
        /// </summary>
        TIMEVORTEX = 10,

        /// <summary>
        ///     PVP地图
        /// </summary>
        OCCUPY_TOWER = 11,

        /// <summary>
        ///     迷雾深渊地图
        /// </summary>
        FOGGYABYSS = 12,

        /// <summary>
        ///     公会王城
        /// </summary>
        GUILD_HOME = 13,

        /// <summary>
        ///     公会副本
        /// </summary>
        GUILD_INSTANCE = 14,

        /// <summary>
        ///     公会多人副本
        /// </summary>
        GUILD_PVE = 15,

        /// <summary>
        ///     跨服王城
        /// </summary>
        CROSS_SERVER_HOME = 16,

        /// <summary>
        ///     跨服1V1
        /// </summary>
        CROSS_SERVER_ARENA = 17,

        /// <summary>
        ///     跨服3V3
        /// </summary>
        CROSS_SERVER_PVP = 18,

        /// <summary>
        ///     跨服竞技场
        /// </summary>
        CROSS_SERVER_MONSTER_ARENA = 19,

        /// <summary>
        ///     好友切磋
        /// </summary>
        FRIEND_FIGHT = 20,

        /// <summary>
        ///     巅峰之塔
        /// </summary>
        PEAKTOWER = 22,

        /// <summary>
        ///     资源争夺
        /// </summary>
        RESOURCE_SCRAMBLE = 23,

        /// <summary>
        ///     本服资源争夺
        /// </summary>
        RESOURCE_SCRAMBLE_LOCAL = 24,

        /// <summary>
        ///     野外场景
        /// </summary>
        WILD = 25,

        /// <summary>
        ///     玛蒙之地
        /// </summary>
        MAMEN = 26,

        /// <summary>
        ///     勇闯金矿场景
        /// </summary>
        GOLDREWARD = 27,

        /// <summary>
        ///     组队副本
        /// </summary>
        TEAMCOPY = 28,

        /// <summary>
        ///     国家领土战场景
        /// </summary>
        COUNTRY_FIGHT = 29,

        /// <summary>
        ///     材料副本
        /// </summary>
        MATERIALSCOPY = 31,

        /// <summary>
        ///     非主城的普通场景
        /// </summary>
        MAP_TYPE_NOT_KINGDOM_NORMAL = 32,

        /// <summary>
        ///     跨服永恒战场阵营战
        /// </summary>
        CROSS_CAMP_BATTLE = 33,

        /// <summary>
        ///     双星奇缘战斗副本
        /// </summary>
        DOUBLE_STAR = 34,

        JIU_HUN = 35,
    }

}
