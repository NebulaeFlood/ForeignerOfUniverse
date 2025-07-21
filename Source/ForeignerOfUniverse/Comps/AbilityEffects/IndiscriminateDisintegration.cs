using ForeignerOfUniverse.Utilities;
using Nebulae.RimWorld.UI;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;
using Verse;
using static Verse.DamageWorker;

namespace ForeignerOfUniverse.Comps.AbilityEffects
{
    public class IndiscriminateDisintegration : NaniteCost
    {
        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override sealed void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            float totalDamageAmount = FOU.Settings.IndiscriminateDisintegrationDamage;

            var damageInfo = new DamageInfo(FOUDefOf.FOU_Disintegration, totalDamageAmount, instigator: parent.pawn);
            var targets = GenRadial.RadialDistinctThingsAround(target.Cell, parent.pawn.Map, FOU.Settings.IndiscriminateDisintegrationRadius, true).ToArray();

            damageInfo.SetIgnoreArmor(true);

            for (int i = targets.Length - 1; i >= 0; i--)
            {
                var item = targets[i];

                if (item is Pawn pawn)
                {
                    if (pawn.OwnNanites())
                    {
                        continue;
                    }

                    MoteMaker.MakeAttachedOverlay(pawn, FOUDefOf.Mote_FOU_MatterFloatAway, Vector3.zero);

                    float totalDamageDealt = 0f;

                    while (!pawn.Dead && totalDamageDealt < totalDamageAmount)
                    {
                        damageInfo.SetAmount(GetRandowDamage());
                        totalDamageDealt += ForceTakeDamage(pawn, damageInfo);
                    }

                    GenLeaving.DropFilthDueToDamage(pawn, totalDamageAmount);
                    base.Apply(pawn, dest);
                }
                else
                {
                    damageInfo.SetAmount(totalDamageAmount);
                    MoteMaker.MakeAttachedOverlay(item, FOUDefOf.Mote_FOU_MatterFloatAway, Vector3.zero);
                    GenLeaving.DropFilthDueToDamage(item, ForceTakeDamage(item, damageInfo));
                    base.Apply(item, dest);
                }
            }
        }

        public override sealed bool CanApplyOn(GlobalTargetInfo target) => false;

        public override sealed bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest) => true;

        public override sealed void DrawEffectPreview(LocalTargetInfo target) => GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(target.Cell, FOU.Settings.IndiscriminateDisintegrationRadius, true).ToList());

        public override sealed void Initialize(AbilityCompProperties props)
        {
            this.props = props;
            var settings = FOU.Settings;
            settings.Saved += OnSettingsSaved;
            Cost = settings.IndiscriminateDisintegrationCost;
        }

        public override sealed bool Valid(LocalTargetInfo target, bool throwMessages = false) => true;

        public override sealed bool Valid(GlobalTargetInfo target, bool throwMessages = false) => false;

        #endregion


        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        protected virtual void DisintegratePawn(Pawn pawn, DamageInfo damageInfo)
        {
            float totalDamageAmount = damageInfo.Amount;
            float totalDamageDealt = 0f;

            while (totalDamageDealt < totalDamageAmount)
            {
                damageInfo.SetAmount(GetRandowDamage());
                totalDamageDealt += Mathf.Max(1f, pawn.TakeDamage(damageInfo).totalDamageDealt);
            }
        }

        protected float GetRandowDamage()
        {
            _damageRange.max = MaxDamageRange.RandomInRange;
            return _damageRange.RandomInRange;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void OnSettingsSaved(FOUSettings settings, EventArgs args)
        {
            Cost = settings.IndiscriminateDisintegrationCost;
        }

        private float ForceTakeDamage(Thing target, DamageInfo info)
        {
            var result = FOUDefOf.FOU_Disintegration.Worker.Apply(info, target);

            if (target.SpawnedOrAnyParentSpawned)
            {
                target.MapHeld.damageWatcher.Notify_DamageTaken(target, result.totalDamageDealt);
            }

            if (FOUDefOf.FOU_Disintegration.ExternalViolenceFor(target))
            {
                GenLeaving.DropFilthDueToDamage(target, result.totalDamageDealt);

                parent.pawn.records.AddTo(RecordDefOf.DamageDealt, result.totalDamageDealt);
                
                if (parent.pawn.Faction == Faction.OfPlayer)
                {
                    QuestUtility.SendQuestTargetSignals(target.questTags, "TookDamageFromPlayer", this.Named("SUBJECT"), parent.pawn.Named("INSTIGATOR"));
                }

                QuestUtility.SendQuestTargetSignals(target.questTags, "TookDamage", target.Named("SUBJECT"), parent.pawn.Named("INSTIGATOR"), target.MapHeld.Named("MAP"));
            }

            target.PostApplyDamage(info, result.totalDamageDealt);
            return Mathf.Max(1f, result.totalDamageDealt);
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Static Fields
        //
        //------------------------------------------------------

        #region Private Static Fields

        private static readonly FloatRange MaxDamageRange = new FloatRange(10f, 15f);

        private static FloatRange _damageRange = new FloatRange(2f, 15f);

        #endregion
    }
}
