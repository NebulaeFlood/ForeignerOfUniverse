using ForeignerOfUniverse.Utilities;
using ForeignerOfUniverse.Views;
using ForeignerOfUniverse.Windows;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ForeignerOfUniverse.Comps.AbilityEffects
{
    public sealed class MatterDisintegration : CompAbilityEffect
    {
        public override bool ShouldHideGizmo => FOU.Settings.HideGizmoWhenMultiSelected && Find.Selector.SelectedPawns.Count != 1;


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override bool AICanTargetNow(LocalTargetInfo target) => false;

        public override void Apply(GlobalTargetInfo target)
        {
            if (disintegrateQueue is null)
            {
                return;
            }

            var caravan = parent.pawn.GetCaravan();

            if (caravan is null)
            {
                disintegrateQueue = null;
                return;
            }

            var node = disintegrateQueue.First;

            while (node != null)
            {
                var thing = node.Value.Model;
                int cost = node.Value.Count;
                var unitsPerMaterial = thing.CalculateNanitePerMaterial();

                parent.pawn.OffsetNaniteStore(cost * unitsPerMaterial * 0.01f);
                parent.pawn.RecordWeavableThing(thing);
                thing.Disintegrate(cost);

                node = node.Next;
            }

            disintegrateQueue = null;

            Messages.Message("FOU.NaniteAbility.DisintegratedInventory".Translate(parent.pawn.Named("PAWN"), caravan.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                MessageTypeDefOf.NeutralEvent, historical: false);
            caravan.RecacheInventory();
            base.Apply(target);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Thing is Pawn pawn)
            {
                if (disintegrateQueue is null)
                {
                    return;
                }

                var node = disintegrateQueue.First;

                while (node != null)
                {
                    var thing = node.Value.Model;
                    int cost = node.Value.Count;
                    var unitsPerMaterial = thing.CalculateNanitePerMaterial();

                    parent.pawn.OffsetNaniteStore(cost * unitsPerMaterial * 0.01f);
                    parent.pawn.RecordWeavableThing(thing);
                    thing.Disintegrate(cost);
                    base.Apply(target, dest);

                    node = node.Next;
                }

                Messages.Message("FOU.NaniteAbility.DisintegratedInventory".Translate(parent.pawn.Named("PAWN"), pawn.LabelShortCap.Colorize(ColoredText.NameColor)).Resolve(),
                    MessageTypeDefOf.NeutralEvent, historical: false);
                disintegrateQueue = null;
            }
            else
            {
                var thing = target.Thing;
                int cost = parent.pawn.CalculateMaterialCostByReplication(thing, out var unitsPerMaterial);

                parent.pawn.OffsetNaniteStore(cost * unitsPerMaterial * 0.01f);
                parent.pawn.RecordWeavableThing(thing);
                thing.Disintegrate(cost);
                base.Apply(target, dest);
            }
        }

        public override bool CanApplyOn(GlobalTargetInfo target) => true;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => true;

        public override Window ConfirmationDialog(GlobalTargetInfo target, Action confirmAction)
        {
            var caravan = parent.pawn.GetCaravan();
            return new ThingDisintegrateWindow(this, caravan.AllThings.Where(MatterManipulateUtility.AllowDisintegrate), caravan.LabelCap, confirmAction);
        }

        public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
        {
            if (target.TryGetPawn(out var pawn))
            {
                return new ThingDisintegrateWindow(this, pawn.EquippedWornOrInventoryThings.Where(MatterManipulateUtility.AllowDisintegrate), pawn.LabelShortCap, confirmAction);
            }

            return null;
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (target.Thing is Pawn pawn)
            {
                return "FOU.NaniteAbility.DisintegratePawnInventory".Translate(pawn.Named("PAWN")).Resolve();
            }
            else if (target.Thing is Thing thing)
            {
                return "FOU.NaniteAbility.TargetThingCostInfo".Translate(thing.LabelShort.Colorize(ColoredText.NameColor), parent.pawn.CalculateMaterialCostByReplication(thing, out _).ToString().Colorize(ColoredText.NameColor)).Resolve();
            }
            else
            {
                return string.Empty;
            }
        }

        public override string ExtraTooltipPart() => $"{"FOU.NaniteAbility.NanitesPerKilogram".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor)}: {FOU.Settings.NanitesPerKilogram}/kg";

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

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false) => true;

        #endregion


        internal LinkedList<ThingDisintegratableView> disintegrateQueue;
    }
}
