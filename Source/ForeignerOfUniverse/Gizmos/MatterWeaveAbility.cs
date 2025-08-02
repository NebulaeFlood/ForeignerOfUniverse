using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ForeignerOfUniverse.Gizmos
{
    internal sealed class MatterWeaveAbility : Command_Ability
    {
        public MatterWeaveAbility(Ability ability, Pawn pawn) : base(ability, pawn) { }

        public override void ProcessInput(Event ev)
        {
            var caravan = Pawn.GetCaravan();

            if (caravan != null && Pawn.MapHeld is null)
            {
                CurActivateSound?.PlayOneShotOnCamera();
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                ability.QueueCastingJob(caravan);
            }
            else
            {
                base.ProcessInput(ev);
            }
        }
    }
}
