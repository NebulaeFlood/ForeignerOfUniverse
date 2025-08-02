using Nebulae.RimWorld;
using Nebulae.RimWorld.UI.Automation.Attributes;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ForeignerOfUniverse.Models
{
    internal struct ThingInfo : IEquatable<ThingInfo>, IExposable
    {
        public static readonly ThingInfo Empty = new ThingInfo();


        //------------------------------------------------------
        //
        //  Public Fields
        //
        //------------------------------------------------------

        #region Public Fields

        public int Count;

        public DefInfo<ThingDef> DefInfo;
        public DefInfo<ThingDef> StuffDefInfo;
        public DefInfo<ThingStyleDef> StyleDefInfo;

        public bool Qualified;
        public QualityCategory Quality;

        public bool AnyBladelinkTrait;
        public List<DefInfo<WeaponTraitDef>> BladelinkTraits;

        public bool AnyUniqueTrait;
        public List<DefInfo<WeaponTraitDef>> UniqueTraits;

        public bool Loaded;

        #endregion


        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties

        public string Description
        {
            get
            {
                var anyBladelinkTrait = AnyBladelinkTrait && BladelinkTraits.Count > 0;
                var anyUniqueTrait = AnyUniqueTrait && UniqueTraits.Count > 0;
                var description = DefInfo.Def.description;

                if (anyBladelinkTrait || anyUniqueTrait)
                {
                    description += $"\n\n{"WeaponTraits".Translate().Colorize(ColoredText.TipSectionTitleColor)}:\n";

                    if (anyBladelinkTrait)
                    {
                        for (int i = 0; i < BladelinkTraits.Count; i++)
                        {
                            description += $"\n{BladelinkTraits[i].Def.LabelCap.Colorize(ColoredText.TipSectionTitleColor)}\n{BladelinkTraits[i].Def.description}\n";
                        }
                    }

                    if (anyUniqueTrait)
                    {
                        for (int i = 0; i < UniqueTraits.Count; i++)
                        {
                            description += $"\n{UniqueTraits[i].Def.LabelCap.Colorize(ColoredText.TipSectionTitleColor)}\n{UniqueTraits[i].Def.description}\n";
                        }
                    }
                }

                return description;
            }
        }

        public string LabelCap
        {
            get
            {
                var label = DefInfo.Def.MadeFromStuff
                    ? "ThingMadeOfStuffLabel".Translate(StuffDefInfo.Def.LabelAsStuff, DefInfo.Def.label)
                    : DefInfo.Def.LabelCap;

                return Qualified ? $"{label} ({Quality.GetLabel()})" : label.Resolve();
            }
        }

        #endregion


        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors

        internal ThingInfo(Thing thing)
        {
            Count = 0;
            DefInfo = new DefInfo<ThingDef>(thing.def);

            _hashCode = DefInfo.GetHashCode();

            if (thing.Stuff is null)
            {
                StuffDefInfo = DefInfo<ThingDef>.Empty;
            }
            else
            {
                StuffDefInfo = new DefInfo<ThingDef>(thing.Stuff);

                _hashCode ^= StuffDefInfo.GetHashCode();
            }

            if (thing.StyleDef is null)
            {
                StyleDefInfo = DefInfo<ThingStyleDef>.Empty;
            }
            else
            {
                StyleDefInfo = new DefInfo<ThingStyleDef>(thing.StyleDef);

                _hashCode ^= StyleDefInfo.GetHashCode();
            }

            Qualified = thing.TryGetQuality(out Quality);

            if (Qualified)
            {
                _hashCode ^= Quality.GetHashCode();
            }

            if (thing.TryGetComp<CompBladelinkWeapon>(out var bladelinkWeapon))
            {
                AnyBladelinkTrait = true;
                BladelinkTraits = bladelinkWeapon.TraitsListForReading.Select(AsDefInfo).ToList();

                for (int i = BladelinkTraits.Count - 1; i >= 0; i--)
                {
                    _hashCode ^= BladelinkTraits[i].GetHashCode();
                }
            }
            else
            {
                AnyBladelinkTrait = false;
                BladelinkTraits = null;
            }

            if (thing.TryGetComp<CompUniqueWeapon>(out var uniqueWeapon))
            {
                AnyUniqueTrait = true;
                UniqueTraits = uniqueWeapon.TraitsListForReading.Select(AsDefInfo).ToList();

                for (int i = UniqueTraits.Count - 1; i >= 0; i--)
                {
                    _hashCode ^= UniqueTraits[i].GetHashCode();
                }
            }
            else
            {
                AnyUniqueTrait = false;
                UniqueTraits = null;
            }

            Loaded = true;
        }

        internal ThingInfo(ThingInfo thing, int count)
        {
            Count = count;

            DefInfo = thing.DefInfo;
            StuffDefInfo = thing.StuffDefInfo;
            StyleDefInfo = thing.StyleDefInfo;

            Qualified = thing.Qualified;
            Quality = thing.Quality;

            AnyBladelinkTrait = thing.AnyBladelinkTrait;
            BladelinkTraits = thing.BladelinkTraits;

            AnyUniqueTrait = thing.AnyUniqueTrait;
            UniqueTraits = thing.UniqueTraits;

            Loaded = true;

            _hashCode = thing._hashCode;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override bool Equals(object obj)
        {
            return obj is ThingInfo other
                && DefInfo.Equals(other.DefInfo)
                && StuffDefInfo.Equals(other.StuffDefInfo)
                && StyleDefInfo.Equals(other.StyleDefInfo)
                && Quality == other.Quality
                && (!AnyBladelinkTrait || (other.AnyBladelinkTrait && BladelinkTraits.SequenceEqual(other.BladelinkTraits)))
                && (!AnyUniqueTrait || (other.AnyUniqueTrait && UniqueTraits.SequenceEqual(other.UniqueTraits)));
        }

        public bool Equals(ThingInfo other)
        {
            return DefInfo.Equals(other.DefInfo)
                && StuffDefInfo.Equals(other.StuffDefInfo)
                && StyleDefInfo.Equals(other.StyleDefInfo)
                && Quality == other.Quality
                && (!AnyBladelinkTrait || (other.AnyBladelinkTrait && BladelinkTraits.SequenceEqual(other.BladelinkTraits)))
                && (!AnyUniqueTrait || (other.AnyUniqueTrait && UniqueTraits.SequenceEqual(other.UniqueTraits)));
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Count, nameof(Count), defaultValue: 0);

            Scribe_Deep.Look(ref DefInfo, nameof(DefInfo));
            Scribe_Deep.Look(ref StuffDefInfo, nameof(StuffDefInfo));
            Scribe_Deep.Look(ref StyleDefInfo, nameof(StyleDefInfo));

            Scribe_Values.Look(ref Qualified, nameof(Qualified), defaultValue: false);
            Scribe_Values.Look(ref Quality, nameof(Quality), defaultValue: QualityCategory.Normal);

            Scribe_Values.Look(ref AnyBladelinkTrait, nameof(AnyBladelinkTrait), defaultValue: false);
            Scribe_Collections.Look(ref BladelinkTraits, nameof(BladelinkTraits), LookMode.Deep);

            Scribe_Values.Look(ref AnyUniqueTrait, nameof(AnyUniqueTrait), defaultValue: false);
            Scribe_Collections.Look(ref UniqueTraits, nameof(UniqueTraits), LookMode.Deep);

            Scribe_Values.Look(ref _hashCode, "HashCode", defaultValue: 0);
        }

        public override int GetHashCode() => _hashCode;

        #endregion


        //------------------------------------------------------
        //
        //  Internal Static Methods
        //
        //------------------------------------------------------

        #region Internal Static Methods

        internal static float GetMass(ThingInfo info)
        {
            return info.DefInfo.Def.GetStatValueAbstract(StatDefOf.Mass, info.StuffDefInfo.Def);
        }

        internal static bool IsLoaded(ThingInfo info)
        {
            return info.Loaded;
        }

        internal static ThingInfo Resolve(ThingInfo info)
        {
            if (info.Loaded)
            {
                return info;
            }

            info.DefInfo.Resolve();

            if (!info.DefInfo.Loaded)
            {
                return info;
            }

            if (info.DefInfo.Def.MadeFromStuff)
            {
                info.StuffDefInfo.Resolve();

                if (!info.StuffDefInfo.Loaded)
                {
                    return info;
                }
            }

            if (!string.IsNullOrEmpty(info.StyleDefInfo.DefName))
            {
                info.StyleDefInfo.Resolve();

                if (!info.StyleDefInfo.Loaded)
                {
                    return info;
                }
            }

            if (info.AnyBladelinkTrait)
            {
                if (info.BladelinkTraits is null)
                {
                    return info;
                }

                for (int i = info.BladelinkTraits.Count - 1; i >= 0; i--)
                {
                    var trait = info.BladelinkTraits[i].Resolve();

                    if (!trait.Loaded)
                    {
                        return info;
                    }

                    info.BladelinkTraits[i] = trait;
                }
            }

            if (info.AnyUniqueTrait)
            {
                if (info.UniqueTraits is null)
                {
                    return info;
                }

                for (int i = info.UniqueTraits.Count - 1; i >= 0; i--)
                {
                    var trait = info.UniqueTraits[i].Resolve();

                    if (!trait.Loaded)
                    {
                        return info;
                    }

                    info.UniqueTraits[i] = trait;
                }
            }

            info.Loaded = true;
            return info;
        }

        #endregion


        private static DefInfo<WeaponTraitDef> AsDefInfo(WeaponTraitDef trait)
        {
            return new DefInfo<WeaponTraitDef>(trait);
        }


        private int _hashCode;
    }
}
