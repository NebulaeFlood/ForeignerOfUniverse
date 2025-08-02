using ForeignerOfUniverse.Utilities;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ForeignerOfUniverse.Patches
{
    [HarmonyPatch(typeof(AnomalyUtility), nameof(AnomalyUtility.TryDuplicatePawn))]
    internal static class AnomalyUtility_Patch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> TryDuplicatePawnTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            Label originStartPoint = il.DefineLabel();

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NaniteGeneUtility), nameof(NaniteGeneUtility.AnyNaniteGene)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, originStartPoint);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldnull);
            yield return new CodeInstruction(OpCodes.Stind_Ref);
            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
            yield return new CodeInstruction(OpCodes.Ret);
            yield return instructions.First().WithLabels(originStartPoint);

            foreach (var code in instructions.Skip(1))
            {
                yield return code;
            }
        }
    }
}
