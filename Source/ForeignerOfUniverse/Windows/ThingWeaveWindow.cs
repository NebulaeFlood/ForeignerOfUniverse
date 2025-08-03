using ForeignerOfUniverse.Comps.AbilityEffects;
using ForeignerOfUniverse.Views;
using Nebulae.RimWorld.UI.Windows;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace ForeignerOfUniverse.Windows
{
    [StaticConstructorOnStartup]
    internal sealed class ThingWeaveWindow : ControlWindow
    {
        public readonly MatterWeave Comp;


        internal ThingWeaveWindow(MatterWeave comp, Action confirmAction)
        {
            Comp = comp;

            _confirmAction = confirmAction;
            _view = new ThingWeaveView(comp);

            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            doCloseButton = false;
            doCloseX = true;
            forcePause = true;
            preventCameraMotion = true;

            Content = _view;
        }


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override void OnAcceptKeyPressed()
        {
            if (_view.selectedViews.Count > 0)
            {
                _confirmed = true;
                Close();
            }
            else
            {
                Messages.Message("FOU.NaniteAbility.WeaveTargetIsNull".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            }
        }

        public override void OnCancelKeyPressed()
        {
            _view.selectedViews.Clear();
            base.OnCancelKeyPressed();
        }

        public override void PostClose()
        {
            if (_confirmed)
            {
                Comp.weaveQueue = _view.selectedViews;
                _confirmAction();
            }

            Comp.weavePolicies = _view.OrderedProlicies.ToList();
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly Action _confirmAction;
        private bool _confirmed;

        private readonly ThingWeaveView _view;

        #endregion
    }
}
