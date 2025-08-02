using Nebulae.RimWorld;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ForeignerOfUniverse.Models
{
    internal readonly struct ThingWeaveInfo
    {
        public readonly ThingDef Def;
        public readonly ThingDef StuffDef;
        public readonly ThingStyleDef StyleDef;

        public readonly bool Qualified;
        public readonly QualityCategory Quality;

        public readonly bool AnyBladelinkTrait;
        public readonly WeaponTraitDef[] BladelinkTraits;

        public readonly bool AnyUniqueTrait;
        public readonly WeaponTraitDef[] UniqueTraits;


        internal ThingWeaveInfo(ThingInfo info)
        {
            Def = info.DefInfo.Def;
            StuffDef = info.StuffDefInfo.Def;
            StyleDef = info.StyleDefInfo.Def;

            Qualified = info.Qualified;
            Quality = info.Quality;

            if (info.AnyBladelinkTrait)
            {
                AnyBladelinkTrait = true;
                BladelinkTraits = info.BladelinkTraits.Select(DefInfo<WeaponTraitDef>.GetDef).ToArray();
            }
            else
            {
                AnyBladelinkTrait = false;
                BladelinkTraits = null;
            }

            if (info.AnyUniqueTrait)
            {
                AnyUniqueTrait = true;
                UniqueTraits = info.UniqueTraits.Select(DefInfo<WeaponTraitDef>.GetDef).ToArray();
            }
            else
            {
                AnyUniqueTrait = false;
                UniqueTraits = null;
            }
        }


        public Thing Weave(int count)
        {
            int spawnCount = count;
            var thing = ThingMaker.MakeThing(Def, StuffDef);

            thing.StyleDef = StyleDef;

            if (Qualified && thing.TryGetComp<CompQuality>(out var quality))
            {
                quality.SetQuality(Quality, ArtGenerationContext.Colony);
            }

            if (AnyBladelinkTrait && thing.TryGetComp<CompBladelinkWeapon>(out var bladelinkWeapon))
            {
                bladelinkWeapon.TraitsListForReading.Clear();
                bladelinkWeapon.TraitsListForReading.AddRange(BladelinkTraits);
            }

            if (AnyUniqueTrait && thing.TryGetComp<CompUniqueWeapon>(out var uniqueWeapon))
            {
                uniqueWeapon.TraitsListForReading.Clear();
                uniqueWeapon.TraitsListForReading.AddRange(UniqueTraits);
            }

            if (Def.minifiedDef != null)
            {
                thing = MinifyUtility.MakeMinified(thing);
            }

            thing.stackCount = spawnCount;
            return thing;
        }
    }
}
