/*----------------------------------------------------------------
// Copyright (C) 2013 ���ݣ�����
//
// ģ������RandomHelper
// �����ߣ�Key Pan
// �޸����б�Key Pan
// �������ڣ�20130227
// ����޸����ڣ�20130228
// ģ�����������������
// ����汾�����԰�V1.1
//----------------------------------------------------------------*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Mogo.Util;
using Mogo.Game;
using Mogo.GameData;

namespace Mogo.FSM
{
    public class StateCharging : IState
    {
        // �����״̬
        public void Enter(EntityParent theOwner, params System.Object[] args)
        {
        }

        private void DestroyCurrentChargeFx(EntityParent theOwner)
        {
        }

        // �뿪״̬
        public void Exit(EntityParent theOwner, params System.Object[] args)
        {
        }

        // ״̬����
        public void Process(EntityParent theOwner, params System.Object[] args)
        {
        }
    }
}