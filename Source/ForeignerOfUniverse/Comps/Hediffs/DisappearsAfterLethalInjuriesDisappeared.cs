using ForeignerOfUniverse.Utilities;
using RimWorld;
using Verse;

namespace ForeignerOfUniverse.Comps.Hediffs
{
    public sealed class InjuryInducedNanostate : HediffComp
    {
        public override bool CompShouldRemove
        {
            get
            {
                var hediffs = parent.pawn.health.hediffSet.hediffs;

                for (int i = hediffs.Count - 1; i >= 0; i--)
                {
                    var hediff = hediffs[i];

                    if (hediff is Hediff_MissingPart)
                    {
                        var part = hediff.Part;

                        if (part.height is BodyPartHeight.Top
                            || part.height is BodyPartHeight.Middle
                            || part.parent is null)
                        {
                            return false;
                        }
                    }
                    else if (hediff is Hediff_Injury && hediff.Part.parent is null)
                    {
                        return false;
                    }
                }

                return !parent.pawn.health.ShouldBeDeadFromLethalDamageThreshold() 
                    && parent.pawn.health.ShouldBeDeadFromRequiredCapacity() is null;
            }
        }


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            var hediffSet = parent.pawn.health.hediffSet;
            var hediffs = hediffSet.hediffs;

            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                var hediff = hediffs[i];

                if (hediff.def == FOUDefOf.FOU_InjuryInducedFinalNanostate)
                {
                    hediff.PreRemoved();
                    hediffs.RemoveAt(i);
                    hediffSet.DirtyCache();
                    hediff.PostRemoved();
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            if (!parent.pawn.Dead && parent.pawn.OwnNanites())
            {
                parent.pawn.health.AddHediff(FOUDefOf.FOU_InjuryInducedFinalNanostate);
            }
        }
    }
}
