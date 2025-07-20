using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Utilities;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ForeignerOfUniverse.Genes
{
    public sealed class NanoSynapse : Gene
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

        public override void Reset()
        {
            if (pawn.TryGetNanites(out var nanites))
            {
                nanites.protocol.PsychicShildOpen = false;
                nanites.protocol.PsychicShild = null;
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
