using ForeignerOfUniverse.Utilities;
using Verse;

namespace ForeignerOfUniverse.Genes
{
    public sealed class PhysicalAscension : Gene
    {
        public override bool Active => overriddenByGene is null && pawn.OwnNanites();


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override void PostAdd()
        {
            if (pawn.TryGetNanites(out var nanites))
            {
                nanites.OnAuthorityChanged();
            }
        }

        public override void PostRemove()
        {
            if (pawn.TryGetNanites(out var nanites))
            {
                nanites.OnAuthorityChanged();
            }
        }

        #endregion
    }
}
