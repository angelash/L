/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：游戏UI控制器参数
// 创建者：Zeng Dexuan
// 修改者列表：
// 创建日期：2015-1-26
// 模块描述：
//----------------------------------------------------------------*/

using System;
namespace Mogo.UI
{
    [Flags]
    public enum CameraType
    {
        None = 0,
        MogoMainCamera = 1,
        MogoUICamera = 2,
        GlobalUICamera = 4,
        GlobalUI2Camera = 8,
        TeachUICamera = 16,
        UIFXCamera = 32,
    }

    [Flags]
    public enum UIProperties
    {
        None = 0,
        /// <summary>
        /// 限定UI只在某些场景打开
        /// </summary>
        UseMapType = 1,
        /// <summary>
        /// 全屏UI（关闭主摄像机）
        /// </summary>
        FullScreen = 2,
        /// <summary>
        /// 把其他UI最小化（但不清空队列，关闭本UI时会把其他UI恢复）
        /// </summary>
        MinimizeByOther = 4,
        /// <summary>
        /// 切场景时不关闭UI
        /// </summary>
        DontCloseWhenSwitchScene = 8,
        /// <summary>
        /// 打开自己时不把父UI隐藏
        /// </summary>
        DontCloseParent = 16,
        /// <summary>
        /// 关闭时不释放资源
        /// </summary>
        DontRelease = 32,
        /// <summary>
        /// 不参与旧UI队列管理
        /// </summary>
        DontUseMogoUIManager = 64,
        /// <summary>
        /// 不显示LoadingTip
        /// </summary>
        DontShowLoadingTip = 128,
    }

    public static class UIConst
    {
        public static bool HasProperty(this UIProperties flag, UIProperties property)
        {
            return (flag & property) == property;
        }

        public static bool HasCamera(this CameraType flag, CameraType type)
        {
            return (flag & type) == type;
        }
    }
}
