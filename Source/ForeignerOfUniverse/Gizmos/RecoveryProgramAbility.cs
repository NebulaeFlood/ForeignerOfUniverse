using ForeignerOfUniverse.Hediffs;
using ForeignerOfUniverse.Utilities;
using Nebulae.RimWorld.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ForeignerOfUniverse.Gizmos
{
    internal sealed class RecoveryProgramAbility : Command
    {
        //------------------------------------------------------
        //
        //  Public Static Fields
        //
        //------------------------------------------------------

        #region Public Static Fields

        public static readonly CachedTexture Icon = new CachedTexture("UI/Icons/Abilities/FOU_ActivateRecoveryProgram");
        public static readonly CachedTexture TeleportIcon = new CachedTexture("UI/Icons/Abilities/FOU_Transport");

        #endregion


        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties

        public override Texture2D BGTexture => Command_Ability.BGTex;

        public override Texture2D BGTextureShrunk => Command_Ability.BGTexShrunk;

        #endregion


        internal RecoveryProgramAbility(RecoveryProgram program)
        {
            defaultLabel = "FOU.NaniteAbility.ActivateRecoveryProgram.Label".Translate().CapitalizeFirst();
            defaultDesc = $"{"FOU.NaniteAbility.ActivateRecoveryProgram.Description".Translate(program.pawn.Named("PAWN")).Resolve()}\n\n - {"FOU.Protocols.Phoenix.Label".Translate().Colorize(ColoredText.NameColor)}";
            icon = Icon.Texture;

            _program = program;

            if (!program.pawn.OwnNanites())
            {
                Disable("FOU.NaniteAbility.InsufficientPrivileges".Translate(program.pawn.Named("PAWN")));
            }
        }


        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            TargetUtility.TargetLocal(RecoverAndTeleport, TargetingParameters.ForCell(), TeleportIcon.Texture, validator: ValidateTarget);
        }


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private async void RecoverAndTeleport(LocalTargetInfo target)
        {
            int version = ++_version;

            await AwaitUtility.WaitForUnpauseAsync();

            if (_version != version)
            {
                return;
            }

            var pawn = _program.pawn;

            if (pawn.Dead && !ResurrectionUtility.TryResurrect(pawn))
            {
                FOU.DebugLabel.Warning($"Faild to resurrect pawn: {pawn}");
            }

            var map = pawn.Map;
            var targetPos = target.Cell;

            map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(pawn, map), pawn.Position, 60);
            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(SoundInfo.InMap(pawn));

            pawn.Position = targetPos;
            pawn.Notify_Teleported();
            NaniteRecoverUtility.Recover(pawn, pawn);

            map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(targetPos, map), targetPos, 60);
            FOUDefOf.FOU_TransportExit.PlayOneShot(SoundInfo.InMap(target.ToTargetInfo(map)));

            if ((pawn.Faction == Faction.OfPlayer || pawn.IsPlayerControlled) && pawn.Position.Fogged(map))
            {
                FloodFillerFog.FloodUnfog(targetPos, map);
            }

            _program.StartTick();
        }

        private bool ValidateTarget(LocalTargetInfo target, out TaggedString message)
        {
            var map = _program.pawn.MapHeld;

            if (map is null)
            {
                message = "FOU.NaniteAbility.CannotTeleportTo".Translate(_program.pawn.Named("PAWN"));

                Find.Targeter.StopTargeting();
                return false;
            }

            if (!target.IsValid
                || target.Cell.GetDoor(map) is Building_Door door && !door.CanPhysicallyPass(_program.pawn)
                || target.Cell.Impassable(map) || !target.Cell.WalkableBy(map, _program.pawn))
            {
                message = "FOU.NaniteAbility.CannotTeleportTo".Translate(_program.pawn.Named("PAWN"));
                return false;
            }

            message = "FOU.NaniteAbility.TeleportTo".Translate(_program.pawn.Named("PAWN"));
            return true;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly RecoveryProgram _program;
        private int _version;

        #endregion
    }
}
