using ForeignerOfUniverse.Comps.AbilityEffects;
using ForeignerOfUniverse.Genes;
using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Windows;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Controls.Panels;
using Nebulae.RimWorld.UI.Core.Events;
using Nebulae.RimWorld.UI.Utilities;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using TextBlock = Nebulae.RimWorld.UI.Controls.Basic.TextBlock;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingWeaveView : CompositeControl
    {
        internal ThingWeaveView(MatterWeave comp)
        {
            var naniteStore = comp.parent.pawn.genes.GetFirstGeneOfType<HigherDimensionalNanites>().Value * 100f;
            var unit = FOU.Settings.KilogramsPerNanite;
            var inWorldCaravan = comp.parent.pawn.MapHeld != null && comp.parent.pawn.GetCaravan() != null;

            _selectedNullTooltip = $"\n\n{"FOU.NaniteAbility.WeaveTargetIsNull".Translate().Colorize(ColoredText.WarningColor)}";

            _asPolicyTooltip = "FOU.ThingInfo.Weave.AsPolicy.Tooltip".Translate();

            _asPolicyButton = new Button
            {
                Text = "FOU.ThingInfo.Weave.AsPolicy.Label".Translate(),
                Tooltip = _asPolicyTooltip + _selectedNullTooltip,
                IsEnabled = false
            };
            _asPolicyButton.Click += OnAsPolicy;

            _confirmTooltip = inWorldCaravan
                ? "FOU.ThingInfo.Weave.ConfirmInCaravan.Tooltip".Translate()
                : "FOU.ThingInfo.Weave.Confirm.Tooltip".Translate();

            _confirmButton = new Button
            {
                Text = "FOU.ThingInfo.Weave.Confirm.Label".Translate(),
                Tooltip = _confirmTooltip + _selectedNullTooltip,
                IsEnabled = false
            };
            _confirmButton.Click += OnConfirm;

            _searchBox = new SearchBox();
            _searchBox.Search += OnSearch;
            _searchBox.Focus();

            ThingWeavableView Convert(ThingInfo info)
            {
                return new ThingWeavableView(info, naniteStore, unit);
            }

            var thingViews = comp.weavableThings
                .Where(ThingInfo.IsLoaded)
                .OrderBy(ThingInfo.GetMass)
                .Select(Convert);
            var policyViews = comp.weavePolicies.Select(ThingWeavePolicyView.Convert);

            _anyWeavableThing = thingViews.Any();

            _infos = new StackPanel { Margin = 4f, Filter = Filter, VerticalAlignment = VerticalAlignment.Top }.Set(thingViews);
            _policies = new StackPanel { Margin = 4f, Filter = Filter, VerticalAlignment = VerticalAlignment.Top }.Set(policyViews);

            Initialize();
        }


        protected override Control CreateContent()
        {
            Grid grid;

            var cancleButton = new Button { Text = "CloseButton".Translate() };
            cancleButton.Click += OnCancle;

            if (_anyWeavableThing)
            {
                var footPanel = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                }.DefineColumns(140f, 20f, 140f, 20f, 140f)
                .DefineRows(34f)
                .Set(cancleButton, null, _asPolicyButton, null, _confirmButton);

                var policyPanelBorder = new Border
                {
                    Background = BrushUtility.WindowBackground,
                    BorderBrush = BrushUtility.LightGrey,
                    BorderThickness = 1f,
                    Margin = new Thickness(0f, 4f, 4f, 0f),
                    Content = new ScrollViewer { Content = _policies, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden }
                };

                var infoPanelBorder = new Border
                {
                    Background = BrushUtility.WindowBackground,
                    BorderBrush = BrushUtility.LightGrey,
                    BorderThickness = 1f,
                    Margin = new Thickness(0f, 4f, 0f, 0f),
                    Content = new ScrollViewer { Content = _infos, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden }
                };

                grid = new Grid { Margin = 4f }
                    .DefineColumns(0.35f, 0.65f)
                    .DefineRows(30f, Grid.Remain, 48f)
                    .Set(
                        _searchBox, _searchBox,
                        policyPanelBorder, infoPanelBorder,
                        policyPanelBorder, footPanel);
            }
            else
            {
                cancleButton.Width = 140f;
                cancleButton.Height = 34f;

                var tip = new TextBlock
                {
                    AutoHeight = false,
                    FontSize = GameFont.Medium,
                    Text = "FOU.NaniteAbility.EmptyRecordTip".Translate(FOUDefOf.FOU_MatterDisintegration.LabelCap.Colorize(ColoredText.NameColor)).Colorize(ColoredText.SubtleGrayColor),
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

        internal void Deselect(ThingWeavableView view)
        {
            if (selectedViews.Remove(view))
            {
                if (selectedViews.Count < 1 && _confirmButton.IsEnabled)
                {
                    _confirmButton.IsEnabled = false;
                    _confirmButton.Tooltip = _confirmTooltip + _selectedNullTooltip;

                    _asPolicyButton.IsEnabled = false;
                    _asPolicyButton.Tooltip = _asPolicyTooltip + _selectedNullTooltip;
                }

                DeselectPolicy();
            }
        }

        internal void DeselectPolicy()
        {
            if (_selectedPolicy is null)
            {
                return;
            }

            _selectedPolicy.Selected = false;
            _selectedPolicy = null;
        }

        internal void Remove(ThingWeavableView view)
        {
            _infos.Remove(view);
            Deselect(view);
        }

        internal void Remove(ThingWeavePolicyView view)
        {
            _policies.Remove(view);

            if (ReferenceEquals(view, _selectedPolicy))
            {
                _selectedPolicy = null;
            }
        }

        internal void Select(ThingWeavableView view)
        {
            selectedViews.AddFirst(view);

            if (!_confirmButton.IsEnabled)
            {
                _confirmButton.IsEnabled = true;
                _confirmButton.Tooltip = _confirmTooltip;

                _asPolicyButton.IsEnabled = true;
                _asPolicyButton.Tooltip = _asPolicyTooltip;
            }

            DeselectPolicy();
        }

        internal void Select(ThingWeavePolicyView view)
        {
            var node = selectedViews.First;

            while (node != null)
            {
                node.Value.Selected = false;
                node = node.Next;
            }

            selectedViews.Clear();

            foreach (var child in _infos.LogicalChildren.Cast<ThingWeavableView>())
            {
                if (view.Model.Contains(child.Model))
                {
                    child.Count = view.Model[child.Model];

                    var currentCount = child.Count;

                    if (currentCount > 0)
                    {
                        child.Selected = true;
                        selectedViews.AddLast(child);
                    }
                }
            }

            DeselectPolicy();

            _selectedPolicy = view;

            if (_confirmButton.IsEnabled)
            {
                if (selectedViews.Count < 1)
                {
                    _confirmButton.IsEnabled = false;
                    _confirmButton.Tooltip = _confirmTooltip + _selectedNullTooltip;

                    _asPolicyButton.IsEnabled = false;
                    _asPolicyButton.Tooltip = _asPolicyTooltip + _selectedNullTooltip;
                }
            }
            else
            {
                if (selectedViews.Count > 0)
                {
                    _confirmButton.IsEnabled = true;
                    _confirmButton.Tooltip = _confirmTooltip;

                    _asPolicyButton.IsEnabled = true;
                    _asPolicyButton.Tooltip = _asPolicyTooltip;
                }
            }
        }

        #endregion


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

        private void OnAsPolicy(object sender, RoutedEventArgs args)
        {
            var comp = ((ThingWeaveWindow)LayoutManager.Owner).Comp;
            var policy = new ThingWeavePolicy(ProcessSelectedViews(), comp.weavePolicies.Count + 1);
            var policyView = new ThingWeavePolicyView(policy) { Selected = true };

            comp.weavePolicies.Add(policy);
            _policies.Append(policyView);

            if (_selectedPolicy != null)
            {
                _selectedPolicy.Selected = false;
            }

            _selectedPolicy = policyView;
        }

        private void OnCancle(object sender, RoutedEventArgs args)
        {
            LayoutManager.Owner.OnCancelKeyPressed();
        }

        private void OnConfirm(object sender, RoutedEventArgs args)
        {
            LayoutManager.Owner.OnAcceptKeyPressed();
        }

        private void OnSearch(SearchBox sender, EventArgs args)
        {
            _infos.InvalidateFilter();
            _policies.InvalidateFilter();
        }

        private IEnumerable<ThingInfo> ProcessSelectedViews()
        {
            var node = selectedViews.First;

            while (node != null)
            {
                var view = node.Value;
                yield return new ThingInfo(view.Model, view.Count);
                node = node.Next;
            }
        }

        #endregion


        internal readonly LinkedList<ThingWeavableView> selectedViews = new LinkedList<ThingWeavableView>();


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly StackPanel _infos;
        private readonly StackPanel _policies;
        private readonly SearchBox _searchBox;
        private readonly Button _confirmButton;
        private readonly Button _asPolicyButton;

        private readonly bool _anyWeavableThing;

        private readonly string _confirmTooltip;
        private readonly string _asPolicyTooltip;

        private readonly string _selectedNullTooltip;

        private ThingWeavePolicyView _selectedPolicy;

        #endregion
    }
}
