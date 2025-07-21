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
                RecordWeavableThing(thing);
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

        private void RecordWeavableThing(Thing thing)
        {
            if (parent.pawn.abilities is null)
            {
                return;
            }

            var ability = parent.pawn.abilities.GetAbility(FOUDefOf.FOU_MatterWeave);

            if (ability is null)
            {
                return;
            }

            var comp = ability.CompOfType<MatterWeave>();

            if (comp is null)
            {
                return;
            }

            var info = new ThingInfo(thing);

            if (thing is MinifiedThing || thing is MinifiedTree || thing is Corpse)
            {
                Messages.Message("FOU.NaniteAbility.CannotAddWeavableThing".Translate(info.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                    MessageTypeDefOf.NegativeEvent, historical: false);
                return;
            }

            if (comp.weavableThings.Add(info))
            {
                Messages.Message("FOU.NaniteAbility.WeavableThingAdded".Translate(info.LabelCap.Colorize(ColoredText.NameColor), FOUDefOf.FOU_MatterWeave.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                    MessageTypeDefOf.PositiveEvent, historical: false);
            }
        }

        #endregion
    }
}
