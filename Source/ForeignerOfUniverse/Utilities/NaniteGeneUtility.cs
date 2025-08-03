using ForeignerOfUniverse.Genes;
using RimWorld;
using Verse;

namespace ForeignerOfUniverse.Utilities
{
    public static class NaniteGeneUtility
    {
        public static bool AllowAscension(this Pawn pawn)
        {
            return pawn.TryGetNanites(out var nanites) && nanites.protocol.Ascension;
        }

        public static bool AllowNooNet(this Pawn pawn)
        {
            return pawn.TryGetNanites(out var nanites) && nanites.protocol.NooNet;
        }

        public static bool AllowPhoenix(this Pawn pawn)
        {
            return pawn.TryGetNanites(out var nanites) && nanites.protocol.Phoenix;
        }

        public static bool AnyNaniteGene(this Pawn pawn)
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

        public static void OffsetNaniteStore(this Pawn pawn, float value)
        {
            pawn?.genes?.GetFirstGeneOfType<HigherDimensionalNanites>()?.OffsetStore(value);
        }

        public static bool OwnNanites(this Pawn pawn)
        {
            return pawn?.genes?.HasActiveGene(FOUDefOf.FOU_HigherDimensionalNanites) ?? false;
        }

        public static bool TryGetHemogen(this Pawn pawn, out Gene_Hemogen hemogen)
        {
            hemogen = pawn?.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            return hemogen != null;
        }

        public static bool TryGetNanites(this Pawn pawn, out HigherDimensionalNanites nanites)
        {
            nanites = pawn?.genes?.GetFirstGeneOfType<HigherDimensionalNanites>();
            return nanites != null;
        }
    }
}
