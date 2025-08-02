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
    public sealed class IndiscriminateDisintegration : NaniteCost
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

            _maxDamageRange.max = totalDamageAmount * 0.5f;

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
                        _damageRange.max = _maxDamageRange.RandomInRange;

                        damageInfo.SetAmount(_damageRange.RandomInRange);
                        totalDamageDealt += pawn.ForceTakeDamage(damageInfo, parent.pawn);
                    }

                    GenLeaving.DropFilthDueToDamage(pawn, totalDamageAmount);
                    base.Apply(pawn, dest);
                }
                else
                {
                    damageInfo.SetAmount(totalDamageAmount);
                    MoteMaker.MakeAttachedOverlay(item, FOUDefOf.Mote_FOU_MatterFloatAway, Vector3.zero);
                    GenLeaving.DropFilthDueToDamage(item, item.ForceTakeDamage(damageInfo, parent.pawn));
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


        private void OnSettingsSaved(FOUSettings settings, EventArgs args)
        {
            Cost = settings.IndiscriminateDisintegrationCost;
        }


        //------------------------------------------------------
        //
        //  Private Static Fields
        //
        //------------------------------------------------------

        #region Private Static Fields

        private static FloatRange _maxDamageRange = new FloatRange(10f, 15f);
        private static FloatRange _damageRange = new FloatRange(2f, 15f);

        #endregion
    }
}
