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

        public string DefName;
        public ThingDef Def;

        public string MaterialDefName;
        public ThingDef MaterialDef;
        
        public string StyleDefName;
        public ThingStyleDef StyleDef;

        public bool Qualified;
        public QualityCategory Quality;

        public bool IsValid;

        #endregion


        internal ThingInfo(Thing thing)
        {
            DefName = thing.def.defName;
            Def = thing.def;

            _hashCode = DefName.GetHashCode();

            Qualified = thing.TryGetQuality(out Quality);

            if (Qualified)
            {
                _hashCode ^= Quality.GetHashCode();
            }

            if (thing.Stuff is null)
            {
                MaterialDef = null;
                MaterialDefName = string.Empty;
            }
            else
            {
                MaterialDef = thing.Stuff;
                MaterialDefName = MaterialDef.defName;

                _hashCode ^= MaterialDefName.GetHashCode();
            }

            if (thing.StyleDef is null)
            {
                StyleDef = null;
                StyleDefName = string.Empty;
            }
            else
            {
                StyleDef = thing.StyleDef;
                StyleDefName = StyleDef.defName;

                _hashCode ^= StyleDefName.GetHashCode();
            }

            IsValid = true;
        }


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public override bool Equals(object obj)
        {
            return obj is ThingInfo other
                && DefName.Equals(other.DefName)
                && MaterialDefName.Equals(other.MaterialDefName)
                && StyleDefName.Equals(other.StyleDefName);
        }

        public bool Equals(ThingInfo other)
        {
            return DefName.Equals(other.DefName)
                && MaterialDefName.Equals(other.MaterialDefName)
                && StyleDefName.Equals(other.StyleDefName);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref DefName, nameof(DefName), defaultValue: string.Empty);
            Scribe_Values.Look(ref MaterialDefName, nameof(MaterialDefName), defaultValue: string.Empty);
            Scribe_Values.Look(ref StyleDefName, nameof(StyleDefName), defaultValue: string.Empty);

            Scribe_Values.Look(ref Qualified, nameof(Qualified), defaultValue: false);
            Scribe_Values.Look(ref Quality, nameof(Quality), defaultValue: QualityCategory.Normal);

            Scribe_Values.Look(ref _hashCode, "HashCode", defaultValue: -1);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Internal Static Methods
        //
        //------------------------------------------------------

        #region Internal Static Methods

        internal static float GetMass(ThingInfo info)
        {
            return info.Def.GetStatValueAbstract(StatDefOf.Mass, info.MaterialDef);
        }

        internal static ThingInfo Resolve(ThingInfo info)
        {
            info.Def = DefDatabase<ThingDef>.GetNamedSilentFail(info.DefName);

            if (info.Def is null)
            {
                info.IsValid = false;
                return info;
            }

            if (string.IsNullOrEmpty(info.MaterialDefName))
            {
                info.IsValid = true;
            }
            else
            {
                info.MaterialDef = DefDatabase<ThingDef>.GetNamedSilentFail(info.MaterialDefName);
                info.IsValid = info.MaterialDef != null;
            }

            if (!string.IsNullOrEmpty(info.StyleDefName))
            {
                info.StyleDef = DefDatabase<ThingStyleDef>.GetNamedSilentFail(info.StyleDefName);
            }

            return info;
        }

        internal static bool Valid(ThingInfo info)
        {
            return info.IsValid;
        }

        #endregion


        internal string LabelCap
        {
            get
            {
                var label = Def.MadeFromStuff
                    ? "ThingMadeOfStuffLabel".Translate(MaterialDef.LabelAsStuff, Def.label)
                    : Def.LabelCap;

                if (!Qualified)
                {
                    return label;
                }

                return $"{label} ({Quality.GetLabel()})";
            }
        }


        private int _hashCode;
    }
}
