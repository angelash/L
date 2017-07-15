using Mogo.FSM;
using Mogo.Game;

public class OtherBattleManager : BattleManager
{
    public OtherBattleManager(EntityParent _owner, SkillManager _skillManager) : base(_owner,_skillManager)
        {
        theOwner = _owner;
        skillManager = _skillManager;
    }

    override public void CastSkill(uint nSpellID)
    {
        theOwner.ChangeMotionState(MotionState.ATTACKING, nSpellID);
    }

}
