using ForeignerOfUniverse.Genes;
using RimWorld;
using Verse;

namespace ForeignerOfUniverse.Utilities
{
    internal static class NaniteGeneUtility
    {
        internal static bool AllowAscension(this Pawn pawn)
        {
            return pawn.TryGetNanites(out var nanites) && nanites.protocol.Ascension;
        }

        internal static bool AllowPhoenix(this Pawn pawn)
        {
            return pawn.TryGetNanites(out var nanites) && nanites.protocol.Phoenix;
        }

        internal static bool AnyNaniteGene(this Pawn pawn)
        {
            if (pawn?.genes is null)
            {
                return false;
            }

            var genes = pawn.genes.GenesListForReading;

            for (int i = genes.Count - 1; i >= 0; i--)
            {
                var gene = genes[i];

                if (gene.def == FOUDefOf.FOU_HigherDimensionalNanites
                    || gene.def == FOUDefOf.FOU_MatterManipulation
                    || gene.def == FOUDefOf.FOU_NanoSynapse
                    || gene.def == FOUDefOf.FOU_PhysicalAscension)
                {
                    return true;
                }
            }

            return false;
        }

        internal static void OffsetNaniteStore(this Pawn pawn, float value)
        {
            pawn?.genes?.GetFirstGeneOfType<HigherDimensionalNanites>()?.OffsetStore(value);
        }

        internal static bool OwnNanites(this Pawn pawn)
        {
            return pawn?.genes?.HasActiveGene(FOUDefOf.FOU_HigherDimensionalNanites) ?? false;
        }

        internal static bool TryGetHemogen(this Pawn pawn, out Gene_Hemogen hemogen)
        {
            hemogen = pawn?.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            return hemogen != null;
        }

        internal static bool TryGetNanites(this Pawn pawn, out HigherDimensionalNanites nanites)
        {
            nanites = pawn?.genes?.GetFirstGeneOfType<HigherDimensionalNanites>();
            return nanites != null;
        }
    }
}
