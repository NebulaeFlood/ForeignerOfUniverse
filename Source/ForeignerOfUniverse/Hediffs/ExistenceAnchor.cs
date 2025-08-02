using ForeignerOfUniverse.Letters;
using ForeignerOfUniverse.Utilities;
using RimWorld;
using System.Diagnostics;
using Verse;

namespace ForeignerOfUniverse.Hediffs
{
    public sealed class ExistenceAnchor : Hediff
    {
        public override bool Visible => false;


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref letter, "Letter");
        }

        public override void Notify_PawnCorpseDestroyed()
        {
            if (letter != null)
            {
                return;
            }

            if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
            {
                return;
            }

            if (!pawn.OwnNanites())
            {
                return;
            }

            var frames = new StackTrace().GetFrames();
            var targetType = typeof(StartingPawnUtility);

            for (int i = frames.Length - 1; i >= 0; i--)
            {
                if (frames[i].GetMethod().DeclaringType == targetType)
                {
                    return;
                }
            }

            letter = (RespawnPawn)LetterMaker.MakeLetter(
                "FOU.Letters.ExistenceAnchor.Label".Translate(pawn.Named("PAWN")),
                "FOU.Letters.ExistenceAnchor.Content".Translate(pawn.Named("PAWN"), "「████」".Colorize(ColoredText.NameColor)),
                FOUDefOf.FOU_RespawnPawn);
            letter.pawn = pawn;

            Find.LetterStack.ReceiveLetter(letter);
        }

        public override void Tick()
        {
            if (letter != null || !pawn.IsHashIntervalTick(240) || (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer))
            {
                return;
            }

            if (pawn.IsKidnapped())
            {
                letter = (RespawnPawn)LetterMaker.MakeLetter(
                    "FOU.Letters.ExistenceAnchor.Label".Translate(pawn.Named("PAWN")),
                    "FOU.Letters.ExistenceAnchor.Content".Translate(pawn.Named("PAWN"), "「████」".Colorize(ColoredText.NameColor)),
                    FOUDefOf.FOU_RespawnPawn);
                letter.pawn = pawn;

                Find.LetterStack.ReceiveLetter(letter);
            }
        }

        #endregion


        internal RespawnPawn letter;
    }
}
