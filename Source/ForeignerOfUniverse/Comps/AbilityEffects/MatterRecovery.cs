using ForeignerOfUniverse.Utilities;
using Nebulae.RimWorld.UI;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace ForeignerOfUniverse.Comps.AbilityEffects
{
    public class MatterRecovery : NaniteCost
    {
        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override sealed bool AICanTargetNow(LocalTargetInfo target)
        {
            if (!base.AICanTargetNow(target))
            {
                return false;
            }

            if (target.Pawn is null)
            {
                return false;
            }

            return !target.Pawn.HostileTo(parent.pawn);
        }

        public override sealed void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Recover(target);
            base.Apply(target, dest);
        }

        public override sealed bool CanApplyOn(GlobalTargetInfo target) => false;

        public override sealed bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => Valid(target);

        public override sealed string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (target.Pawn is Pawn pawn)
            {
                if (IsPawnNeedRecover(pawn))
                {
                    return string.Empty;
                }
                else
                {
                    return "FOU.NaniteAbility.TargetPawnDoNotNeedRecover".Translate(pawn.Named("PAWN")).Resolve();
                }
            }
            else if (target.Thing is Thing thing)
            {
                if (thing is Corpse)
                {
                    return string.Empty;
                }

                if (thing.TryGetComp<CompRottable>() is CompRottable rottable && rottable.RotProgress > 0f)
                {
                    return string.Empty;
                }

                if (thing.TryGetComp<CompRefuelable>() is CompRefuelable refuelable && !refuelable.IsFull)
                {
                    return string.Empty;
                }

                if (!thing.def.useHitPoints)
                {
                    return "FOU.NaniteAbility.TargetThingIsUnrepairable".Translate(parent.def.LabelCap.Colorize(ColoredText.NameColor)).Resolve();
                }

                if (thing.HitPoints < thing.MaxHitPoints)
                {
                    return string.Empty;
                }

                return "FOU.NaniteAbility.TargetThingDoNotNeedRecover".Translate(parent.def.LabelCap.Colorize(ColoredText.NameColor)).Resolve();
            }

            return string.Empty;
        }

        public override sealed void Initialize(AbilityCompProperties props)
        {
            this.props = props;
            var settings = FOU.Settings;
            settings.Saved += OnSettingsSaved;
            Cost = settings.MatterRecoveryCost;
        }

        public override sealed bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Pawn is Pawn pawn)
            {
                if (IsPawnNeedRecover(pawn))
                {
                    return true;
                }
                else
                {
                    if (throwMessages)
                    {
                        Messages.Message("FOU.NaniteAbility.TargetPawnDoNotNeedRecover".Translate(pawn.Named("PAWN")).Resolve(),
                            pawn, MessageTypeDefOf.RejectInput, historical: false);
                    }

                    return false;
                }
            }
            else if (target.Thing is Thing thing)
            {
                if (thing is Corpse)
                {
                    return true;
                }

                if (thing.TryGetComp<CompRottable>() is CompRottable rottable && rottable.RotProgress > 0f)
                {
                    return true;
                }

                if (thing.TryGetComp<CompRefuelable>() is CompRefuelable refuelable && !refuelable.IsFull)
                {
                    return true;
                }

                if (!thing.def.useHitPoints)
                {
                    if (throwMessages)
                    {
                        Messages.Message("FOU.NaniteAbility.TargetThingIsUnrepairable".Translate(parent.def.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                            thing, MessageTypeDefOf.RejectInput, historical: false);
                    }

                    return false;
                }

                if (thing.HitPoints < thing.MaxHitPoints)
                {
                    return true;
                }

                if (throwMessages)
                {
                    Messages.Message("FOU.NaniteAbility.TargetThingDoNotNeedRecover".Translate(parent.def.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                        thing, MessageTypeDefOf.RejectInput, historical: false);
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        public override sealed bool Valid(GlobalTargetInfo target, bool throwMessages = false) => false;

        #endregion


        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        protected virtual bool IsPawnNeedRecover(Pawn pawn)
        {
            return NaniteRecoverUtility.CanRecover(pawn);
        }

        protected virtual void Recover(LocalTargetInfo target)
        {
            if (target.Pawn is Pawn pawn)
            {
                NaniteRecoverUtility.Recover(pawn, parent.pawn);
            }
            else if (target.Thing is Thing thing)
            {
                NaniteRecoverUtility.Recover(thing, parent.pawn);
            }
        }

        #endregion


        private void OnSettingsSaved(FOUSettings sender, EventArgs args)
        {
            Cost = sender.MatterRecoveryCost;
        }
    }
}
