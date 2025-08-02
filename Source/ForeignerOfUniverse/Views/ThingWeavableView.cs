using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Windows;
using Mono.Security.Cryptography;
using Nebulae.RimWorld.UI.Automation;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Controls.Panels;
using Nebulae.RimWorld.UI.Core.Data;
using Nebulae.RimWorld.UI.Core.Data.Bindings;
using Nebulae.RimWorld.UI.Core.Events;
using Nebulae.RimWorld.UI.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using Grid = Nebulae.RimWorld.UI.Controls.Panels.Grid;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingWeavableView : CompositeControl
    {
        public readonly ThingInfo Model;


        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties

        public int Count
        {
            get => (int)(float)_numberBox.GetValue(NumberBox.ValueProperty);
            set
            {
                _numberBox.SetValue(NumberBox.ValueProperty, (float)value);
            }
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    if (_selected)
                    {
                        _background.Background = CursorDirectlyOver ? BrushUtility.DarkGrey : BrushUtility.DarkerGrey;
                        _selected = false;
                    }
                    else
                    {
                        _background.Background = TexUI.HighlightSelectedTex;
                        _selected = true;
                    }
                }
            }
        }

        #endregion


        internal ThingWeavableView(ThingInfo model, float naniteStore, float unit)
        {
            _maxCount = Mathf.FloorToInt(naniteStore * unit / ThingInfo.GetMass(model));

            IsHitTestVisible = _maxCount > 0;
            Model = model;
            Name = model.LabelCap;
            Tooltip = model.Description;

            _background = new Border
            {
                Background = BrushUtility.DarkerGrey,
                BorderBrush = BrushUtility.LightGrey,
                BorderThickness = 1f
            };

            _numberBox = new NumberBox
            {
                MaxValue = _maxCount,
                Value = 1f,
                Tooltip = "FOU.ThingInfo.Weave.Number.Tooltip".Translate()
            };
            _numberBox.DependencyPropertyChanged += OnNumberBoxDependencyPropertyChanged;

            Initialize();
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

            var deleteButton = new IconButton(TexButton.Delete)
            {
                Tooltip = "FOU.ThingInfo.Weave.Delete.Tooltip".Translate(Name.Colorize(ColoredText.NameColor))
            };
            deleteButton.Click += OnDelete;

            Grid panel;

            if (_maxCount > 0)
            {
                _numberBox.MinValue = 1f;

                var slider = new Slider
                {
                    MinValue = 1f,
                    MaxValue = _maxCount,
                    Value = 1f,
                    Tooltip = "FOU.ThingInfo.Weave.Number.Tooltip".Translate()
                };

                Binding.Create(slider, Slider.ValueProperty, _numberBox, NumberBox.ValueProperty, BindingMode.TwoWay);

                panel = new Grid { Margin = 4f }
                    .DefineColumns(34f, Grid.Remain, 34f, 120f)
                    .DefineRows(34f, 28f)
                    .Set(
                        icon, nameLabel, deleteButton, _numberBox,
                        slider, slider, slider, slider);
            }
            else
            {
                nameLabel.IsEnabled = false;
                _numberBox.IsEnabled = false;
                _numberBox.MinValue = 0f;

                Tooltip = $"{Tooltip.text}\n\n{"FOU.ThingInfo.Weave.Disabled.Tooltip".Translate().Colorize(ColoredText.WarningColor)}";

                panel = new Grid { Margin = 4f }
                    .DefineColumns(34f, Grid.Remain, 34f, 120f)
                    .DefineRows(34f)
                    .Set(icon, nameLabel, deleteButton, _numberBox);
            }

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

                if (this.TryFindPartent<ThingWeaveView>(out var view))
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                    view.Deselect(this);
                }
            }
            else
            {
                _background.Background = TexUI.HighlightSelectedTex;
                _selected = true;

                if (this.TryFindPartent<ThingWeaveView>(out var view))
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
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (LayoutManager.Owner is ThingWeaveWindow window)
            {
                window.Comp.weavableThings.Remove(Model);
            }

            if (this.TryFindPartent<ThingWeaveView>(out var view))
            {
                view.Remove(this);
            }
        }

        private void OnNumberBoxDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (this.TryFindPartent<ThingWeaveView>(out var view))
            {
                view.DeselectPolicy();
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

        private readonly int _maxCount;

        #endregion
    }
}
