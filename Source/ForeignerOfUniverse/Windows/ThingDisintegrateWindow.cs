using ForeignerOfUniverse.Comps.AbilityEffects;
using ForeignerOfUniverse.Views;
using Nebulae.RimWorld.UI.Windows;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ForeignerOfUniverse.Windows
{
    [StaticConstructorOnStartup]
    internal sealed class ThingDisintegrateWindow : ControlWindow
    {
        public readonly MatterDisintegration Comp;


        internal ThingDisintegrateWindow(MatterDisintegration comp,IEnumerable<Thing> things, string targetLabel, Action confirmAction)
        {
            _confirmAction = confirmAction;

            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            doCloseButton = false;
            doCloseX = true;
            forcePause = true;
            preventCameraMotion = true;

            Comp = comp;
            Content = new ThingDisintegrateView(things, targetLabel);
        }


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override void OnAcceptKeyPressed()
        {
            if (selectedViews.Count > 0)
            {
                _confirmed = true;
                Close();
            }
            else
            {
                Messages.Message("FOU.NaniteAbility.DisintegrateTargetIsNull".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            }
        }

        public override void OnCancelKeyPressed()
        {
            selectedViews.Clear();
            base.OnCancelKeyPressed();
        }

        public override void PostClose()
        {
            if (_confirmed)
            {
                Comp.disintegrateQueue = selectedViews;
                _confirmAction();
            }
        }

        #endregion


        internal readonly LinkedList<ThingDisintegratableView> selectedViews = new LinkedList<ThingDisintegratableView>();


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly Action _confirmAction;
        private bool _confirmed;

        #endregion
    }
}
