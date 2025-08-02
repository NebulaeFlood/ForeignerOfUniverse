using ForeignerOfUniverse.Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ForeignerOfUniverse.Models
{
    internal enum NaniteState
    {
        Normal,
        Subside,
        Activate
    }


    internal readonly struct NaniteCheckUp
    {
        public readonly LinkedList<Hediff> ImmunizedDiseases;

        public readonly IEnumerable<Hediff_Injury> Injuries;
        public readonly IEnumerable<Hediff_MissingPart> MissingParts;

        public readonly bool AnyBloodLoss;
        public readonly Hediff BloodLoss;

        public readonly bool AnyToxic;
        public readonly Hediff ToxicBuildup;

        public readonly NaniteState NaniteState;

        public readonly bool ShouldEnforceAnchor;
        public readonly bool ShouldEnforcePhoenix;
        public readonly bool ShouldEnforcePsychicShild;


        internal NaniteCheckUp(NaniteProtocol protocol, List<Hediff> hediffs)
        {
            AnyBloodLoss = false;
            BloodLoss = null;

            AnyToxic = false;
            ToxicBuildup = null;

            ShouldEnforceAnchor = true;
            ShouldEnforcePhoenix = protocol.Phoenix;
            ShouldEnforcePsychicShild = protocol.NooNet && protocol.PsychicShildOpen;

            ImmunizedDiseases = new LinkedList<Hediff>();

            NaniteState = NaniteState.Normal;

            bool anyActivateNanite = false;

            var addedParts = new HashSet<BodyPartRecord>();
            var injuries = InjuryList;
            var missingParts = MissingPartList;

            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                var hediff = hediffs[i];

                if (protocol.Ascension && NaniteImmunityUtility.AscensionImmunizedHediffDefs.Contains(hediff.def))
                {
                    ImmunizedDiseases.AddLast(hediff);
                    continue;
                }
                else if (protocol.NooNet && NaniteImmunityUtility.NooNetImmunizedHediffDefs.Contains(hediff.def))
                {
                    ImmunizedDiseases.AddLast(hediff);
                    continue;
                }
                else if (protocol.Phoenix && NaniteImmunityUtility.PhoenixImmunizedHediffDefs.Contains(hediff.def))
                {
                    ImmunizedDiseases.AddLast(hediff);
                    continue;
                }


                if (ShouldEnforceAnchor && ReferenceEquals(protocol.Anchor, hediff))
                {
                    ShouldEnforceAnchor = false;
                    continue;
                }
                else if (ShouldEnforcePhoenix && ReferenceEquals(protocol.RecoveryProgram, hediff))
                {
                    ShouldEnforcePhoenix = false;
                    continue;
                }
                else if (ShouldEnforcePsychicShild && ReferenceEquals(protocol.PsychicShild, hediff))
                {
                    ShouldEnforcePsychicShild = false;
                    continue;
                }

                if (!protocol.Phoenix)
                {
                    if (!ShouldEnforceAnchor && !ShouldEnforcePsychicShild)
                    {
                        break;
                    }

                    continue;
                }

                if (hediff is Hediff_AddedPart addedPart)
                {
                    addedParts.Add(addedPart.Part);
                    continue;
                }
                else if (hediff is Hediff_Injury injury)
                {
                    injuries.AddFirst(injury);
                    continue;
                }
                else if (hediff is Hediff_MissingPart missingPart)
                {
                    missingParts.AddFirst(missingPart);
                    continue;
                }
                else if (!AnyBloodLoss && hediff.def == HediffDefOf.BloodLoss)
                {
                    AnyBloodLoss = true;
                    BloodLoss = hediff;
                    continue;
                }
                else if (!AnyToxic && hediff.def == HediffDefOf.ToxicBuildup)
                {
                    AnyToxic = true;
                    ToxicBuildup = hediff;
                    continue;
                }

                if (anyActivateNanite)
                {
                    continue;
                }
                else if (hediff.def == FOUDefOf.FOU_InjuryInducedNanostate)
                {
                    anyActivateNanite = true;
                    NaniteState = NaniteState.Activate;
                }
                else if (hediff.def == FOUDefOf.FOU_InjuryInducedFinalNanostate)
                {
                    anyActivateNanite = true;
                    NaniteState = NaniteState.Subside;
                }
            }

            bool Renewable(Hediff_MissingPart hediff)
            {
                var part = hediff.Part.parent;
                return !addedParts.Contains(part) && !Contains(missingParts, part);
            }

            Injuries = injuries.Count > 0 ? injuries.OrderBy(GetOrder) : Enumerable.Empty<Hediff_Injury>();
            MissingParts = missingParts.Count > 0 ? missingParts.Where(Renewable).OrderBy(GetOrder) : Enumerable.Empty<Hediff_MissingPart>();
        }


        public void Purge()
        {
            InjuryList.Clear();
            MissingPartList.Clear();
        }


        //------------------------------------------------------
        //
        //  Private Static Methods
        //
        //------------------------------------------------------

        #region Private Static Methods

        private static bool Contains(LinkedList<Hediff_MissingPart> hediffs, BodyPartRecord part)
        {
            var node = hediffs.Last;

            while (node != null)
            {
                if (node.Value.Part == part)
                {
                    return true;
                }

                node = node.Previous;
            }

            return false;
        }

        private static int GetOrder(Hediff hediff)
        {
            var part = hediff.Part;
            return (int)part.depth + (int)part.height;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Static Fields
        //
        //------------------------------------------------------

        #region Private Static Fields

        private static readonly LinkedList<Hediff_Injury> InjuryList = new LinkedList<Hediff_Injury>();
        private static readonly LinkedList<Hediff_MissingPart> MissingPartList = new LinkedList<Hediff_MissingPart>();

        #endregion
    }
}
