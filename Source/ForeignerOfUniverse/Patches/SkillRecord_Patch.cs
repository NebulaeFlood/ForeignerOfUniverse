using ForeignerOfUniverse.Utilities;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ForeignerOfUniverse.Patches
{
    [HarmonyPriority(int.MaxValue)]
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Interval))]
    public static class SkillRecord_Patch
    {
        [HarmonyPrefix]
        public static bool IntervalPrefix(SkillRecord __instance, Pawn ___pawn)
        {
            if (___pawn.TryGetNanites(out var nanites) && nanites.protocol.NooNet)
            {
                __instance.Learn(8f, ignoreLearnRate: false);
                return false;
            }

            return true;
        }
    }
}
