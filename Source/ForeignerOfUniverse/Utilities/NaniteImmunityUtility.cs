using ForeignerOfUniverse.Models;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ForeignerOfUniverse.Utilities
{
    internal static class NaniteImmunityUtility
    {
        //------------------------------------------------------
        //
        //  Public Static Fields
        //
        //------------------------------------------------------

        #region Public Static Fields

        public static readonly HashSet<HediffDef> AscensionImmunizedHediffDefs = new HashSet<HediffDef>();
        public static readonly HashSet<HediffDef> NooNetImmunizedHediffDefs = new HashSet<HediffDef>();
        public static readonly HashSet<HediffDef> PhoenixImmunizedHediffDefs = new HashSet<HediffDef>();

        public static readonly HashSet<HediffDef> DisallowedHediffDefs = new HashSet<HediffDef>
        {
            FOUDefOf.FOU_PsychicShild,
            FOUDefOf.FOU_InjuryInducedFinalNanostate,
            FOUDefOf.FOU_InjuryInducedNanostate,
            FOUDefOf.FOU_ExistenceAnchor,
            FOUDefOf.FOU_RecoveryProgram,
            FOUDefOf.FOU_Regenerating
        };

        #endregion


        //------------------------------------------------------
        //
        //  Internal Static Methods
        //
        //------------------------------------------------------

        #region Internal Static Methods

        internal static bool TryAdd(ProtocolType protocol, HediffInfo info)
        {
            if (DisallowedHediffDefs.Contains(info.Def))
            {
                Messages.Message("FOU.Messages.Settings.Protocols.Disallowed.Content".Translate(info.Def.LabelCap).Resolve(),
                    MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }

            var settings = FOU.Settings;

            switch (protocol)
            {
                case ProtocolType.Ascension:
                    if (!AscensionImmunizedHediffDefs.Add(info.Def))
                    {
                        Messages.Message("FOU.Messages.Settings.Protocols.AlreadyHaveDef.Content".Translate(info.Def.LabelCap.Colorize(ColoredText.NameColor), info.DefName, "FOU.Protocols.Ascension.Label".Translate().Colorize(ColoredText.NameColor)).Resolve(),
                            MessageTypeDefOf.RejectInput, historical: false);
                        return false;
                    }
                    settings.AscensionImmutableHediffs.Add(info);

                    Messages.Message("FOU.Messages.Settings.Protocols.AddSecceed.Content".Translate(info.Def.LabelCap.Colorize(ColoredText.NameColor), info.DefName, "FOU.Protocols.Ascension.Label".Translate().Colorize(ColoredText.NameColor)).Resolve(),
                        MessageTypeDefOf.NeutralEvent, historical: false);
                    break;
                case ProtocolType.NooNet:
                    if (!NooNetImmunizedHediffDefs.Add(info.Def))
                    {
                        Messages.Message("FOU.Messages.Settings.Protocols.AlreadyHaveDef.Content".Translate(info.Def.LabelCap.Colorize(ColoredText.NameColor), info.DefName, "FOU.Protocols.NooNet.Label".Translate().Colorize(ColoredText.NameColor)).Resolve(),
                            MessageTypeDefOf.RejectInput, historical: false);
                        return false;
                    }
                    settings.NooNetImmutableHediffs.Add(info);

                    Messages.Message("FOU.Messages.Settings.Protocols.AddSecceed.Content".Translate(info.Def.LabelCap.Colorize(ColoredText.NameColor), info.DefName, "FOU.Protocols.NooNet.Label".Translate().Colorize(ColoredText.NameColor)).Resolve(),
                        MessageTypeDefOf.NeutralEvent, historical: false);
                    break;
                case ProtocolType.Phoenix:
                    if (!PhoenixImmunizedHediffDefs.Add(info.Def))
                    {
                        Messages.Message("FOU.Messages.Settings.Protocols.AlreadyHaveDef.Content".Translate(info.Def.LabelCap.Colorize(ColoredText.NameColor), info.DefName, "FOU.Protocols.Phoenix.Label".Translate().Colorize(ColoredText.NameColor)).Resolve(),
                            MessageTypeDefOf.RejectInput, historical: false);
                        return false;
                    }
                    settings.PhoenixImmutableHediffs.Add(info);

                    Messages.Message("FOU.Messages.Settings.Protocols.AddSecceed.Content".Translate(info.Def.LabelCap.Colorize(ColoredText.NameColor), info.DefName, "FOU.Protocols.Phoenix.Label".Translate().Colorize(ColoredText.NameColor)).Resolve(),
                        MessageTypeDefOf.NeutralEvent, historical: false);
                    break;
                default:
                    Messages.Message("FOU.Messages.Settings.Protocols.UnknownProtocol.Content".Translate(protocol.ToString().Colorize(ColoredText.NameColor)).Resolve(),
                        MessageTypeDefOf.RejectInput, historical: false);
                    return false;
            }

            settings.Mod.WriteSettings();

            return true;
        }

        internal static void Initialize()
        {
            var settings = FOU.Settings;

            AscensionImmunizedHediffDefs
                .UnionWith(AscensionDefaultImmunizd.Concat(settings.AscensionImmutableHediffs)
                    .Where(HediffInfo.IsLoaded)
                    .Select(HediffInfo.GetDef));

            NooNetImmunizedHediffDefs
                .UnionWith(NooNetDefaultImmunizd.Concat(settings.NooNetImmutableHediffs)
                    .Where(HediffInfo.IsLoaded)
                    .Select(HediffInfo.GetDef));

            PhoenixImmunizedHediffDefs
                .UnionWith(PhoenixDefaultImmunizd.Concat(settings.PhoenixImmutableHediffs)
                    .Where(HediffInfo.IsLoaded)
                    .Select(HediffInfo.GetDef));
        }

        internal static void Remove(ProtocolType protocol, HediffInfo info)
        {
            var settings = FOU.Settings;

            switch (protocol)
            {
                case ProtocolType.Ascension:
                    settings.AscensionImmutableHediffs.Remove(info);

                    if (info.Loaded)
                    {
                        AscensionImmunizedHediffDefs.Remove(info.Def);
                    }
                    break;
                case ProtocolType.NooNet:
                    settings.NooNetImmutableHediffs.Remove(info);

                    if (info.Loaded)
                    {
                        NooNetImmunizedHediffDefs.Remove(info.Def);
                    }
                    break;
                case ProtocolType.Phoenix:
                    settings.PhoenixImmutableHediffs.Remove(info);

                    if (info.Loaded)
                    {
                        PhoenixImmunizedHediffDefs.Remove(info.Def);
                    }
                    break;
                default:
                    break;
            }

            settings.Mod.WriteSettings();
        }

        internal static IEnumerable<HediffInfo> SetDefault(ProtocolType protocol)
        {
            var settings = FOU.Settings;

            switch (protocol)
            {
                case ProtocolType.Ascension:
                    AscensionImmunizedHediffDefs.Clear();
                    AscensionImmunizedHediffDefs
                        .UnionWith(AscensionDefaultImmunizd
                            .Where(HediffInfo.IsLoaded)
                            .Select(HediffInfo.GetDef));
                    settings.AscensionImmutableHediffs.Clear();
                    settings.AscensionImmutableHediffs.UnionWith(AscensionDefaultImmunizd);
                    settings.Mod.WriteSettings();
                    return AscensionDefaultImmunizd;
                case ProtocolType.NooNet:
                    NooNetImmunizedHediffDefs.Clear();
                    NooNetImmunizedHediffDefs
                        .UnionWith(NooNetDefaultImmunizd
                            .Where(HediffInfo.IsLoaded)
                            .Select(HediffInfo.GetDef));
                    settings.NooNetImmutableHediffs.Clear();
                    settings.NooNetImmutableHediffs.UnionWith(NooNetDefaultImmunizd);
                    settings.Mod.WriteSettings();
                    return NooNetDefaultImmunizd;
                case ProtocolType.Phoenix:
                    PhoenixImmunizedHediffDefs.Clear();
                    PhoenixImmunizedHediffDefs
                        .UnionWith(PhoenixDefaultImmunizd
                            .Where(HediffInfo.IsLoaded)
                            .Select(HediffInfo.GetDef));
                    settings.PhoenixImmutableHediffs.Clear();
                    settings.PhoenixImmutableHediffs.UnionWith(PhoenixDefaultImmunizd);
                    settings.Mod.WriteSettings();
                    return PhoenixDefaultImmunizd;
                default:
                    return Enumerable.Empty<HediffInfo>();
            }
        }

        #endregion


        //------------------------------------------------------
        //
        //  Internal Static Properties
        //
        //------------------------------------------------------

        #region Internal Static Properties

        internal static IEnumerable<HediffInfo> AscensionDefaultImmunizd
        {
            get
            {
                yield return HediffInfo.From("AlcoholAddiction");
                yield return HediffInfo.From("Alzheimers");
                yield return HediffInfo.From("AmbrosiaAddiction");
                yield return HediffInfo.From("Asthma");
                yield return HediffInfo.From("BadBack");
                yield return HediffInfo.From("BrainShock");
                yield return HediffInfo.From("Cataract");
                yield return HediffInfo.From("Cirrhosis");
                yield return HediffInfo.From("DeathRefusalSickness");
                yield return HediffInfo.From("Dementia");
                yield return HediffInfo.From("DrugOverdose");
                yield return HediffInfo.From("FoodPoisoning");
                yield return HediffInfo.From("Frail");
                yield return HediffInfo.From("GoJuiceAddiction");
                yield return HediffInfo.From("GravNausea");
                yield return HediffInfo.From("HearingLoss");
                yield return HediffInfo.From("HeartArteryBlockage");
                yield return HediffInfo.From("LuciferiumHigh");
                yield return HediffInfo.From("MorningSickness");
                yield return HediffInfo.From("PostpartumExhaustion");
                yield return HediffInfo.From("PsychiteAddiction");
                yield return HediffInfo.From("ResurrectionPsychosis");
                yield return HediffInfo.From("ResurrectionSickness");
                yield return HediffInfo.From("SmokeleafAddiction");
                yield return HediffInfo.From("WakeUpAddiction");
            }
        }

        internal static IEnumerable<HediffInfo> NooNetDefaultImmunizd
        {
            get
            {
                yield return HediffInfo.From("AgonyPulse");
                yield return HediffInfo.From("BrainwipeComa");
                yield return HediffInfo.From("CubeComa");
                yield return HediffInfo.From("CubeInterest");
                yield return HediffInfo.From("CubeRage");
                yield return HediffInfo.From("CubeWithdrawal");
                yield return HediffInfo.From("DarkPsychicShock");
                yield return HediffInfo.From("DisruptorFlash");
                yield return HediffInfo.From("Inhumanized");
                yield return HediffInfo.From("PsychicAnesthesia");
                yield return HediffInfo.From("PsychicBlindness");
                yield return HediffInfo.From("PsychicBurden");
                yield return HediffInfo.From("PsychicShock");
                yield return HediffInfo.From("PsychicVertigo");
                yield return HediffInfo.From("ThrumboRoar");
                yield return HediffInfo.From("WarTrumpet");
            }
        }

        internal static IEnumerable<HediffInfo> PhoenixDefaultImmunizd
        {
            get
            {
                yield return HediffInfo.From("BiosculptingSickness");
                yield return HediffInfo.From("BlissLobotomy");
                yield return HediffInfo.From("BloodRot");
                yield return HediffInfo.From("Carcinoma");
                yield return HediffInfo.From("CryptosleepSickness");
                yield return HediffInfo.From("Decayed");
                yield return HediffInfo.From("DirtInEyes");
                yield return HediffInfo.From("FibrousMechanites");
                yield return HediffInfo.From("FleshmassLung");
                yield return HediffInfo.From("FleshmassStomach");
                yield return HediffInfo.From("FleshWhip");
                yield return HediffInfo.From("Flu");
                yield return HediffInfo.From("GravelInEyes");
                yield return HediffInfo.From("GutWorms");
                yield return HediffInfo.From("HeartAttack");
                yield return HediffInfo.From("LungRot");
                yield return HediffInfo.From("Malaria");
                yield return HediffInfo.From("MetalhorrorImplant");
                yield return HediffInfo.From("MudInEyes");
                yield return HediffInfo.From("MuscleParasites");
                yield return HediffInfo.From("OrganDecay");
                yield return HediffInfo.From("Plague");
                yield return HediffInfo.From("PorcupineQuill");
                yield return HediffInfo.From("SandInEyes");
                yield return HediffInfo.From("Scaria");
                yield return HediffInfo.From("ScariaInfection");
                yield return HediffInfo.From("SensoryMechanites");
                yield return HediffInfo.From("SleepingSickness");
                yield return HediffInfo.From("TraumaSavant");
                yield return HediffInfo.From("WoundInfection");
            }
        }

        #endregion
    }
}
