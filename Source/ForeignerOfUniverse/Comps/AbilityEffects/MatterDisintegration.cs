using ForeignerOfUniverse.Genes;
using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Utilities;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace ForeignerOfUniverse.Comps.AbilityEffects
{
    public sealed class MatterDisintegration : CompAbilityEffect
    {
        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override bool AICanTargetNow(LocalTargetInfo target) => false;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing is Thing thing)
            {
                int cost = CalculateMaterialCost(thing, out var unitsPerMaterial);

                if (cost < thing.stackCount)
                {
                    thing.stackCount -= cost;
                }
                else
                {
                    thing.Kill(new DamageInfo(FOUDefOf.FOU_Disintegration, float.PositiveInfinity));
                }

                parent.pawn.OffsetNaniteStore(cost * unitsPerMaterial * 0.01f);

                if (thing is Corpse || !TryAddWeavableThing(thing, out var exist))
                {
                    Messages.Message("FOU.NaniteAbility.CannotAddWeavableThing".Translate(thing.LabelShortCap.Colorize(ColoredText.NameColor)).Resolve(),
                        MessageTypeDefOf.NegativeEvent, historical: false);
                }
                else
                {
                    if (!exist)
                    {
                        Messages.Message("FOU.NaniteAbility.WeavableThingAdded".Translate(thing.LabelShortCap.Colorize(ColoredText.NameColor), FOUDefOf.FOU_MatterWeave.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                            MessageTypeDefOf.PositiveEvent, historical: false);
                    }
                }

                base.Apply(target, dest);
            }
        }

        public override bool CanApplyOn(GlobalTargetInfo target) => false;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => true;

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (target.Thing is Thing thing)
            {
                return "FOU.NaniteAbility.TargetThingCostInfo".Translate(thing.LabelShort.Colorize(ColoredText.NameColor), CalculateMaterialCost(thing, out _).ToString().Colorize(ColoredText.NameColor)).Resolve();
            }
            else
            {
                return string.Empty;
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!parent.pawn.OwnNanites())
            {
                reason = "FOU.NaniteAbility.NoFoundationProtocol".Translate(parent.pawn.Named("PAWN")).Resolve();
                return true;
            }

            reason = string.Empty;
            return false;
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false) => true;

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false) => false;

        public override string ExtraTooltipPart() => $"{"FOU.NaniteAbility.NanitesPerKilogram".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor)}: {FOU.Settings.NanitesPerKilogram}/kg";

        #endregion


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private int CalculateMaterialCost(Thing material, out float unitsPerMaterial)
        {
            unitsPerMaterial = 0f;
            return parent.pawn.TryGetNanites(out var nanites)
                ? Mathf.Clamp(nanites.CalculateDismantleForMaxGrowth(material, out unitsPerMaterial), 1, material.stackCount)
                : 1;
        }

        private bool TryAddWeavableThing(Thing thing, out bool exist)
        {
            if (parent.pawn.abilities is null)
            {
                exist = false;
                return false;
            }

            var ability = parent.pawn.abilities.GetAbility(FOUDefOf.FOU_MatterWeave);

            if (ability is null)
            {
                exist = false;
                return false;
            }

            var comp = ability.CompOfType<MatterWeave>();

            if (comp is null)
            {
                exist = false;
                return false;
            }

            exist = !comp.weavableThings.Add(new ThingInfo(thing));
            return true;
        }

        #endregion
    }
}
