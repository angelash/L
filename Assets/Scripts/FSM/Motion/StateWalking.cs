/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：StateWalking
// 创建者：Steven Yang
// 修改者列表：2013-1-29
// 创建日期：
// 模块描述：Walking状态
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using System.Collections;

using Mogo.Util;
using Mogo.Game;

namespace Mogo.FSM
{
    public class StateWalking : IState
    {
        // 进入该状态
        public void Enter(EntityParent theOwner, params System.Object[] args)
        {
            theOwner.CurrentMotionState = MotionState.WALKING;
        }

        // 离开状态
        public void Exit(EntityParent theOwner, params System.Object[] args)
        {
        }

        // 状态处理
        public void Process(EntityParent theOwner, params System.Object[] args)
        {
        }

    }
}