using ForeignerOfUniverse.Models;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingIconView : Control
    {
        public readonly ThingInfo Model;


        public ThingIconView(ThingInfo model)
        {
            Model = model;
        }


        protected override void DrawCore(ControlState states)
        {
            Widgets.ThingIcon(DesiredRect, Model.DefInfo.Def, Model.StuffDefInfo.Def, Model.StyleDefInfo.Def);
        }
    }
}
