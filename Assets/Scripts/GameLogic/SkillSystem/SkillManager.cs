using System;
using System.ComponentModel;
using Mogo.FSM;
using Mogo.GameData;
using Mogo.Util;
using UnityEngine;


namespace Mogo.Game
{
    public class SkillManager
    {
        protected EntityParent theOwner;

        public SkillManager(EntityParent owner)
        {
            theOwner = owner;
        }


        public void OnAttacking(int skillID)
        {
            AttackingFx(skillID);
        }


        //播放特效
        private void AttackingFx(int skillID)
        {

        }

    }
}
