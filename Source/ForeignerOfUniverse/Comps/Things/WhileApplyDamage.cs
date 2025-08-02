using ForeignerOfUniverse.Genes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ForeignerOfUniverse.Comps.Things
{
    internal sealed class WhileApplyDamage : ThingComp
    {
        public HigherDimensionalNanites nanites;


        public override void PostExposeData()
        {
            Scribe_References.Look(ref nanites, "Nanites");

            if (Scribe.mode is LoadSaveMode.PostLoadInit && nanites is null)
            {
                parent.AllComps.Remove(this);
            }
        }


        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            nanites.PreApplyDamage(ref dinfo, out absorbed);
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            nanites.PostApplyDamage(dinfo);
        }
    }
}
