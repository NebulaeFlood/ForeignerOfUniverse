using System.Collections.Generic;
using Verse;

namespace ForeignerOfUniverse.Models
{
    internal sealed class ThingWeavePolicy : IExposable
    {
        public string Label;


        public int this[ThingInfo thing]
        {
            get => _things.TryGetValue(thing, out var item) ? item.Count : 0;
            set
            {
                thing.Count = value;

                _things.Remove(thing);
                _things.Add(thing);
            }
        }


        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors

        private ThingWeavePolicy() { }

        public ThingWeavePolicy(IEnumerable<ThingInfo> things, int policyCount)
        {
            Label = $"{"Untitled".Translate()} {policyCount}";

            _things = new HashSet<ThingInfo>(things);
        }

        #endregion


        public static string GetLabel(ThingWeavePolicy policy)
        {
            return policy.Label;
        }


        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        public bool Contains(ThingInfo thing) => _things.Contains(thing);

        public void ExposeData()
        {
            Scribe_Values.Look(ref Label, nameof(Label), defaultValue: null);
            Scribe_Collections.Look(ref _things, "Things", LookMode.Deep);
        }

        public void Remove(ThingInfo thing) => _things.Remove(thing);

        #endregion


        private HashSet<ThingInfo> _things;
    }
}
