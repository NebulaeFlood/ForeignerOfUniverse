using Verse;

namespace ForeignerOfUniverse.Comps.Hediffs
{
    public sealed class GetsPermanent : HediffComp_GetsPermanent
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (!parent.pawn.RaceProps.IsFlesh)
            {
                return;
            }

            float severity = parent.Severity;
            float partHealth = parent.Part?.def.GetMaxHealth(parent.pawn) ?? 15f;
            float minSeverityCausePermanent = partHealth / 3f;

            if (severity > minSeverityCausePermanent)
            {
                IsPermanent = true;

                if (severity < minSeverityCausePermanent * 2f)
                {
                    SetPainCategory(PainCategory.MediumPain);
                }
                else
                {
                    SetPainCategory(PainCategory.HighPain);
                }
            }
        }
    }
}
