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
using Grid = Nebulae.RimWorld.UI.Controls.Panels.Grid;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingView : CompositeControl
    {
        public readonly ThingInfo Model;


        public int Count => (int)(float)_numberBox.GetValue(NumberBox.ValueProperty);


        internal ThingView(ThingInfo model, float naniteStore, float unit)
        {
            IsHitTestVisible = true;

            _maxCount = Mathf.FloorToInt(naniteStore / (ThingInfo.GetMass(model) * unit));

            Model = model;
            Name = model.Def.LabelCap;

            _background = new Border
            {
                Background = BrushUtility.DarkGrey,
                BorderBrush = BrushUtility.Grey,
                BorderThickness = 1f
            };

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
                Tooltip = "FOU.ThingInfo.Delete.Tooltip".Translate(Name.Colorize(ColoredText.NameColor))
            };
            deleteButton.Click += OnDelete;

            Grid panel;

            if (_maxCount > 0)
            {
                var confirmButton = new Button
                {
                    Text = "FOU.ThingInfo.Confirm.Label".Translate(),
                    Tooltip = "FOU.ThingInfo.Confirm.Tooltip".Translate()
                };
                confirmButton.Click += OnConfirm;

                _numberBox = new NumberBox
                {
                    MinValue = 1f,
                    MaxValue = _maxCount,
                    Value = 1f,
                    Tooltip = "FOU.ThingInfo.Number.Tooltip".Translate()
                };

                var slider = new Slider
                {
                    MinValue = 1f,
                    MaxValue = _maxCount,
                    Value = 1f,
                    Tooltip = "FOU.ThingInfo.Number.Tooltip".Translate()
                };

                Binding.Create(slider, Slider.ValueProperty, _numberBox, NumberBox.ValueProperty, BindingMode.TwoWay);

                panel = new Grid { Margin = 4f }
                    .DefineColumns(34f, Grid.Remain, 34f, 220f)
                    .DefineRows(34f, 28f)
                    .Set(
                        icon, nameLabel, deleteButton, confirmButton,
                        slider, slider, slider, _numberBox);
            }
            else
            {
                panel = new Grid { Margin = 4f }
                    .DefineColumns(34f, Grid.Remain, 34f, 220f)
                    .DefineRows(34f)
                    .Set(icon, nameLabel, deleteButton, new Button { IsEnabled = false, Text = "FOU.ThingInfo.Confirm.Label".Translate(), Tooltip = "FOU.ThingInfo.Disabled.Tooltip".Translate().Colorize(ColoredText.WarningColor) });
            }

            _background.Content = panel;

            return _background;
        }

        protected override void OnMouseEnter(RoutedEventArgs e)
        {
            _background.Background = TexUI.HighlightSelectedTex;
            ThingWeaveWindow.potentialWeavingThing = this;
        }

        protected override void OnMouseLeave(RoutedEventArgs e)
        {
            _background.Background = BrushUtility.DarkGrey;
            ThingWeaveWindow.potentialWeavingThing = null;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void OnConfirm(object sender, RoutedEventArgs e)
        {
            ThingWeaveWindow.potentialWeavingThing = this;
            LayoutManager.Owner.OnAcceptKeyPressed();
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (this.TryFindPartent<ThingWeaveView>(out var view))
            {
                view.Comp.weavableThings.Remove(Model);
                view.Remove(this);
            }
        }

        #endregion



        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private NumberBox _numberBox;
        private Border _background;
        private readonly int _maxCount;

        #endregion
    }
}
