using ForeignerOfUniverse.Comps.AbilityEffects;
using ForeignerOfUniverse.Genes;
using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Views;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Windows;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ForeignerOfUniverse.Windows
{
    internal sealed class ThingWeaveWindow : ControlWindow
    {
        internal ThingWeaveWindow(MatterWeave comp, Action confirmAction)
        {
            _comp = comp;
            _confirmAction = confirmAction;

            absorbInputAroundWindow = true;
            closeOnCancel = false;
            doCloseButton = true;
            doCloseX = true;
            forcePause = true;
            preventCameraMotion = true;

            Content = new ThingWeaveView(comp);
        }


        public override void OnAcceptKeyPressed()
        {
            if (potentialWeavingThing != null)
            {
                _comp.weavingThing = potentialWeavingThing.Model;
                _comp.weavingThingCount = potentialWeavingThing.Count;

                potentialWeavingThing = null;

                Close();

                if (KeyBindingDefOf.Accept.KeyDownEvent)
                {
                    Event.current.Use();
                }
            }
        }

        public override void PostClose()
        {
            if (_comp.weavingThingCount > 0)
            {
                _confirmAction();
            }

            potentialWeavingThing = null;
        }


        internal static ThingView potentialWeavingThing;


        private readonly MatterWeave _comp;
        private readonly Action _confirmAction;
    }
}
