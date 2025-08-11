using ForeignerOfUniverse.Comps.Things;
using ForeignerOfUniverse.Gizmos;
using ForeignerOfUniverse.Models;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ForeignerOfUniverse.Genes
{
    public sealed class HigherDimensionalNanites : Gene_Resource, IGeneResourceDrain
    {
        //------------------------------------------------------
        //
        //  Public Static Fields
        //
        //------------------------------------------------------

        #region Public Static Fields

        public static readonly Color ResourceValueBarColor = new ColorInt(20, 28, 56).ToColor;
        public static readonly Color ResourceValueBarHighlightColor = new ColorInt(40, 48, 76).ToColor;

        #endregion


        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties

        public override bool Active => overriddenByGene is null;

        public bool CanOffset => overriddenByGene is null;

        public string DisplayLabel => def.label + " (" + "Gene".Translate() + ")";

        public override float InitialResourceMax => FOU.Settings.NanitesMaxStorage * 0.01f;

        public override float MinLevelForAlert => 0f;

        public Pawn Pawn => pawn;

        public Gene_Resource Resource => this;

        public float ResourceLossPerDay
        {
            get
            {
                switch (_checkUp.NaniteState)
                {
                    case NaniteState.Subside:
                        return -(FOU.Settings.NanitesDailyReplication * 0.02f);
                    case NaniteState.Activate:
                        return -(FOU.Settings.NanitesDailyReplication * 0.04f);
                    default:
                        return -(FOU.Settings.NanitesDailyReplication * 0.01f);
                }
            }
        }

        #endregion


        //------------------------------------------------------
        //
        //  Protected Properties
        //
        //------------------------------------------------------

        #region Protected Properties

        protected override Color BarColor => ResourceValueBarColor;

        protected override Color BarHighlightColor => ResourceValueBarHighlightColor;

        #endregion


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public int CalculateDismantleForMaxGrowth(Thing material, out float unitsPerMaterial)
        {
            if (cur >= max)
            {
                unitsPerMaterial = 0f;
                return 0;
            }

            unitsPerMaterial = material.GetStatValue(StatDefOf.Mass) * FOU.Settings.NanitesPerKilogram;

            if (material.stackCount == 1)
            {
                return 1;
            }

            return Mathf.CeilToInt((max - cur) * 100f / unitsPerMaterial);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref protocol, "ProtocolType");

            if (Scribe.mode is LoadSaveMode.PostLoadInit)
            {
                var settings = FOU.Settings;
                settings.Saved += OnSettingsSaved;

                float maxValue = settings.NanitesMaxStorage * 0.01f;

                cur = (cur / max) * maxValue;
                max = maxValue;

                _healAmount = settings.PhoenixRegenerationPerDay * 0.0005f;
                _replicationAmount = settings.NanitesDailyReplication * 0.000005f;

                if (pawn.GetComp<WhileApplyDamage>() is null)
                {
                    pawn.AllComps.Add(new WhileApplyDamage
                    {
                        nanites = this,
                        parent = pawn
                    });
                }

                protocol = new NaniteProtocol(protocol, pawn);

                if (protocol.CheckUp)
                {
                    _checkUp = new NaniteCheckUp(protocol, pawn.health.hediffSet.hediffs);
                    _checkUp.Purge();
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (!Active)
            {
                yield break;
            }

            if (Find.Selector.SelectedPawns.Count == 1 || pawn.MapHeld is null)
            {
                if (gizmo is null)
                {
                    bool DrainNanites(Gene x)
                    {
                        return x.Active && x is IGeneResourceDrain gene && ReferenceEquals(gene.Resource, this);
                    }

                    gizmo = new ResourceNanites(
                        this,
                        pawn.genes.GenesListForReading.Where(DrainNanites).Cast<IGeneResourceDrain>().ToList(),
                        ResourceValueBarColor,
                        ResourceValueBarHighlightColor);
                }

                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: " + ResourceLabel + " -100",
                    action = delegate
                    {
                        OffsetStore(-1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: " + ResourceLabel + " +100",
                    action = delegate
                    {
                        OffsetStore(1f);
                    }
                };
            }
        }

        public void OffsetStore(float value)
        {
            cur = Mathf.Clamp(cur + value, 0f, max);
        }

        public void OnAuthorityChanged()
        {
            protocol = new NaniteProtocol(protocol, pawn);
        }

        public override void PostAdd()
        {
            base.PostAdd();
            pawn.AllComps.Add(new WhileApplyDamage
            {
                nanites = this,
                parent = pawn
            });
            protocol = new NaniteProtocol(pawn);
        }

        public override void PostMake()
        {
            var settings = FOU.Settings;
            settings.Saved += OnSettingsSaved;

            _healAmount = settings.PhoenixRegenerationPerDay * 0.0005f;
            _replicationAmount = settings.NanitesDailyReplication * 0.000005f;
        }

        public override int PostProcessValue(float value)
        {
            return Mathf.FloorToInt(value * 100f);
        }

        public override void PostRemove()
        {
            base.PostRemove();

            if (pawn.TryGetComp<WhileApplyDamage>(out var comp))
            {
                pawn.AllComps.Remove(comp);
            }
        }

        public void PsychicShildClose()
        {
            if (!protocol.PsychicShildOpen)
            {
                return;
            }

            pawn.health.RemoveHediff(protocol.PsychicShild);

            protocol.PsychicShildOpen = false;
            protocol.PsychicShild = null;
        }

        public void PsychicShildOpen()
        {
            if (protocol.PsychicShildOpen)
            {
                return;
            }

            protocol.PsychicShildOpen = true;
            protocol.PsychicShild = pawn.health.AddHediff(FOUDefOf.FOU_PsychicShild);
        }

        public override void Tick()
        {
            if (!pawn.IsHashIntervalTick(30) || !Active)
            {
                return;
            }

            if (!protocol.CheckUp)
            {
                OffsetStore(_replicationAmount);
                return;
            }

            bool shouldNotify = pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer;
            _checkUp = new NaniteCheckUp(protocol, pawn.health.hediffSet.hediffs);

            if (_checkUp.AnyDisease)
            {
                RemoveDiseases(_checkUp, shouldNotify && FOU.Settings.SendLetterWhenImmunized);
            }

            if (protocol.Phoenix)
            {
                float healAmount = _healAmount;
                float replicationAmount = _replicationAmount;

                switch (_checkUp.NaniteState)
                {
                    case NaniteState.Subside:
                        healAmount *= 2f;
                        replicationAmount *= 2f;
                        break;
                    case NaniteState.Activate:
                        healAmount *= 4f;
                        replicationAmount *= 4f;
                        break;
                    default:
                        break;
                }

                Regenerate(healAmount, shouldNotify);
                OffsetStore(replicationAmount);
            }
            else
            {
                OffsetStore(_replicationAmount);
            }

            if (_checkUp.ShouldEnforceAnchor)
            {
                protocol.Anchor = pawn.health.AddHediff(FOUDefOf.FOU_ExistenceAnchor);
            }

            if (_checkUp.ShouldEnforcePhoenix)
            {
                protocol.RecoveryProgram = pawn.health.AddHediff(FOUDefOf.FOU_RecoveryProgram);
            }

            if (_checkUp.ShouldEnforcePsychicShild)
            {
                protocol.PsychicShild = pawn.health.AddHediff(FOUDefOf.FOU_PsychicShild);
            }

            _checkUp.Purge();
        }

        #endregion


        //------------------------------------------------------
        //
        //  Internal Methods
        //
        //------------------------------------------------------

        #region Internal Methods

        internal void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            if (protocol.Contravention && protocol.ContraventionBarrierOpen)
            {
                if (cur >= 0.01f)
                {
                    var amount = dinfo.Amount;
                    var factor = FOU.Settings.NanitesPerDamage;

                    cur = Mathf.Clamp(cur - amount * factor * 0.01f, 0f, max);
                    absorbed = true;

                    var impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                    var pos = pawn.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f;

                    SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(pawn);
                    FleckMaker.Static(pos, pawn.Map, FleckDefOf.ExplosionFlash, Mathf.Min(10f, 2f + amount * 0.1f));
                }
                else
                {
                    absorbed = false;
                }
            }
            else
            {
                absorbed = false;
            }
        }

        internal void PostApplyDamage(DamageInfo dinfo)
        {
            if (protocol.Ascension && dinfo.Def.causeStun)
            {
                pawn.stances.stunner.StopStun();
            }
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void OnSettingsSaved(FOUSettings sender, EventArgs args)
        {
            float maxValue = sender.NanitesMaxStorage * 0.01f;

            cur = (cur / max) * maxValue;
            max = maxValue;

            _healAmount = sender.PhoenixRegenerationPerDay * 0.0005f;
            _replicationAmount = sender.NanitesDailyReplication * 0.000005f;
        }

        private void Regenerate(float amount, bool shouldNotify)
        {
            if (pawn.mutant != null && pawn.mutant.HasTurned)
            {
                pawn.mutant.Revert();

                if (shouldNotify)
                {
                    TaggedString letterLabel = "FOU.Letters.NaniteRevertMutant.Label".Translate();
                    TaggedString letterContent = "FOU.Letters.NaniteRevertMutant.Content".Translate(pawn.Named("TARGET"), pawn.Named("CASTER"), $"\n\n - {"FOU.Protocols.Phoenix.Label".Translate().Colorize(ColoredText.NameColor)}");

                    Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.PositiveEvent, pawn);
                }
            }

            var health = pawn.health;
            var hediffSet = health.hediffSet;

            float cost;

            foreach (var injury in _checkUp.Injuries)
            {
                cost = Mathf.Min(amount, injury.Severity);
                amount -= cost;
                injury.Heal(cost);
                hediffSet.Notify_Regenerated(cost);

                if (amount <= 0f)
                {
                    return;
                }
            }

            foreach (var missingPart in _checkUp.MissingParts)
            {
                var part = missingPart.Part;
                float partMaxHealth = part.def.GetMaxHealth(hediffSet.pawn);

                health.RemoveHediff(missingPart);

                cost = Mathf.Max(1f, Mathf.Min(amount, partMaxHealth));
                amount -= cost;

                float damageAmount = partMaxHealth - cost;
                health.AddHediff(
                    FOUDefOf.FOU_Regenerating,
                    part,
                    new DamageInfo(DamageDefOf.SurgicalCut, damageAmount, hitPart: part))
                        .Severity = damageAmount;

                hediffSet.Notify_Regenerated(cost);

                if (amount <= 0f)
                {
                    return;
                }
            }

            if (_checkUp.AnyBloodLoss)
            {
                _checkUp.BloodLoss.Heal(amount * 0.01f);
            }

            if (_checkUp.AnyToxic)
            {
                _checkUp.ToxicBuildup.Heal(amount * 0.1f);
            }
        }

        private void RemoveDiseases(NaniteCheckUp checkUp, bool shouldNotify)
        {
            if (checkUp.ImmunizedDiseases.Count > 0)
            {
                var node = checkUp.ImmunizedDiseases.First;

                if (shouldNotify)
                {
                    string hediffs = string.Empty;

                    while (node != null)
                    {
                        pawn.health.RemoveHediff(node.Value);
                        hediffs += $"\n - {node.Value.LabelCap}";
                        node = node.Next;
                    }

                    TaggedString letterLabel = "FOU.Letters.DiseaseCured.Label".Translate();
                    TaggedString letterContent = "FOU.Letters.DiseaseCured.Content".Translate(pawn.Named("PAWN"), hediffs);

                    Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.PositiveEvent, pawn);
                }
                else
                {
                    while (node != null)
                    {
                        pawn.health.RemoveHediff(node.Value);
                        node = node.Next;
                    }
                }
            }
        }

        #endregion


        internal NaniteProtocol protocol;


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private NaniteCheckUp _checkUp;
        private float _healAmount;
        private float _replicationAmount;

        #endregion
    }
}
