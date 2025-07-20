using ForeignerOfUniverse.Utilities;
using HarmonyLib;
using Nebulae.RimWorld.UI;
using Nebulae.RimWorld.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ForeignerOfUniverse.Patches
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.CheckForStateChange))]
    internal static class Pawn_HealthTracker_Patch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> CheckForStateChangeTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = instructions.ToArray();
            var method = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDead));

            bool patched = false;

            for (int i = 0; i < codes.Length; i++)
            {
                var code = codes[i];

                if (!patched && code.opcode == OpCodes.Call && (MethodInfo)code.operand == method)
                {
                    yield return code;  // call ShouldBeDead()

                    code = codes[++i];  // brfalse.s
                    var exit = code.operand;

                    yield return code;

                    var label = il.DefineLabel();

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NaniteGeneUtility), nameof(NaniteGeneUtility.AllowPhoenix)));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Pawn_HealthTracker_Patch), nameof(ActicateNanites)));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Downed)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, exit);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.forceDowned)));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Pawn_HealthTracker), "MakeDowned"));
                    yield return new CodeInstruction(OpCodes.Br, exit);
                    yield return codes[++i].WithLabels(label);  // ldarg.0
                    yield return codes[++i];    // call ShouldBeDeathrestingOrInComa();

                    patched = true;
                }
                else
                {
                    yield return code;
                }
            }

            FOU.DebugLabel.TranspileMessage(patched, typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.CheckForStateChange));
        }


        private static void ActicateNanites(Pawn pawn)
        {
            var hediffs = pawn.health.hediffSet.hediffs;

            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                if (hediffs[i].def == FOUDefOf.FOU_InjuryInducedNanostate)
                {
                    return;
                }
            }

            pawn.health.AddHediff(FOUDefOf.FOU_InjuryInducedNanostate);

            if (PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                TaggedString letterLabel = "FOU.Letters.NaniteActivated.Label".Translate();
                TaggedString letterContent = "FOU.Letters.NaniteActivated.Content".Translate(pawn.Named("PAWN"));

                Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NegativeEvent, pawn);
            }
        }
    }
}
