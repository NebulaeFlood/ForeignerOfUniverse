using RimWorld;
using Verse;

namespace ForeignerOfUniverse
{
    [DefOf]
    public static class FOUDefOf
    {
        public static readonly AbilityDef FOU_MatterDisintegration;
        public static readonly AbilityDef FOU_MatterWeave;

        public static readonly DamageDef FOU_Disintegration;

        public static readonly GeneDef FOU_HigherDimensionalNanites;
        public static readonly GeneDef FOU_NanoSynapse;
        public static readonly GeneDef FOU_MatterManipulation;
        public static readonly GeneDef FOU_PhysicalAscension;

        public static readonly LetterDef FOU_RespawnPawn;

        public static readonly HediffDef FOU_PsychicShild;
        public static readonly HediffDef FOU_InjuryInducedFinalNanostate;
        public static readonly HediffDef FOU_InjuryInducedNanostate;
        public static readonly HediffDef FOU_ExistenceAnchor;
        public static readonly HediffDef FOU_RecoveryProgram;
        public static readonly HediffDef FOU_Regenerating;

        public static readonly SoundDef FOU_TransportExit;

        public static readonly ThingDef Mote_FOU_MatterFloatAway;
        public static readonly ThingDef Mote_FOU_NanitesFlash;


        static FOUDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FOUDefOf));
        }
    }
}
