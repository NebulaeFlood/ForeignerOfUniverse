using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
