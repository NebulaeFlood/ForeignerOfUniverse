using ForeignerOfUniverse.Genes;
using HarmonyLib;
using RimWorld;

namespace ForeignerOfUniverse.Patches
{
    [HarmonyPatch(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.AddictionChanceFactor))]
    public static class Pawn_GeneTracker_Patch
    {
        [HarmonyPrefix]
        public static bool AddictionChanceFactorPrefix(Pawn_GeneTracker __instance, ref float __result)
        {
            var nanite = __instance.GetFirstGeneOfType<HigherDimensionalNanites>();

            if (nanite is null || !nanite.protocol.Ascension)
            {
                return true;
            }

            __result = 0f;
            return false;
        }
    }
}
