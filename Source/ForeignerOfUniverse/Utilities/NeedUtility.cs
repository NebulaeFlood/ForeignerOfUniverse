using RimWorld;
using Verse;

namespace ForeignerOfUniverse.Utilities
{
    internal static class NeedUtility
    {
        internal static T GetNeed<T>(this Pawn pawn) where T : Need
        {
            return pawn.needs?.TryGetNeed<T>();
        }

        internal static bool IsNeedSatisfied<T>(this Pawn pawn) where T : Need
        {
            if (pawn.needs?.TryGetNeed<T>() is Need need)
            {
                return need.CurLevel >= need.MaxLevel;
            }

            return true;
        }

        internal static void SatisfyNeed<T>(this Pawn pawn) where T : Need
        {
            if (pawn.needs?.TryGetNeed<T>() is Need need)
            {
                need.CurLevel = need.MaxLevel;
            }
        }
    }
}
