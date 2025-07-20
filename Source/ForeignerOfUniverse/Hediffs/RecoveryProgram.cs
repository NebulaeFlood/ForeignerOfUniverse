using ForeignerOfUniverse.Gizmos;
using ForeignerOfUniverse.Letters;
using ForeignerOfUniverse.Utilities;
using Nebulae.RimWorld.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ForeignerOfUniverse.Hediffs
{
    public sealed class RecoveryProgram : Hediff, IClock
    {
        public override bool Visible => false;


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _cooldownTicks, "AbilityCooldownLeft", 0);

            if (Scribe.mode is LoadSaveMode.PostLoadInit)
            {
                FOU.Settings.Saved += OnSettingSaved;

                if (_cooldownTicks > 0)
                {
                    this.StartTick();
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (pawn.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            if (pawn.Downed || pawn.Dead)
            {
                if (_gizmo is null)
                {
                    _gizmo = new RecoveryProgramAbility(this);
                }

                yield return _gizmo;

                if (DebugSettings.ShowDevGizmos && _cooldownTicks > 0)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Reset Cooldown",
                        action = delegate
                        {
                            this.StopTick();
                        }
                    };
                }
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            FOU.Settings.Saved += OnSettingSaved;
        }

        #endregion


        private void OnSettingSaved(FOUSettings sender, EventArgs args)
        {
            if (_cooldownTicks > 0)
            {
                _cooldownTicks = Mathf.RoundToInt(sender.RecoveryProgramCooldown
                    * ((float)_defaultCooldownTicks / sender.RecoveryProgramCooldown).Clamp(0f, 1f));

                _gizmo.disabledReason = "AbilityOnCooldown".Translate(_cooldownTicks.ToStringTicksToPeriod()).Resolve();
            }

            _defaultCooldownTicks = sender.RecoveryProgramCooldown;
        }


        //------------------------------------------------------
        //
        //  IClock
        //
        //------------------------------------------------------

        #region IClock

        bool IClock.Tick(int interval)
        {
            if (TickUtility.Time(ref _cooldownTicks))
            {
                return false;
            }
            else
            {
                _gizmo.disabledReason = "AbilityOnCooldown".Translate(_cooldownTicks.ToStringTicksToPeriod()).Resolve();
                return true;
            }
        }

        void IClock.OnStarted()
        {
            if (_gizmo is null)
            {
                _gizmo = new RecoveryProgramAbility(this);
            }

            if (_cooldownTicks < 1)
            {
                _cooldownTicks = FOU.Settings.RecoveryProgramCooldown;
            }

            _gizmo.Disable("AbilityOnCooldown".Translate(_cooldownTicks.ToStringTicksToPeriod()).Resolve());
        }

        void IClock.OnStopped()
        {
            _cooldownTicks = 0;

            _gizmo.Disabled = false;
            _gizmo.disabledReason = string.Empty;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private int _cooldownTicks;
        private int _defaultCooldownTicks = FOU.Settings.RecoveryProgramCooldown;

        [Unsaved]
        private Command _gizmo;

        #endregion
    }
}
