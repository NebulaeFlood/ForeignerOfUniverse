using Nebulae.RimWorld.UI.Automation.Attributes;
using Nebulae.RimWorld.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ForeignerOfUniverse.Models
{
    [LayoutModel("FOU.HediffInfo")]
    public struct HediffInfo : IEquatable<HediffInfo>, IExposable
    {
        [TextEntry(ReadOnly = true)]
        public string DefName;

        public HediffDef Def;

        [TextEntry(ReadOnly = true, WrapText = true)]
        public string Description;

        [BooleanEntry(ReadOnly = true)]
        public bool Loaded;


        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors

        private HediffInfo(string defName, HediffDef def)
        {
            DefName = defName;

            Def = def;

            Loaded = Def != null;

            Description = Loaded ? Def.description : string.Empty;

            _hashCode = DefName.GetHashCode();
        }

        internal HediffInfo(HediffDef def)
        {
            DefName = def.defName;

            Def = def;
            Description = def.Description;
            Loaded = true;

            _hashCode = DefName.GetHashCode();
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
            return obj is HediffInfo other
                && DefName.Equals(other.DefName);
        }

        public bool Equals(HediffInfo other)
        {
            return DefName.Equals(other.DefName);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref DefName, nameof(DefName), defaultValue: string.Empty);
            Scribe_Values.Look(ref _hashCode, "HashCode", defaultValue: -1);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return DefName;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Internal Static Methods
        //
        //------------------------------------------------------

        #region Internal Static Methods

        internal static HediffInfo From(string defName)
        {
            return new HediffInfo(defName, DefDatabase<HediffDef>.GetNamedSilentFail(defName));
        }

        internal static HediffDef GetDef(HediffInfo info)
        {
            return info.Def;
        }

        internal static bool IsLoaded(HediffInfo info)
        {
            return info.Loaded;
        }

        internal static HediffInfo Resolve(HediffInfo info)
        {
            info.Def = DefDatabase<HediffDef>.GetNamedSilentFail(info.DefName);
            info.Loaded = info.Def != null;
            info.Description = info.Loaded ? info.Def.Description : string.Empty;
            return info;
        }

        #endregion


        private int _hashCode;
    }
}
