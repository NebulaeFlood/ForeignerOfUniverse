using ForeignerOfUniverse.Models;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Verse;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingIconView : Control
    {
        public ThingIconView(ThingInfo model)
        {
            _def = model.DefInfo.Def;
            _stuffDef = model.StuffDefInfo.Def;
            _styleDef = model.StyleDefInfo.Def;
        }

        public ThingIconView(Thing thing)
        {
            _def = thing.def;
            _stuffDef = thing.Stuff;
            _styleDef = thing.StyleDef;
        }


        protected override void DrawCore(ControlState states)
        {
            Widgets.ThingIcon(DesiredRect, _def, _stuffDef, _styleDef);
        }


        private readonly ThingDef _def;
        private readonly ThingDef _stuffDef;
        private readonly ThingStyleDef _styleDef;
    }
}
