using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Utilities;
using ForeignerOfUniverse.Views;
using ForeignerOfUniverse.Windows;
using Nebulae.RimWorld.UI.Utilities;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace ForeignerOfUniverse.Comps.AbilityEffects
{
    public sealed class MatterWeave : CompAbilityEffect
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
            if (weaveQueue is null)
            {
                return;
            }

            if (!parent.pawn.TryGetNanites(out var nanites))
            {
                weaveQueue = null;
                return;
            }

            var caravan = parent.pawn.GetCaravan();

            if (caravan is null)
            {
                weaveQueue = null;
                return;
            }

            var node = weaveQueue.First;

            while (node != null)
            {
                var weavingThing = node.Value.Model;
                var weavingThingCount = node.Value.Count;

                caravan.WeaveThing(weavingThing, weavingThingCount);
                nanites.OffsetStore(-(weavingThingCount * ThingInfo.GetMass(weavingThing) / (FOU.Settings.KilogramsPerNanite * 100f)));

                node = node.Next;
            }

            caravan.RecacheInventory();
            Messages.Message("FOU.NaniteAbility.SpawnedToCaravan".Translate(caravan.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                MessageTypeDefOf.PositiveEvent, historical: false);

            weaveQueue = null;

            base.Apply(target);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (weaveQueue is null)
            {
                return;
            }

            if (!parent.pawn.TryGetNanites(out var nanites))
            {
                weaveQueue = null;
                return;
            }

            var map = parent.pawn.MapHeld;

            if (map is null)
            {
                weaveQueue = null;
                return;
            }

            var pos = target.Cell;
            var node = weaveQueue.First;

            if (target.Thing is Pawn pawn)
            {
                while (node != null)
                {
                    var weavingThing = node.Value.Model;
                    var weavingThingCount = node.Value.Count;

                    pawn.WeaveThing(weavingThing, weavingThingCount);
                    nanites.OffsetStore(-(weavingThingCount * ThingInfo.GetMass(weavingThing) / (FOU.Settings.KilogramsPerNanite * 100f)));

                    node = node.Next;
                }

                pawn.inventory.UnloadEverything = true;

                map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(pawn, map), pos, 60);
                FOUDefOf.FOU_TransportExit.PlayOneShot(new TargetInfo(pawn));
            }
            else
            {
                while (node != null)
                {
                    var weavingThing = node.Value.Model;
                    var weavingThingCount = node.Value.Count;

                    map.WeaveThing(pos, weavingThing, weavingThingCount);
                    nanites.OffsetStore(-(weavingThingCount * ThingInfo.GetMass(weavingThing) / (FOU.Settings.KilogramsPerNanite * 100f)));

                    node = node.Next;
                }
            }

            weaveQueue = null;
            base.Apply(target, dest);
        }

        public override bool CanApplyOn(GlobalTargetInfo target) => true;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => true;

        public override Window ConfirmationDialog(GlobalTargetInfo target, Action confirmAction)
        {
            return new ThingWeaveWindow(this, confirmAction);
        }

        public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
        {
            return new ThingWeaveWindow(this, confirmAction);
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (target.Thing is Pawn pawn)
            {
                return "FOU.NaniteAbility.SpawnToPawn".Translate(pawn.Named("PAWN")).Resolve();
            }

            var map = parent.pawn.Map;

            if (map is null)
            {
                return "FOU.NaniteAbility.CannotSpawnTo".Translate();
            }

            if (!target.IsValid
                || target.Cell.GetDoor(map) is Building_Door door && !door.CanPhysicallyPass(parent.pawn)
                || target.Cell.Impassable(map) || !target.Cell.WalkableBy(map, parent.pawn))
            {
                return "FOU.NaniteAbility.CannotSpawnTo".Translate();
            }
            ;

            return "FOU.NaniteAbility.SpawnTo".Translate();
        }

        public override string ExtraTooltipPart() => $"{"FOU.NaniteAbility.KilogramsPerNanite".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor)}: {1f / FOU.Settings.KilogramsPerNanite}/kg";

        public override bool GizmoDisabled(out string reason)
        {
            if (!parent.pawn.OwnNanites())
            {
                reason = "FOU.NaniteAbility.NoFoundationProtocol".Translate(parent.pawn.Named("PAWN"));
                return true;
            }

            reason = string.Empty;
            return false;
        }

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref weavableThings, "WeavableThings", LookMode.Deep);
            Scribe_Collections.Look(ref weavePolicies, "WeavePolicies", LookMode.Deep);

            if (Scribe.mode is LoadSaveMode.PostLoadInit)
            {
                if (weavableThings is null)
                {
                    weavableThings = new HashSet<ThingInfo>();
                }
                else
                {
                    weavableThings = new HashSet<ThingInfo>(weavableThings.Select(ThingInfo.Resolve));
                }

                if (weavePolicies is null)
                {
                    weavePolicies = new List<ThingWeavePolicy>();
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Thing is Pawn)
            {
                return true;
            }

            var map = parent.pawn.Map;

            if (map is null)
            {
                if (throwMessages)
                {
                    Messages.Message("FOU.NaniteAbility.CannotSpawnTo".Translate(),
                        MessageTypeDefOf.RejectInput, historical: false);
                }

                return false;
            }

            if (!target.IsValid
                || target.Cell.GetDoor(map) is Building_Door door && !door.CanPhysicallyPass(parent.pawn)
                || target.Cell.Impassable(map) || !target.Cell.WalkableBy(map, parent.pawn))
            {
                if (throwMessages)
                {
                    Messages.Message("FOU.NaniteAbility.CannotSpawnTo".Translate(),
                        MessageTypeDefOf.RejectInput, historical: false);
                }

                return false;
            }

            return true;
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false) => true;

        #endregion


        //------------------------------------------------------
        //
        //  Internal Fields
        //
        //------------------------------------------------------

        #region Internal Fields

        internal HashSet<ThingInfo> weavableThings = new HashSet<ThingInfo>();
        internal List<ThingWeavePolicy> weavePolicies = new List<ThingWeavePolicy>();
        internal LinkedList<ThingWeavableView> weaveQueue = new LinkedList<ThingWeavableView>();

        #endregion
    }
}
