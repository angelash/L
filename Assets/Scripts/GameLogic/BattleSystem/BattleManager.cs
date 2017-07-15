using System;
using Mogo.FSM;
using Mogo.UI;
using UnityEngine;


namespace Mogo.Game
{
    public class BattleManager
    {
        protected EntityParent theOwner;
        public SkillManager skillManager;

        public int CurrentAttckMode;

        public BattleManager(EntityParent _owner, SkillManager _skillManager)
        {

            theOwner = _owner;
            skillManager = _skillManager;
        }

        

        // Walk状态
        virtual public void Move()
        {
            if (theOwner.CurrentMotionState == MotionState.LOCKING
                || theOwner.CurrentMotionState == MotionState.ATTACKING
                || theOwner.CurrentMotionState == MotionState.DEAD
                || theOwner.CurrentMotionState == MotionState.HIT
                || theOwner.CurrentMotionState == MotionState.PICKING
                || theOwner.CurrentMotionState == MotionState.ROLL)
            {
                return;
            }
            //theOwner.ChangeMotionState(MotionState.WALKING);
        }

        // Idle状态
        virtual public void Idle()
        {
            if (theOwner.CurrentMotionState == MotionState.LOCKING
                || theOwner.CurrentMotionState == MotionState.ATTACKING
                || theOwner.CurrentMotionState == MotionState.DEAD
                || theOwner.CurrentMotionState == MotionState.PICKING
                || theOwner.CurrentMotionState == MotionState.PREPARING
                || theOwner.CurrentMotionState == MotionState.ROLL)
            {
                return;
            }
            //theOwner.ChangeMotionState(MotionState.IDLE);
        }

        // 主动释放技能。直接进入PREPARING放技能
        virtual public void CastSkill(uint actionID)
        {
            theOwner.ChangeMotionState(MotionState.ATTACKING, actionID);
        }

        virtual public void OnAttacking(int spellID)
        {
            skillManager.OnAttacking(spellID);
        }

    }
}
