using RimWorld;
using Verse;

namespace ForeignerOfUniverse.Verbs
{
    public class CastAbilityDirectly : Verb_CastAbility
    {
        public override bool HidePawnTooltips => true;

        public override bool CanHitTarget(LocalTargetInfo targ)
        {
            return caster != null && caster.Spawned && CanHitTargetFrom(caster.Position, targ);
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (ReferenceEquals(targ.Thing, caster))
            {
                return verbProps.targetParams.canTargetSelf;
            }

            return !OutOfRange(root, targ, targ.Thing?.OccupiedRect() ?? CellRect.SingleCell(targ.Cell));
        }
    }
}
