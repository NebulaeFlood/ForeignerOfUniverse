using ForeignerOfUniverse.Genes;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignerOfUniverse.Patches
{
    [HarmonyPatch(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.AddictionChanceFactor))]
    internal static class Pawn_GeneTracker_Patch
    {
        [HarmonyPrefix]
        internal static bool AddictionChanceFactorPrefix(Pawn_GeneTracker __instance, ref float __result)
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
