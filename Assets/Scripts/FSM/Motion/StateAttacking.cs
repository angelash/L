/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：StateAttacking
// 创建者：Steven Yang
// 修改者列表：2013-1-29
// 创建日期：
// 模块描述：Attacking状态
//----------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;

using Mogo.Util;
using Mogo.Game;
using Mogo.GameData;
using UnityEngine;

namespace Mogo.FSM
{
    public class StateAttacking : IState
    {
        // 进入该状态
        public void Enter(EntityParent theOwner, params System.Object[] args)
        {
        }

        private void DestroyCurrentAttackChargeFx(EntityParent theOwner)
        {
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