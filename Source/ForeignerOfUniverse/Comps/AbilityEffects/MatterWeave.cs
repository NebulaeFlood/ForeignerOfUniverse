using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Utilities;
using ForeignerOfUniverse.Views;
using ForeignerOfUniverse.Windows;
using Nebulae.RimWorld.UI.Utilities;
using Nebulae.RimWorld.UI.Windows;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ForeignerOfUniverse.Comps.AbilityEffects
{
    internal sealed class MatterWeave : CompAbilityEffect
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
            if (!weavingThing.Loaded || weavingThingCount < 1 || !parent.pawn.TryGetNanites(out var nanites))
            {
                return;
            }

            WeaveThings(target);

            nanites.OffsetStore(-(weavingThingCount * ThingInfo.GetMass(weavingThing) * FOU.Settings.KilogramsPerNanite * 0.01f));

            weavingThing = ThingInfo.Empty;
            weavingThingCount = 0;

            base.Apply(target, dest);
        }

        public override bool CanApplyOn(GlobalTargetInfo target) => false;

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => true;

        public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
        {
            return new ThingWeaveWindow(this, confirmAction);
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
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

            return "FOU.NaniteAbility.SpawnTo".Translate();
        }

        public override string ExtraTooltipPart() => $"{"FOU.NaniteAbility.KilogramsPerNanite".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor)}: {FOU.Settings.KilogramsPerNanite}/kg";

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
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
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

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false) => false;

        #endregion


        private void WeaveThings(LocalTargetInfo target)
        {
            var map = parent.pawn.Map;
            var pos = target.Cell;

            var weaveInfo = new ThingWeaveInfo(weavingThing);

            for (var i = weavingThingCount; i > 0; i -= weavingThing.DefInfo.Def.stackLimit)
            {
                var thing = weaveInfo.Weave(i);

                GenSpawn.Spawn(thing, pos, map, WipeMode.VanishOrMoveAside);
                map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(thing, map), thing.Position, 60);
                FOUDefOf.FOU_TransportExit.PlayOneShot(new TargetInfo(thing));
            }
        }


        //------------------------------------------------------
        //
        //  Internal Fields
        //
        //------------------------------------------------------

        #region Internal Fields

        internal HashSet<ThingInfo> weavableThings = new HashSet<ThingInfo>();
        internal ThingInfo weavingThing = ThingInfo.Empty;
        internal int weavingThingCount;

        #endregion
    }
}
