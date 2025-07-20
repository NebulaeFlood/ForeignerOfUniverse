using ForeignerOfUniverse.Views;
using Nebulae.RimWorld.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignerOfUniverse.Windows
{
    public sealed class HediffDefOverviewWindow : ControlWindow
    {
        public HediffDefOverviewWindow()
        {
            doCloseButton = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;

            Content = new HediffDefOverviewView();
        }
    }
}
