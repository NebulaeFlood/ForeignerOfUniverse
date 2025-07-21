using ForeignerOfUniverse.Models;
using Nebulae.RimWorld.UI;
using Nebulae.RimWorld.UI.Automation;
using Nebulae.RimWorld.UI.Automation.Attributes;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ForeignerOfUniverse
{
    [LayoutModel("FOU.Settings")]
    public class FOUSettings : NebulaeModSettings<FOUSettings>
    {
        [NumberEntry(200f, 4000f, Prompted = false)]
        public float NanitesMaxStorage = 200f;
        [NumberEntry(0f, 4000f)]
        public float NanitesDailyReplication = 50f;

        [NumberEntry(10f, 2000f)]
        public float PhoenixRegenerationPerDay = 200f;

        [NumberEntry(10f, 2000f)]
        public float MatterRecoveryCost = 140f;
        [NumberEntry(0.1f, 100f, Decimals = 1)]
        public float NanitesPerKilogram = 2f;
        [NumberEntry(0.1f, 100f, Decimals = 1)]
        public float KilogramsPerNanite = 2f;

        [NumberEntry(10f, 2000f)]
        public float IndiscriminateDisintegrationCost = 200f;
        [NumberEntry(30f, 1000f)]
        public float IndiscriminateDisintegrationDamage = 50f;
        [NumberEntry(1f, 10f, Decimals = 1)]
        public float IndiscriminateDisintegrationRadius = 4f;

        [NumberEntry(300f, 60000f, SliderStep = 60f, Prompted = false)]
        public int RecoveryProgramCooldown = 6000;


        public HashSet<HediffInfo> AscensionImmutableHediffs;
        public HashSet<HediffInfo> NooNetImmutableHediffs;
        public HashSet<HediffInfo> PhoenixImmutableHediffs;


        public override void ExposeData()
        {
            Scribe_Values.Look(ref NanitesMaxStorage, nameof(NanitesMaxStorage), defaultValue: 200f);
            Scribe_Values.Look(ref NanitesDailyReplication, nameof(NanitesDailyReplication), defaultValue: 50f);
            Scribe_Values.Look(ref PhoenixRegenerationPerDay, nameof(PhoenixRegenerationPerDay), defaultValue: 200f);

            Scribe_Values.Look(ref MatterRecoveryCost, nameof(MatterRecoveryCost), defaultValue: 140f);
            Scribe_Values.Look(ref NanitesPerKilogram, nameof(NanitesPerKilogram), defaultValue: 2f);
            Scribe_Values.Look(ref KilogramsPerNanite, nameof(KilogramsPerNanite), defaultValue: 2f);

            Scribe_Values.Look(ref IndiscriminateDisintegrationCost, nameof(IndiscriminateDisintegrationCost), defaultValue: 200f);
            Scribe_Values.Look(ref IndiscriminateDisintegrationDamage, nameof(IndiscriminateDisintegrationDamage), defaultValue: 50f);
            Scribe_Values.Look(ref IndiscriminateDisintegrationRadius, nameof(IndiscriminateDisintegrationRadius), defaultValue: 4f);

            Scribe_Values.Look(ref RecoveryProgramCooldown, nameof(RecoveryProgramCooldown), defaultValue: 6000);

            Scribe_Collections.Look(ref AscensionImmutableHediffs, nameof(AscensionImmutableHediffs), LookMode.Deep);
            Scribe_Collections.Look(ref NooNetImmutableHediffs, nameof(NooNetImmutableHediffs), LookMode.Deep);
            Scribe_Collections.Look(ref PhoenixImmutableHediffs, nameof(PhoenixImmutableHediffs), LookMode.Deep);
        }

        public void Reset()
        {
            NanitesMaxStorage = 200f;
            NanitesDailyReplication = 50f;
            PhoenixRegenerationPerDay = 200f;

            MatterRecoveryCost = 140f;
            NanitesPerKilogram = 2f;
            KilogramsPerNanite = 2f;

            IndiscriminateDisintegrationCost = 200f;
            IndiscriminateDisintegrationDamage = 80f;
            IndiscriminateDisintegrationRadius = 4f;

            RecoveryProgramCooldown = 6000;
        }
    }
}
