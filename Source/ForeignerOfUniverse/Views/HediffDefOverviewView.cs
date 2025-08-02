using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Controls.Panels;
using Nebulae.RimWorld.UI.Utilities;
using System;
using System.Linq;
using Verse;
using Grid = Nebulae.RimWorld.UI.Controls.Panels.Grid;

namespace ForeignerOfUniverse.Views
{
    internal sealed class HediffDefOverviewView : CompositeControl
    {
        public HediffDefOverviewView()
        {
            _searchBox = new SearchBox();
            _searchBox.Search += OnSearch;

            _infos = new StackPanel { Margin = 4f, Filter = Filter, VerticalAlignment = VerticalAlignment.Top }
                .Set(DefDatabase<HediffDef>.AllDefs.Select(ReadonlyHediffView.Convert));

            Initialize();
        }


        protected override Control CreateContent()
        {
            var infoPanel = new Border
            {
                Background = BrushUtility.WindowBackground,
                BorderBrush = BrushUtility.Grey,
                BorderThickness = 1f,
                Content = new ScrollViewer { Content = _infos, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden },
                Margin = new Thickness(0f, 4f, 0f, 0f),
                Padding = 4f
            };

            var grid = new Grid { Margin = 4f }
                .DefineRows(30f, Grid.Remain)
                .Set(_searchBox, infoPanel);

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

        #endregion]


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly StackPanel _infos;
        private readonly SearchBox _searchBox;

        #endregion
    }
}
