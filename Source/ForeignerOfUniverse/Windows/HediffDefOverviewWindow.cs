using ForeignerOfUniverse.Views;
using Nebulae.RimWorld.UI.Windows;

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
