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
using Nebulae.RimWorld.UI.Windows;
using ForeignerOfUniverse.Windows;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ImmuneDiseasesView : CompositeControl
    {
        static ImmuneDiseasesView()
        {
            MarginProperty.OverrideMetadata(typeof(ImmuneDiseasesView),
                new ControlPropertyMetadata(new Thickness(8f), ControlRelation.Measure));
        }

        internal ImmuneDiseasesView(ProtocolType protocol)
        {
            Protocol = protocol;

            _infos = new StackPanel { Margin = 4f, VerticalAlignment = VerticalAlignment.Top };
            _textBox = new TextBox { Tooltip = "FOU.Settings.Protocols.ImmuneDiseases.InputBox.Tooltip".Translate() };

            switch (Protocol)
            {
                case ProtocolType.Ascension:
                    _infos.Set(FOU.Settings.AscensionImmutableHediffs.Select(HediffView.Convert));
                    break;
                case ProtocolType.NooNet:
                    _infos.Set(FOU.Settings.NooNetImmutableHediffs.Select(HediffView.Convert));
                    break;
                case ProtocolType.Phoenix:
                    _infos.Set(FOU.Settings.PhoenixImmutableHediffs.Select(HediffView.Convert));
                    break;
                default:
                    break;
            }

            Initialize();
        }


        internal void Remove(HediffView view)
        {
            _infos.Remove(view);
            NaniteImmunityUtility.Remove(Protocol, view.Model);
        }


        protected override Control CreateContent()
        {
            var addButton = new IconButton(TexButton.Plus)
            {
                Tooltip = "FOU.Settings.Protocols.ImmuneDiseases.AddButton.Tooltip".Translate()
            };
            addButton.Click += OnAdd;

            var listButton = new IconButton(CaravanThingsTabUtility.SpecificTabButtonTex)
            {
                Tooltip = "FOU.Settings.Protocols.ImmuneDiseases.OverviewButton.Tooltip".Translate()
            };
            listButton.Click += OnListAll;

            var resetButton = new IconButton(TexButton.HotReloadDefs)
            {
                Tooltip = "FOU.Settings.Protocols.ImmuneDiseases.ResetButton.Tooltip".Translate()
            };
            resetButton.Click += OnReset;

            var infoPanel = new Border
            {
                Background = BrushUtility.WindowBackground,
                BorderBrush = BrushUtility.LightGrey,
                BorderThickness = 1f,
                Content = new ScrollViewer { Content = _infos, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden },
                Padding = 8f
            };

            var joinPanel = new Grid()
                .DefineColumns(34f, Grid.Remain, 34f, 34f)
                .DefineRows(34f)
                .Set(addButton, _textBox, listButton, resetButton);

            var joinPanelBorder = new Border
            {
                Background = BrushUtility.WindowBackground,
                BorderBrush = BrushUtility.LightGrey,
                BorderThickness = 1f,
                Content = joinPanel,
                Margin = new Thickness(0f, 4f, 0f, 0f)
            };

            return new Grid()
                .DefineRows(Grid.Remain, Grid.Auto)
                .Set(infoPanel, joinPanelBorder);
        }


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void OnAdd(object sender, RoutedEventArgs args)
        {
            var info = HediffInfo.From(_textBox.Text);

            if (!info.Loaded)
            {
                Messages.Message("FOU.Messages.Settings.Protocols.CannotFindDef.Content".Translate(info.DefName.Colorize(ColoredText.NameColor)).Resolve(), 
                    MessageTypeDefOf.RejectInput, historical: false);
                return;
            }

            if (NaniteImmunityUtility.TryAdd(Protocol, info))
            {
                _infos.Append(new HediffView(info));
                _textBox.Text = string.Empty;
            }
        }

        private void OnListAll(object sender, RoutedEventArgs args)
        {
            new HediffDefOverviewWindow().Show();
        }

        private void OnReset(object sender, RoutedEventArgs args)
        {
            string header;

            switch (Protocol)
            {
                case ProtocolType.Ascension:
                    header = "FOU.Protocols.Ascension.Label".Translate();
                    break;
                case ProtocolType.NooNet:
                    header = "FOU.Protocols.NooNet.Label".Translate();
                    break;
                case ProtocolType.Phoenix:
                    header = "FOU.Protocols.Phoenix.Label".Translate();
                    break;
                default:
                    header = string.Empty;
                    break;
            }

            new Dialog_MessageBox("FOU.Messages.Settings.Protocols.ComfirmReset.Content".Translate(header.Colorize(ColoredText.NameColor)),
                buttonAAction: ResetThis, buttonBText: "GoBack".Translate(), buttonADestructive: true).Show();

            void ResetThis()
            {
                _infos.Clear();
                _infos.Set(
                    NaniteImmunityUtility.SetDefault(Protocol)
                    .Select(HediffView.Convert));
            }
        }

        #endregion


        internal readonly ProtocolType Protocol;


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly StackPanel _infos;
        private readonly TextBox _textBox;

        #endregion
    }
}
