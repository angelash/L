
namespace Mogo.Game
{
    public class PlayerSkillManager : SkillManager
    {
        public PlayerSkillManager(EntityParent owner)
        : base(owner)
        {
            theOwner = owner;
        }

    }
}
