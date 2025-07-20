using Verse;

namespace ForeignerOfUniverse.Comps.Hediffs
{
    public sealed class AlreadyTended : HediffComp_TendDuration
    {
        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties

        public override string CompDescriptionExtra => string.Empty;

        public override bool CompShouldRemove => false;

        public override TextureAndColor CompStateIcon => TextureAndColor.None;

        #endregion


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override string CompDebugString() => string.Empty;

        public override void CompPostPostAdd(DamageInfo? dinfo) => CompTended(1.4f, 1.4f, 1);

        public override void CompPostTick(ref float severityAdjustment) { }

        #endregion
    }


    public sealed class Properties_AlreadyTended : HediffCompProperties_TendDuration
    {
        public Properties_AlreadyTended()
        {
            compClass = typeof(AlreadyTended);
            showTendQuality = false;
        }
    }
}
