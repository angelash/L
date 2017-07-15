using UnityEngine;
using System.Collections;
using Mogo.FSM;

namespace Mogo.Game
{
    public class PlayerBattleManager : BattleManager
    {
        public PlayerBattleManager(EntityParent _owner, SkillManager _skillManager) : base(_owner,_skillManager)
        {
            theOwner = _owner;
            skillManager = _skillManager;
        }

        public void Attack(int attackMode,bool isFire)
        {
            theOwner.CastSkill(attackMode,isFire);
        }

        override public void CastSkill(uint nSpellID)
        {
            theOwner.ChangeMotionState(MotionState.ATTACKING, nSpellID);
        }

    }
}
