using HarmonyLib;
using Verse;
using Verse.AI;

namespace ForeignerOfUniverse.Patches
{
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    public static class MentalStateHandler_Patch
    {
        [HarmonyPrefix]
        public static bool TryStartMentalStatePrefix(Pawn ___pawn, ref bool __result)
        {
            if (___pawn.story is null || ___pawn.story.traits is null)
            {
                return true;
            }

            if (___pawn.story.traits.HasTrait(FOUDefOf.FOU_RealityGamer))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
