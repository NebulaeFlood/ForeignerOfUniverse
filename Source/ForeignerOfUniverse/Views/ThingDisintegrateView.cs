using ForeignerOfUniverse.Windows;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Controls.Panels;
using Nebulae.RimWorld.UI.Core.Events;
using Nebulae.RimWorld.UI.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using TextBlock = Nebulae.RimWorld.UI.Controls.Basic.TextBlock;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingDisintegrateView : CompositeControl
    {
        internal ThingDisintegrateView(IEnumerable<Thing> disintegratableThings, string targetLabel)
        {
            _searchBox = new SearchBox();
            _searchBox.Search += OnSearch;

            _views = disintegratableThings
                .OrderBy(GetMass)
                .Select(ThingDisintegratableView.Convert)
                .ToArray();

            _infos = new StackPanel { Margin = 4f, Filter = Filter, VerticalAlignment = VerticalAlignment.Top }
                .Set(_views);

            _confirmTooltip = "FOU.ThingInfo.Disintegrate.Confirm.Tooltip".Translate();
            _confirmDisabledTooltip = $"{"FOU.ThingInfo.Disintegrate.Confirm.Tooltip".Translate()}\n\n{"FOU.NaniteAbility.DisintegrateTargetIsNull".Translate().Colorize(ColoredText.WarningColor)}";

            _confirmButton = new Button
            {
                Text = "FOU.ThingInfo.Disintegrate.Confirm.Label".Translate(),
                Tooltip = _confirmDisabledTooltip,
                IsEnabled = false
            };
            _confirmButton.Click += OnConfirm;

            _targetLabel = targetLabel;

            Initialize();
        }


        protected override Control CreateContent()
        {
            Grid grid;

            var cancleButton = new Button
            {
                Text = "CloseButton".Translate()
            };
            cancleButton.Click += OnCancle;

            if (_views.Length > 0)
            {
                var confirmAllButton = new Button
                {
                    Text = "FOU.ThingInfo.Disintegrate.Confirm.All.Label".Translate(),
                    Tooltip = "FOU.ThingInfo.Disintegrate.Confirm.All.Tooltip".Translate(_targetLabel.Colorize(ColoredText.NameColor))
                };
                confirmAllButton.Click += OnConfirmAll;

                var footPanel = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }.DefineColumns(140f, 20f, 140f, 20f, 140f)
                .DefineRows(34f)
                .Set(cancleButton, null, confirmAllButton, null, _confirmButton);

                var infoPanelBorder = new Border
                {
                    Background = BrushUtility.WindowBackground,
                    BorderBrush = BrushUtility.LightGrey,
                    BorderThickness = 1f,
                    Margin = new Thickness(0f, 4f, 0f, 0f),
                    Content = new ScrollViewer { Content = _infos, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden }
                };

                grid = new Grid { Margin = 4f }
                    .DefineRows(30f, Grid.Remain, 48f)
                    .Set(_searchBox, infoPanelBorder, footPanel);
            }
            else
            {
                cancleButton.Width = 140f;
                cancleButton.Height = 34f;

                var tip = new TextBlock
                {
                    AutoHeight = false,
                    FontSize = GameFont.Medium,
                    Text = "FOU.NaniteAbility.EmptyInventoryTip".Translate(_targetLabel.Colorize(ColoredText.NameColor)).Colorize(ColoredText.SubtleGrayColor),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                var tipBorder = new Border
                {
                    Background = BrushUtility.WindowBackground,
                    BorderBrush = BrushUtility.LightGrey,
                    BorderThickness = 1f,
                    Margin = new Thickness(0f, 4f, 0f, 0f),
                    Content = tip
                };

                grid = new Grid { Margin = 4f }
                    .DefineRows(30f, Grid.Remain, 48f)
                    .Set(_searchBox, tipBorder, cancleButton);
            }

            return new Border
            {
                Background = BrushUtility.DarkerGrey,
                BorderBrush = BrushUtility.LightGrey,
                BorderThickness = 1f,
                Content = grid
            };
        }


        //------------------------------------------------------
        //
        //  Internal Methods
        //
        //------------------------------------------------------

        #region Internal Methods

        internal void Deselect(ThingDisintegratableView view)
        {
            if (LayoutManager.Owner is ThingDisintegrateWindow window && window.selectedViews.Remove(view))
            {
                if (window.selectedViews.Count < 1 && _confirmButton.IsEnabled)
                {
                    _confirmButton.IsEnabled = false;
                    _confirmButton.Tooltip = _confirmDisabledTooltip;
                }
            }
        }

        internal void Select(ThingDisintegratableView view)
        {
            if (LayoutManager.Owner is ThingDisintegrateWindow window)
            {
                window.selectedViews.AddFirst(view);

                if (!_confirmButton.IsEnabled)
                {
                    _confirmButton.IsEnabled = true;
                    _confirmButton.Tooltip = _confirmTooltip;
                }
            }
        }

        #endregion


        private static float GetMass(Thing thing)
        {
            return thing.def.GetStatValueAbstract(StatDefOf.Mass, thing.Stuff);
        }


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private bool Filter(Control control)
        {
            return _searchBox.Matches(control.Name);
        }

        private void OnCancle(object sender, RoutedEventArgs args)
        {
            LayoutManager.Owner.OnCancelKeyPressed();
        }

        private void OnConfirm(object sender, RoutedEventArgs args)
        {
            LayoutManager.Owner.OnAcceptKeyPressed();
        }

        private void OnConfirmAll(object sender, RoutedEventArgs args)
        {
            if (LayoutManager.Owner is ThingDisintegrateWindow window)
            {
                window.selectedViews.Clear();

                foreach (var child in _infos.LogicalChildren.Cast<ThingDisintegratableView>())
                {
                    child.Count = child.Model.stackCount;
                    window.selectedViews.AddLast(child);
                }
            }

            LayoutManager.Owner.OnAcceptKeyPressed();
        }

        private void OnSearch(SearchBox sender, EventArgs args)
        {
            _infos.InvalidateFilter();
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly StackPanel _infos;
        private readonly SearchBox _searchBox;
        private readonly Button _confirmButton;

        private readonly ThingDisintegratableView[] _views;

        private readonly string _targetLabel;

        private readonly string _confirmTooltip;
        private readonly string _confirmDisabledTooltip;

        #endregion
    }
}
