using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Utilities;
using ForeignerOfUniverse.Views;
using HarmonyLib;
using Nebulae.RimWorld.UI;
using Nebulae.RimWorld.UI.Automation;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Grid = Nebulae.RimWorld.UI.Controls.Panels.Grid;

namespace ForeignerOfUniverse
{
    public sealed class FOU : NebulaeMod<FOUSettings>
    {
        public const string DebugLabel = "ForeignerOfUniverse";

        internal static readonly Harmony HarmonyInstance;


        static FOU()
        {
            HarmonyInstance = new Harmony("Nebulae.ForeignerOfUniverse");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public FOU(ModContentPack content) : base(content) { }


        public override string SettingsCategory()
        {
            return "FOU.Settings.Category.Label".Translate();
        }


        protected override Control CreateContent()
        {
            var resetButton = new Button
            {
                Margin = new Thickness(12f, 4f, 12f, 4f),
                Text = "FOU.Settings.ResetButton.Label".Translate(),
                Tooltip = "FOU.Settings.ResetButton.Tooltip".Translate()
            };
            resetButton.Click += (sender, args) =>
            {
                Settings.Reset();
                Window.Content = CreateContent();
            };

            var basicPanel = new Grid()
                .DefineRows(Grid.Remain, 38f)
                .Set(Settings.GenerateLayout(), resetButton);

            var panel = new TabControl()
                .Set(
                    new TabItem
                    {
                        Header = "FOU.Settings.Basic.Label".Translate(),
                        Content = basicPanel
                    },
                    new TabItem
                    {
                        Header = "FOU.Protocols.Ascension.Label".Translate(),
                        Tooltip = "FOU.Settings.Protocols.Tooltip".Translate("FOU.Protocols.Ascension.Label".Translate().Colorize(ColoredText.NameColor)),
                        Content = new ImmuneDiseasesView(ProtocolType.Ascension)
                    },
                    new TabItem
                    {
                        Header = "FOU.Protocols.NooNet.Label".Translate(),
                        Tooltip = "FOU.Settings.Protocols.Tooltip".Translate("FOU.Protocols.NooNet.Label".Translate().Colorize(ColoredText.NameColor)),
                        Content = new ImmuneDiseasesView(ProtocolType.NooNet)
                    },
                    new TabItem
                    {
                        Header = "FOU.Protocols.Phoenix.Label".Translate(),
                        Tooltip = "FOU.Settings.Protocols.Tooltip".Translate("FOU.Protocols.Phoenix.Label".Translate().Colorize(ColoredText.NameColor)),
                        Content = new ImmuneDiseasesView(ProtocolType.Phoenix)
                    });
            return panel;
        }


        protected override void OnInitializing()
        {
            var settings = Settings;

            if (settings.AscensionImmutableHediffs is null)
            {
                settings.AscensionImmutableHediffs = new HashSet<HediffInfo>(NaniteImmunityUtility.AscensionDefaultImmunizd);
            }
            else
            {
                settings.AscensionImmutableHediffs = new HashSet<HediffInfo>(settings.AscensionImmutableHediffs.Select(HediffInfo.Resolve));
            }

            if (settings.NooNetImmutableHediffs is null)
            {
                settings.NooNetImmutableHediffs = new HashSet<HediffInfo>(NaniteImmunityUtility.NooNetDefaultImmunizd);
            }
            else
            {
                settings.NooNetImmutableHediffs = new HashSet<HediffInfo>(settings.NooNetImmutableHediffs.Select(HediffInfo.Resolve));
            }

            if (settings.PhoenixImmutableHediffs is null)
            {
                settings.PhoenixImmutableHediffs = new HashSet<HediffInfo>(NaniteImmunityUtility.PhoenixDefaultImmunizd);
            }
            else
            {
                settings.PhoenixImmutableHediffs = new HashSet<HediffInfo>(settings.PhoenixImmutableHediffs.Select(HediffInfo.Resolve));
            }

            NaniteImmunityUtility.Initialize();
        }
    }
}
