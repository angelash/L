using UnityEngine;
using System.Collections;
using Mogo.Util;

namespace Mogo.Game
{
    public partial class EntityParent
    {
        public BattleManager battleManager;
        public SkillManager skillManager;


        virtual public void CastSkill(int nSkillID, bool isfire = false)
        {
        }

        virtual public void OnAttacking(int spellID, int actionID, Matrix4x4 ltwm, Quaternion rotation, Vector3 forward, Vector3 position)
        {
        }

        //动作播放完时调用
        virtual public void SkillActionEnd()
        {

        }

        virtual public void OnDeath(int hitActionID)
        {

        }


    }
}
