using ForeignerOfUniverse.Genes;
using ForeignerOfUniverse.Utilities;
using HarmonyLib;
using Nebulae.RimWorld.Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace ForeignerOfUniverse.Comps.AbilityEffects
{
    public abstract class NaniteCost : CompAbilityEffect
    {
        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties

        public Properties_NaniteCost CostProps => (Properties_NaniteCost)props;

        public float Cost
        {
            get => _cost;
            set
            {
                value *= 0.01f;

                if (_cost != value)
                {
                    _cost = value;
                    CostProps.nanitesCost = value;

                    if (AccessTools.Field(typeof(AbilityDef), "cachedTooltip") is FieldInfo field)
                    {
                        field.SetValue(parent.def, null);
                    }
                    else
                    {
                        FOU.DebugLabel.Warning("Can not update cached tooltip for ability named: " + parent.def.label);
                    }
                }
            }
        }

        #endregion


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return parent.pawn.TryGetNanites(out var nanites) && nanites.Value > _cost;
        }

        public override void PostApplied(List<LocalTargetInfo> targets, Map map)
        {
            parent.pawn.OffsetNaniteStore(-_cost);
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!parent.pawn.TryGetNanites(out var nanites))
            {
                reason = "FOU.NaniteAbility.InsufficientPrivileges".Translate(parent.pawn.Named("PAWN")).Resolve();
                return true;
            }

            if (nanites.Value < _cost)
            {
                reason = "FOU.NaniteAbility.NoEnoughStorage".Translate(parent.pawn.Named("PAWN")).Resolve();
                return true;
            }

            var jobs = parent.pawn.jobs;

            if (jobs is null)
            {
                reason = string.Empty;
                return false;
            }

            float totalCost = _cost;

            if (jobs.curJob?.verbToUse is Verb_CastAbility verb && verb.ability.CompOfType<NaniteCost>() is NaniteCost naniteCost)
            {
                totalCost += naniteCost._cost;

                if (nanites.Value < totalCost)
                {
                    reason = "FOU.NaniteAbility.NoEnoughStorage".Translate(parent.pawn.Named("PAWN")).Resolve();
                    return true;
                }
            }

            if (jobs.jobQueue is JobQueue queuedJobs)
            {
                for (int i = 0; i < queuedJobs.Count; i++)
                {
                    if (queuedJobs[i].job.verbToUse is Verb_CastAbility abilityVerb && abilityVerb.ability.CompOfType<NaniteCost>() is NaniteCost comp)
                    {
                        totalCost += comp._cost;

                        if (nanites.Value < totalCost)
                        {
                            reason = "FOU.NaniteAbility.NoEnoughStorage".Translate(parent.pawn.Named("PAWN")).Resolve();
                            return true;
                        }
                    }
                }
            }

            reason = string.Empty;
            return false;
        }

        #endregion


        private float _cost;
    }


    public class Properties_NaniteCost : CompProperties_AbilityEffect
    {
        public float nanitesCost;

        public Properties_NaniteCost()
        {
            compClass = typeof(NaniteCost);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return "FOU.NaniteAbility.Cost".Translate(nanitesCost * 100f);
        }
    }
}
