using ForeignerOfUniverse.Comps.AbilityEffects;
using ForeignerOfUniverse.Genes;
using ForeignerOfUniverse.Models;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Controls.Panels;
using Nebulae.RimWorld.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using TextBlock = Nebulae.RimWorld.UI.Controls.Basic.TextBlock;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingWeaveView : CompositeControl
    {
        public readonly MatterWeave Comp;


        internal ThingWeaveView(MatterWeave comp)
        {
            var naniteStore = comp.parent.pawn.genes.GetFirstGeneOfType<HigherDimensionalNanites>().Value * 100f;
            var unit = FOU.Settings.KilogramsPerNanite;

            ThingView Convert(ThingInfo info)
            {
                return new ThingView(info, naniteStore, unit);
            }

            _searchBox = new SearchBox { Margin = new Thickness(0f, 6f, 4f, 6f) };
            _searchBox.Search += OnSearch;

            _views = comp.weavableThings
                .Where(ThingInfo.Valid)
                .OrderBy(ThingInfo.GetMass)
                .Select(Convert)
                .ToArray();

            _infos = new StackPanel { Margin = 4f, Filter = Filter, VerticalAlignment = VerticalAlignment.Top }
                .Set(_views);

            Comp = comp;

            Initialize();
        }


        internal void Remove(Control control)
        {
            _infos.Remove(control);
        }


        protected override Control CreateContent()
        {
            Grid grid;

            if (_views.Length > 0)
            {
                var infoPanelBorder = new Border
                {
                    Background = BrushUtility.WindowBackground,
                    BorderBrush = BrushUtility.Grey,
                    BorderThickness = 1f,
                    Content = new ScrollViewer { Content = _infos, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden }
                };

                grid = new Grid() 
                    .DefineRows(36f, Grid.Remain)
                    .Set(_searchBox, infoPanelBorder);
            }
            else
            {
                var tip = new TextBlock
                {
                    AutoHeight = false,
                    FontSize = GameFont.Medium,
                    Text = "FOU.NaniteAbility.EmptyTip".Translate(FOUDefOf.FOU_MatterDisintegration.LabelCap.Colorize(ColoredText.NameColor)).Colorize(ColoredText.SubtleGrayColor),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                var tipBorder = new Border
                {
                    Background = BrushUtility.WindowBackground,
                    BorderBrush = BrushUtility.Grey,
                    BorderThickness = 1f,
                    Content = tip
                };

                grid = new Grid()
                    .DefineRows(36f, Grid.Remain)
                    .Set(_searchBox, tipBorder);
            }

            return new Border
            {
                Background = BrushUtility.DarkGrey,
                BorderBrush = BrushUtility.Grey,
                BorderThickness = 1f,
                Content = grid
            };
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
        private readonly ThingView[] _views;

        #endregion
    }
}
