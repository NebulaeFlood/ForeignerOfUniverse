using ForeignerOfUniverse.Genes;
using ForeignerOfUniverse.Hediffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ForeignerOfUniverse.Models
{
    internal enum ProtocolType : byte
    {
        Ascension,
        NooNet,
        Phoenix
    }


    internal sealed class NaniteProtocol : IExposable
    {
        //------------------------------------------------------
        //
        //  Public Fields
        //
        //------------------------------------------------------

        #region Public Fields

        public bool CheckUp;

        public bool Ascension;
        public bool NooNet;
        public bool Weave;
        public bool Contravention;
        public bool Phoenix;

        public Hediff Anchor;

        public bool PsychicShildOpen;
        public Hediff PsychicShild;

        public bool ContraventionBarrierOpen;

        public Hediff RecoveryProgram;

        #endregion


        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors

        internal NaniteProtocol() { }

        internal NaniteProtocol(Pawn pawn)
        {
            Anchor = null;

            PsychicShildOpen = false;
            PsychicShild = null;

            RecoveryProgram = null;

            var genes = pawn.genes.GenesListForReading;
            int count = genes.Count;

            bool physicalAscensionExist = false;
            bool nanoSynapseExist = false;
            bool matterManipulationExist = false;

            for (int i = 0; i < count; i++)
            {
                var gene = genes[i];

                if (!physicalAscensionExist && gene.def == FOUDefOf.FOU_PhysicalAscension)
                {
                    physicalAscensionExist = true;
                }
                else if (!nanoSynapseExist && gene.def == FOUDefOf.FOU_NanoSynapse)
                {
                    nanoSynapseExist = true;
                }
                else if (!matterManipulationExist && gene.def == FOUDefOf.FOU_MatterManipulation)
                {
                    matterManipulationExist = true;
                }
                else if (physicalAscensionExist && nanoSynapseExist && matterManipulationExist)
                {
                    break;
                }
            }

            Ascension = physicalAscensionExist;
            NooNet = nanoSynapseExist;
            Weave = matterManipulationExist;
            Contravention = nanoSynapseExist && matterManipulationExist;
            Phoenix = physicalAscensionExist && matterManipulationExist;

            CheckUp = physicalAscensionExist || nanoSynapseExist || matterManipulationExist;
        }

        internal NaniteProtocol(NaniteProtocol protocol, Pawn pawn) : this(pawn)
        {
            Anchor = protocol.Anchor;

            if (protocol.NooNet)
            {
                if (NooNet)
                {
                    PsychicShildOpen = protocol.PsychicShildOpen;
                    PsychicShild = protocol.PsychicShild;
                }
                else
                {
                    if (protocol.PsychicShildOpen && PsychicShild != null)
                    {
                        pawn.health.RemoveHediff(PsychicShild);
                    }
                }
            }

            if (protocol.Contravention && Contravention)
            {
                ContraventionBarrierOpen = protocol.ContraventionBarrierOpen;
            }

            if (Phoenix)
            {
                RecoveryProgram = protocol.RecoveryProgram;
            }
        }

        #endregion


        public void ExposeData()
        {
            Scribe_Values.Look(ref CheckUp, nameof(CheckUp));

            Scribe_Values.Look(ref Ascension, nameof(Ascension));
            Scribe_Values.Look(ref NooNet, nameof(NooNet));
            Scribe_Values.Look(ref Weave, nameof(Weave));
            Scribe_Values.Look(ref Contravention, nameof(Contravention));
            Scribe_Values.Look(ref Phoenix, nameof(Phoenix));

            Scribe_Values.Look(ref PsychicShildOpen, nameof(PsychicShildOpen));
            Scribe_Values.Look(ref ContraventionBarrierOpen, nameof(ContraventionBarrierOpen));

            Scribe_References.Look(ref Anchor, nameof(Anchor));
            Scribe_References.Look(ref PsychicShild, nameof(PsychicShild));
            Scribe_References.Look(ref RecoveryProgram, nameof(RecoveryProgram));
        }
    }
}
