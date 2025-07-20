using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ForeignerOfUniverse.Utilities
{
    public static class NaniteRecoverUtility
    {
        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public static bool CanRecover(Pawn target)
        {
            return target.InMentalState
                || PrisonBreakUtility.IsPrisonBreaking(target)
                || !target.IsNeedSatisfied<Need_Food>()
                || !target.IsNeedSatisfied<Need_Rest>()
                || !target.IsNeedSatisfied<Need_MechEnergy>()
                || !target.IsNeedSatisfied<Need_Deathrest>()
                || !target.IsNeedSatisfied<Need_KillThirst>()
                || (target.TryGetHemogen(out var hemogen) && hemogen.Value < hemogen.Max)
                || (ModsConfig.RoyaltyActive && target.psychicEntropy?.EntropyValue > 0f)
                || (ModsConfig.AnomalyActive && target.IsMutant)
                || target.health.hediffSet.hediffs.Exists(ShouldRemove)
                || target.EquippedWornOrInventoryThings.Any(ShouldRestore);
        }

        public static void Recover(Pawn target, Pawn caster)
        {
            if (target is null)
            {
                return;
            }

            if (IsDeathresting(target, out var deathrest))
            {
                deathrest.deathrestTicks = deathrest.MinDeathrestTicks;
                deathrest.Wake();
            }

            if (target.TryGetHemogen(out var hemogen))
            {
                hemogen.ValuePercent = 1f;
            }

            target.psychicEntropy?.RemoveAllEntropy();
            target.MentalState?.RecoverFromState();

            if (ModsConfig.AnomalyActive && caster != null)
            {
                RecoverAnomalyHediffs(target, caster);
            }
            else
            {
                RecoverCommonHediffs(target);
            }

            foreach (var item in target.EquippedWornOrInventoryThings)
            {
                if (item.def.useHitPoints)
                {
                    item.HitPoints = item.MaxHitPoints;
                }
            }

            target.SatisfyNeed<Need_Food>();
            target.SatisfyNeed<Need_Rest>();
            target.SatisfyNeed<Need_MechEnergy>();
            target.SatisfyNeed<Need_Deathrest>();
            target.SatisfyNeed<Need_KillThirst>();
        }

        public static void Recover(Thing thing, Pawn caster)
        {
            if (thing.def.useHitPoints)
            {
                thing.HitPoints = thing.MaxHitPoints;
            }

            if (thing.TryGetComp<CompRottable>() is CompRottable rottable)
            {
                rottable.RotProgress = 0f;
            }

            if (thing.TryGetComp<CompRefuelable>() is CompRefuelable refuelable)
            {
                refuelable.Refuel(float.PositiveInfinity);
            }

            if (thing is Corpse corpse)
            {
                Pawn pawn = corpse.InnerPawn;

                if (ResurrectionUtility.TryResurrect(pawn))
                {
                    Recover(pawn, caster);
                }
            }
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Static Methods
        //
        //------------------------------------------------------

        #region Private Static Methods

        private static bool IsDeathresting(Pawn target, out Gene_Deathrest deathrest)
        {
            if (target.health.hediffSet.HasHediff(HediffDefOf.Deathrest))
            {
                deathrest = target.genes?.GetFirstGeneOfType<Gene_Deathrest>();
                return deathrest != null;
            }

            deathrest = null;
            return false;
        }

        private static bool ShouldRemove(Hediff hediff)
        {
            return hediff.def.isBad
                || hediff.def.isInfection
                || hediff.def.tendable
                || hediff is Hediff_Injury
                || hediff.def == HediffDefOf.Inhumanized
                || hediff.def == HediffDefOf.MetalhorrorImplant;
        }

        private static bool ShouldRestore(Thing thing)
        {
            return thing.def.useHitPoints && thing.HitPoints < thing.MaxHitPoints;
        }

        private static void RecoverAnomalyHediffs(Pawn target, Pawn caster)
        {
            if (target.mutant?.HasTurned ?? false)
            {
                target.mutant.Revert();

                TaggedString letterLabel = "FOU.Letters.NaniteRevertMutant.Label".Translate();
                TaggedString letterContent = "FOU.Letters.NaniteRevertMutant.Content".Translate(target.Named("TARGET"), caster.Named("CASTER"), string.Empty);

                Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.PositiveEvent, target);
            }

            var hediffs = target.health.hediffSet.hediffs;

            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                var hediff = hediffs[i];

                if (hediff.def == HediffDefOf.MetalhorrorImplant)
                {
                    hediff.PreRemoved();
                    hediffs.RemoveAt(i);
                    hediff.PostRemoved();
                    target.health.Notify_HediffChanged(hediff);

                    TaggedString letterLabel = "FOU.Letters.MetalhorrorImplantRemoved.Label".Translate();
                    TaggedString letterContent = "FOU.Letters.MetalhorrorImplantRemoved.Content".Translate(target.Named("TARGET"), caster.Named("CASTER"));

                    Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.PositiveEvent, target);
                }
                else if (hediff.def.isBad || hediff.def.isInfection || hediff.def.tendable || hediff.def == HediffDefOf.Anesthetic || hediff.def == HediffDefOf.Inhumanized || hediff is Hediff_Injury)
                {
                    hediff.PreRemoved();
                    hediffs.RemoveAt(i);
                    hediff.PostRemoved();
                    target.health.Notify_HediffChanged(hediff);
                }
            }

            target.Drawer.renderer.SetAllGraphicsDirty();
        }

        private static void RecoverCommonHediffs(Pawn target)
        {
            var hediffs = target.health.hediffSet.hediffs;

            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                var hediff = hediffs[i];

                if (hediff.def.isBad || hediff.def.isInfection || hediff.def.tendable || hediff.def == HediffDefOf.Anesthetic || hediff is Hediff_Injury)
                {
                    hediff.PreRemoved();
                    hediffs.RemoveAt(i);
                    hediff.PostRemoved();
                    target.health.Notify_HediffChanged(hediff);
                }
            }

            target.Drawer.renderer.SetAllGraphicsDirty();
        }

        #endregion
    }
}
