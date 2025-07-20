using ForeignerOfUniverse.Utilities;
using HarmonyLib;
using Nebulae.RimWorld.UI;
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
    [HarmonyPatch(typeof(CompDrug), nameof(CompDrug.PostIngested))]
    internal static class CompDrug_Patch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> PostIngestedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            var codes = instructions.ToArray();
            var isFlesh = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsFlesh));

            for ( var i = 0; i < codes.Length; i++)
            {
                var code = codes[i];

                if (!patched && code.opcode == OpCodes.Callvirt && (MethodInfo)code.operand == isFlesh)
                {
                    yield return code;

                    code = codes[++i];   // brfalse
                    var exit = code.operand;

                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NaniteGeneUtility), nameof(NaniteGeneUtility.AllowAscension)));
                    yield return new CodeInstruction(OpCodes.Brtrue, exit);

                    patched = true;
                }
                else
                {
                    yield return code;
                }
            }

            FOU.DebugLabel.TranspileMessage(patched, typeof(CompDrug), nameof(CompDrug.PostIngested));
        }
    }
}
