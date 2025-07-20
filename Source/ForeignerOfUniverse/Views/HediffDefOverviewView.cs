using ForeignerOfUniverse.Models;
using Mono.Security.Cryptography;
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
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld.Planet;
using Grid = Nebulae.RimWorld.UI.Controls.Panels.Grid;
using ForeignerOfUniverse.Genes;
using ForeignerOfUniverse.Utilities;

namespace ForeignerOfUniverse.Views
{
    internal sealed class HediffDefOverviewView : CompositeControl
    {
        public HediffDefOverviewView()
        {
            _searchBox = new SearchBox { Margin = new Thickness(0f, 6f, 4f, 6f) };
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
                Padding = 8f
            };

            var grid = new Grid()
                .DefineRows(36f, Grid.Remain)
                .Set(_searchBox, infoPanel);

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

        #endregion]


        private readonly StackPanel _infos;
        private readonly SearchBox _searchBox;  
    }
}
