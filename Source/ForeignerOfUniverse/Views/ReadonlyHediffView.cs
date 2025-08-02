using ForeignerOfUniverse.Models;
using Nebulae.RimWorld.UI.Automation;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Core.Events;
using Nebulae.RimWorld.UI.Utilities;
using Verse;

namespace ForeignerOfUniverse.Views
{
    internal class ReadonlyHediffView : CompositeControl
    {
        public readonly HediffInfo Model;


        internal ReadonlyHediffView(HediffInfo model)
        {
            Model = model;
            Initialize();
        }


        public static ReadonlyHediffView Convert(HediffDef def)
        {
            return new ReadonlyHediffView(new HediffInfo(def));
        }


        protected override Control CreateContent()
        {
            var displayName = Model.Def.LabelCap.ToString();

            Name = displayName;

            var expander = new Expander
            {
                Header = displayName,
                Tooltip = "FOU.HediffInfo.Name.Tooltip".Translate(),
                Content = new Border
                {
                    Background = BrushUtility.WindowBackground,
                    BorderBrush = BrushUtility.LightGrey,
                    BorderThickness = 1f,
                    Content = Model.GenerateLayout(),
                    Margin = new Thickness(0f, 0f, 4f, 4f)
                }
            };
            expander.Click += OnHeaderClick;

            return new Border
            {
                Background = BrushUtility.DarkerGrey,
                BorderBrush = BrushUtility.LightGrey,
                BorderThickness = 1f,
                Content = expander
            };
        }


        private void OnHeaderClick(object sender, RoutedEventArgs e)
        {
            if (sender is Expander expander)
            {
                expander.IsExpanded = !expander.IsExpanded;
            }
        }
    }
}
