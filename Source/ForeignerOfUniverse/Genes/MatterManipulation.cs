using ForeignerOfUniverse.Hediffs;
using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Utilities;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ForeignerOfUniverse.Genes
{
    public sealed class MatterManipulation : Gene
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
