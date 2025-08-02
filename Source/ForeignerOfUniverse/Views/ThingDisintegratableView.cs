using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Core.Data.Bindings;
using Nebulae.RimWorld.UI.Core.Events;
using Nebulae.RimWorld.UI.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Grid = Nebulae.RimWorld.UI.Controls.Panels.Grid;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingDisintegratableView : CompositeControl
    {
        public readonly Thing Model;


        public int Count
        {
            get => (int)(float)_numberBox.GetValue(NumberBox.ValueProperty);
            set => _numberBox.SetValue(NumberBox.ValueProperty, (float)value);
        }


        internal ThingDisintegratableView(Thing thing)
        {
            IsHitTestVisible = true;

            Model = thing;
            Name = thing.LabelCap;
            Tooltip = thing.DescriptionDetailed;

            _background = new Border
            {
                Background = BrushUtility.DarkerGrey,
                BorderBrush = BrushUtility.LightGrey,
                BorderThickness = 1f
            };

            _numberBox = new NumberBox
            {
                MinValue = 1f,
                MaxValue = Model.stackCount,
                Value = 1f,
                Tooltip = "FOU.ThingInfo.Disintegrate.Number.Tooltip".Translate()
            };

            Initialize();
        }


        internal static ThingDisintegratableView Convert(Thing thing)
        {
            return new ThingDisintegratableView(thing);
        }


        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        protected override Control CreateContent()
        {
            var icon = new ThingIconView(Model);

            var nameLabel = new Label
            {
                Anchor = TextAnchor.MiddleLeft,
                Text = Model.LabelCap,
                Margin = new Thickness(4f, 0f, 0f, 0f)
            };

            Grid panel;

            var slider = new Slider
            {
                MinValue = 1f,
                MaxValue = Model.stackCount,
                Value = 1f,
                Tooltip = "FOU.ThingInfo.Disintegrate.Number.Tooltip".Translate()
            };

            Binding.Create(slider, Slider.ValueProperty, _numberBox, NumberBox.ValueProperty, BindingMode.TwoWay);

            panel = new Grid { Margin = 4f }
                .DefineColumns(34f, Grid.Remain, 220f)
                .DefineRows(34f, 28f)
                .Set(
                    icon, nameLabel, _numberBox,
                    slider, slider, slider);
            _background.Content = panel;

            return _background;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!CursorDirectlyOver)
            {
                return;
            }

            if (_selected)
            {
                _background.Background = BrushUtility.DarkerGrey;
                _selected = false;

                if (this.TryFindPartent<ThingDisintegrateView>(out var view))
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                    view.Deselect(this);
                }
            }
            else
            {
                _background.Background = TexUI.HighlightSelectedTex;
                _selected = true;

                if (this.TryFindPartent<ThingDisintegrateView>(out var view))
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                    view.Select(this);
                }
            }
        }

        protected override void OnMouseEnter(RoutedEventArgs e)
        {
            if (!_selected)
            {
                SoundDefOf.Mouseover_Standard.PlayOneShotOnCamera();

                _background.Background = BrushUtility.DarkGrey;
            }
        }

        protected override void OnMouseLeave(RoutedEventArgs e)
        {
            if (!_selected)
            {
                _background.Background = BrushUtility.DarkerGrey;
            }
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly NumberBox _numberBox;
        private readonly Border _background;

        private bool _selected;

        #endregion
    }
}
