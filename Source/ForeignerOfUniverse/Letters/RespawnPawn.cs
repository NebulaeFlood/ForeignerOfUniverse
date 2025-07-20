using ForeignerOfUniverse.Gizmos;
using ForeignerOfUniverse.Hediffs;
using ForeignerOfUniverse.Utilities;
using Nebulae.RimWorld.Utilities;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace ForeignerOfUniverse.Letters
{
    public sealed class RespawnPawn : ChoiceLetter
    {
        //------------------------------------------------------
        //
        //  Pubilc Properties
        //
        //------------------------------------------------------

        #region Pubilc Properties

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                DiaOption postponeOption = new DiaOption("PostponeLetter".Translate())
                {
                    resolveTree = true
                };

                if (_used || LastTickBeforeTimeout)
                {
                    postponeOption.Disable(string.Empty);
                }

                yield return postponeOption;

                DiaOption acceptOption = new DiaOption("AcceptButton".Translate())
                {
                    action = Accept,
                    resolveTree = true
                };

                if (_used || pawn is null || LastTickBeforeTimeout)
                {
                    acceptOption.Disable(string.Empty);
                }

                yield return acceptOption;
            }
        }

        #endregion


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref pawn, "Pawn", saveDestroyedThings: true);
        }


        private static bool FromColonist(Pawn pawn)
        {
            return pawn.IsColonist && pawn.HostFaction == Faction.OfPlayer;
        }


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void Accept() => TargetUtility.TargetWorld(OnMapSelected, RecoveryProgramAbility.TeleportIcon.Texture, validator: ValidateTarget);

        private void OnMapSelected(GlobalTargetInfo target)
        {
            if (target.WorldObject is MapParent mapParent && mapParent.Map is Map map)
            {
                _targetMap = map;
                TargetUtility.TargetLocal(OnPosSelected, TargetingParameters.ForCell(), RecoveryProgramAbility.TeleportIcon.Texture, validator: ValidateTarget);
            }
        }

        private async void OnPosSelected(LocalTargetInfo target)
        {
            int version = ++_version;

            await AwaitUtility.WaitForUnpauseAsync();

            if (_version != version)
            {
                return;
            }

            var factions = Find.FactionManager.AllFactionsListForReading;

            for (int i = factions.Count - 1; i >= 0; i--)
            {
                if (factions[i].kidnapped.KidnappedPawnsListForReading.Contains(pawn))
                {
                    factions[i].kidnapped.RemoveKidnappedPawn(pawn);
                    break;
                }
            }

            if (pawn.Dead && !ResurrectionUtility.TryResurrect(pawn))
            {
                FOU.DebugLabel.Warning($"Faild to resurrect pawn: {pawn}");
            }

            var targetPos = target.Cell;

            Find.LetterStack.RemoveLetter(this);
            _targetMap.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(targetPos, _targetMap), targetPos, 60);
            FOUDefOf.FOU_TransportExit.PlayOneShot(target.ToTargetInfo(_targetMap));
            GenSpawn.Spawn(pawn, targetPos, _targetMap);
            NaniteRecoverUtility.Recover(pawn, pawn);
            pawn.Notify_Teleported();
            
            if (pawn.health.hediffSet.GetFirstHediffOfDef(FOUDefOf.FOU_ExistenceAnchor) is ExistenceAnchor anchor)
            {
                anchor.letter = null;
            }

            _used = true;
            _targetMap = null;
        }

        private bool ValidateTarget(GlobalTargetInfo target, out TaggedString message)
        {
            if (target.WorldObject is MapParent mapParent && mapParent.Map is Map map && (mapParent.Faction == Faction.OfPlayer || map.mapPawns.AllPawnsSpawned.Any(FromColonist)))
            {
                message = "FOU.NaniteAbility.TeleportTo".Translate(pawn.Named("PAWN"));
                return true;
            }
            else
            {
                message = "FOU.NaniteAbility.CannotTeleportTo".Translate(pawn.Named("PAWN"));
                return false;
            }
        }

        private bool ValidateTarget(LocalTargetInfo target, out TaggedString message)
        {
            if (_targetMap is null)
            {
                message = "FOU.NaniteAbility.CannotTeleportTo".Translate(pawn.Named("PAWN"));

                Find.Targeter.StopTargeting();
                return false;
            }

            if (!target.IsValid
                || target.Cell.GetDoor(_targetMap) is Building_Door door && !door.CanPhysicallyPass(pawn)
                || target.Cell.Impassable(_targetMap) || !target.Cell.WalkableBy(_targetMap, pawn))
            {
                message = "FOU.NaniteAbility.CannotTeleportTo".Translate(pawn.Named("PAWN"));
                return false;
            }

            message = "FOU.NaniteAbility.TeleportTo".Translate(pawn.Named("PAWN"));
            return true;
        }

        #endregion


        internal Pawn pawn;


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private bool _used;
        private Map _targetMap;
        private int _version = 0;

        #endregion
    }
}
