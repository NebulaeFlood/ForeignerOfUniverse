using ForeignerOfUniverse.Models;
using Mono.Security.Cryptography;
using Nebulae.RimWorld.UI.Automation;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Controls.Panels;
using Nebulae.RimWorld.UI.Core.Events;
using Nebulae.RimWorld.UI.Utilities;
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
    internal sealed class HediffView : CompositeControl
    {
        public readonly HediffInfo Model;


        static HediffView()
        {
            MarginProperty.OverrideMetadata(typeof(ImmuneDiseasesView),
                new ControlPropertyMetadata(new Thickness(4f), ControlRelation.Measure));
        }

        internal HediffView(HediffInfo model)
        {
            Model = model;
            Initialize();
        }


        public static HediffView Convert(HediffInfo model)
        {
            return new HediffView(model);
        }


        protected override Control CreateContent()
        {
            var displayName = Model.IsValid ? Model.Def.LabelCap.ToString() : Model.DefName;

            Name = displayName;

            var deleteButton = new Button
            {
                Margin = new Thickness(12f, 4f, 12f, 4f),
                Text = "FOU.HediffInfo.Delete.Label".Translate(),
                Tooltip = "FOU.HediffInfo.Delete.Tooltip".Translate(displayName.Colorize(ColoredText.NameColor))
            };
            deleteButton.Click += OnDelete;

            var expander = new Expander
            {
                Header = displayName,
                Tooltip = "FOU.HediffInfo.Name.Tooltip".Translate(),
                Content = new Border
                {
                    Background = BrushUtility.WindowBackground,
                    BorderBrush = BrushUtility.Grey,
                    BorderThickness = 1f,
                    Content = Model.GenerateLayout().Append(deleteButton),
                    Margin = new Thickness(0f, 0f, 4f, 4f)
                }
            };
            expander.Click += OnHeaderClick;

            return new Border
            {
                Background = BrushUtility.DarkGrey,
                BorderBrush = BrushUtility.Grey,
                BorderThickness = 1f,
                Content = expander
            };
        }


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void OnHeaderClick(object sender, RoutedEventArgs e)
        {
            if (sender is Expander expander)
            {
                expander.IsExpanded = !expander.IsExpanded;
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (this.TryFindPartent<ImmuneDiseasesView>(out var view))
            {
                view.Remove(this);
            }
        }

        #endregion
    }
}
