using ForeignerOfUniverse.Comps.AbilityEffects;
using ForeignerOfUniverse.Genes;
using ForeignerOfUniverse.Models;
using Nebulae.RimWorld.UI.Utilities;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ForeignerOfUniverse.Gizmos
{
    [StaticConstructorOnStartup]
    internal sealed class ResourceNanites : GeneGizmo_Resource
    {
        //------------------------------------------------------
        //
        //  Protected Properties
        //
        //------------------------------------------------------

        #region Protected Properties

        protected override bool IsDraggable => false;

        protected override bool DraggingBar { get; set; }

        #endregion


        internal ResourceNanites(HigherDimensionalNanites gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barHighlightColor)
            : base(gene, drainGenes, barColor, barHighlightColor)
        {
            _nanites = gene;
        }


        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            if (Event.current.type is EventType.KeyUp)
            {
                _showDescription = !_showDescription && Event.current.keyCode is KeyCode.LeftControl;
            }

            GizmoResult gizmoResult = base.GizmoOnGUI(topLeft, maxWidth, parms);

            float time = Mathf.Repeat(Time.time, 0.85f);
            float opacity = 1f;

            if (time < 0.1f)
            {
                opacity = time * 10f;
            }
            else if (time >= 0.25f)
            {
                opacity = 1f - (time - 0.25f) / 0.6f;
            }

            if (MapGizmoUtility.LastMouseOverGizmo is Command_Ability ability && gene.Max > 0f)
            {
                if (ability.Ability.EffectComps.Find(x => x is NaniteCost) is NaniteCost comp)
                {
                    Rect highlightRect = barRect.ContractedBy(3f);

                    float width = highlightRect.width;

                    highlightRect.xMax = highlightRect.xMin + width * gene.ValuePercent;
                    highlightRect.xMin = Mathf.Max(
                        highlightRect.xMin,
                        highlightRect.xMax - width * Mathf.Min(comp.Cost / gene.Max, 1f));

                    GUI.color = new Color(1f, 1f, 1f, opacity * 0.7f);
                    GenUI.DrawTextureWithMaterial(highlightRect, ValueBarHighlight, null);
                    GUI.color = Color.white;
                }
            }

            return gizmoResult;
        }


        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        protected override void DrawHeader(Rect headerRect, ref bool mouseOverElement)
        {
            if (_nanites.protocol.NooNet)
            {
                headerRect.xMax -= 24f;
                var iconRect = new Rect(headerRect.xMax + 2f, headerRect.y + 2f, 20f, 20f);

                GUI.DrawTexture(iconRect, ShildIcon, ScaleMode.ScaleToFit);
                GUI.DrawTexture(
                    new Rect(iconRect.center.x, iconRect.y, iconRect.width / 2f, iconRect.height / 2f),
                    _nanites.protocol.PsychicShildOpen ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);

                if (Widgets.ButtonInvisible(iconRect))
                {
                    if (_nanites.protocol.PsychicShildOpen)
                    {
                        _nanites.ShildClose();
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    else
                    {
                        _nanites.ShildOpen();
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                }

                if (iconRect.Contains(Event.current.mousePosition))
                {
                    Widgets.DrawHighlight(new Rect(headerRect.xMax, headerRect.y, 24f, 24f));
                    mouseOverElement = true;
                }

                TooltipHandler.TipRegion(iconRect, "FOU.Protocols.NooNet.Tip".Translate());
            }

            base.DrawHeader(headerRect, ref mouseOverElement);
        }

        protected override string GetTooltip()
        {
            string text = string.Format("{0}: {1} / {2}\n",
                gene.ResourceLabel.CapitalizeFirst().Colorize(ColoredText.TipSectionTitleColor),
                gene.ValueForDisplay,
                gene.MaxForDisplay);

            if (!drainGenes.NullOrEmpty())
            {
                float totalDrainPerDay = 0f;

                var cachedDrainGenes = new List<(IGeneResourceDrain Gene, float Value)>(drainGenes.Count);

                drainGenes.ForEach(x =>
                {
                    if (x.CanOffset)
                    {
                        totalDrainPerDay += x.ResourceLossPerDay;
                        cachedDrainGenes.Add((x, x.ResourceLossPerDay));
                    }
                });

                if (totalDrainPerDay != 0f)
                {
                    text = string.Concat(new string[]
                    {
                        text,
                        "\n",
                        (totalDrainPerDay < 0f) ? "RegenerationRate".Translate().Resolve() : "DrainRate".Translate().Resolve(),
                        ": ",
                        "PerDay".Translate(Mathf.Abs(totalDrainPerDay * 100f)).Resolve()
                    });

                    foreach (var pair in cachedDrainGenes)
                    {
                        text = string.Concat(new string[]
                        {
                            text,
                            "\n  - ",
                            pair.Gene.DisplayLabel.CapitalizeFirst(),
                            ": ",
                            "PerDay".Translate(gene.PostProcessValue(-pair.Value).ToStringWithSign()).Resolve()
                        });
                    }
                }
            }

            if (!gene.def.resourceDescription.NullOrEmpty())
            {
                text += "\n\n" + gene.def.resourceDescription;
            }

            text += "\n\n" + "FOU.Protocols.Title".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor) + '\n';
            text += "\n - " + "FOU.Protocols.Foundation.Label".Translate().Resolve().Colorize(ColoredText.NameColor);

            if (_showDescription)
            {
                text += "\n" + "FOU.Protocols.Foundation.Description".Translate().Resolve() + "\n";
            }

            if (_nanites.protocol.Ascension)
            {
                text += "\n - " + "FOU.Protocols.Ascension.Label".Translate().Resolve().Colorize(ColoredText.NameColor);

                if (_showDescription)
                {
                    text += "\n" + "FOU.Protocols.Ascension.Description".Translate().Resolve() + "\n";
                }
            }

            if (_nanites.protocol.NooNet)
            {
                text += "\n - " + "FOU.Protocols.NooNet.Label".Translate().Resolve().Colorize(ColoredText.NameColor);

                if (_showDescription)
                {
                    text += "\n" + "FOU.Protocols.NooNet.Description".Translate().Resolve() + "\n";
                }
            }

            if (_nanites.protocol.Weave)
            {
                text += "\n - " + "FOU.Protocols.Weave.Label".Translate().Resolve().Colorize(ColoredText.NameColor);

                if (_showDescription)
                {
                    text += "\n" + "FOU.Protocols.Weave.Description".Translate().Resolve() + "\n";
                }
            }

            if (_nanites.protocol.Phoenix)
            {
                text += "\n - " + "FOU.Protocols.Phoenix.Label".Translate().Resolve().Colorize(ColoredText.NameColor);

                if (_showDescription)
                {
                    text += "\n" + "FOU.Protocols.Phoenix.Description".Translate().Resolve() + "\n";
                }
            }

            text += "\n - " + "FOU.Protocols.Anchor.Label".Translate().Resolve().Colorize(ColoredText.NameColor);

            if (_showDescription)
            {
                text += "\n" + "FOU.Protocols.Anchor.Description".Translate().Resolve();
                text += "\n\n<color=grey>" + "FOU.Protocols.Hide.Tip".Translate().Resolve() + "</color>";
            }
            else
            {
                text += "\n\n<color=grey>"+ "FOU.Protocols.Tip".Translate().Resolve() + "</color>";
            }

            return text;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly HigherDimensionalNanites _nanites;
        private bool _showDescription;

        #endregion


        //------------------------------------------------------
        //
        //  Private Static Fields
        //
        //------------------------------------------------------

        #region Private Static Fields

        private static readonly Texture2D ShildIcon = ContentFinder<Texture2D>.Get("UI/Icons/Abilities/FOU_Shild");
        private static readonly Texture2D ValueBarHighlight = SolidColorMaterials.NewSolidColorTexture(new Color(0.78f, 0.72f, 0.66f));

        #endregion
    }
}
