using ForeignerOfUniverse.Comps.AbilityEffects;
using ForeignerOfUniverse.Models;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;
using Verse.Sound;
using RimWorld.Planet;

namespace ForeignerOfUniverse.Utilities
{
    internal static class MatterManipulateUtility
    {
        public static bool AllowDisintegrate(this Thing thing)
        {
            return thing.def.category is ThingCategory.Item;
        }

        public static int CalculateMaterialCostByReplication(this Pawn pawn, Thing material, out float unitsPerMaterial)
        {
            unitsPerMaterial = 0f;
            return pawn.TryGetNanites(out var nanites) ? Mathf.Clamp(nanites.CalculateDismantleForMaxGrowth(material, out unitsPerMaterial), 1, material.stackCount) : 1;
        }

        public static float CalculateNanitePerMaterial(this Thing material)
        {
            return material.def.GetStatValueAbstract(StatDefOf.Mass, material.Stuff) * FOU.Settings.NanitesPerKilogram;
        }

        public static void Disintegrate(this Thing thing, int count)
        {
            if (thing.stackCount > count)
            {
                thing.stackCount -= count;
            }
            else
            {
                thing.Destroy();
            }
        }

        public static float ForceTakeDamage(this Thing target, DamageInfo info, Pawn instigator)
        {
            var result = FOUDefOf.FOU_Disintegration.Worker.Apply(info, target);

            if (target.SpawnedOrAnyParentSpawned)
            {
                target.MapHeld.damageWatcher.Notify_DamageTaken(target, result.totalDamageDealt);
            }

            if (FOUDefOf.FOU_Disintegration.ExternalViolenceFor(target))
            {
                GenLeaving.DropFilthDueToDamage(target, result.totalDamageDealt);

                instigator.records.AddTo(RecordDefOf.DamageDealt, result.totalDamageDealt);

                if (instigator.Faction == Faction.OfPlayer)
                {
                    QuestUtility.SendQuestTargetSignals(target.questTags, "TookDamageFromPlayer", target.Named("SUBJECT"), instigator.Named("INSTIGATOR"));
                }

                QuestUtility.SendQuestTargetSignals(target.questTags, "TookDamage", target.Named("SUBJECT"), instigator.Named("INSTIGATOR"), target.MapHeld.Named("MAP"));
            }

            target.PostApplyDamage(info, result.totalDamageDealt);
            return Mathf.Max(1f, result.totalDamageDealt);
        }

        public static void RecordWeavableThing(this Pawn pawn, Thing thing)
        {
            if (pawn.abilities is null)
            {
                return;
            }

            var ability = pawn.abilities.GetAbility(FOUDefOf.FOU_MatterWeave);

            if (ability is null)
            {
                return;
            }

            var comp = ability.CompOfType<MatterWeave>();

            if (comp is null)
            {
                return;
            }

            if (thing is Corpse)
            {
                Messages.Message("FOU.NaniteAbility.CannotAddWeavableThing".Translate(thing.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                    MessageTypeDefOf.NegativeEvent, historical: false);
                return;
            }

            ThingInfo info;

            if (thing is MinifiedThing minifiedThing)
            {
                var innerThing = minifiedThing.InnerThing;

                if (innerThing.def.minifiedDef != null)
                {
                    info = new ThingInfo(innerThing);
                }
                else
                {
                    Messages.Message("FOU.NaniteAbility.CannotAddWeavableThing".Translate(thing.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                        MessageTypeDefOf.NegativeEvent, historical: false);
                    return;
                }
            }
            else
            {
                info = new ThingInfo(thing);
            }

            if (comp.weavableThings.Add(info))
            {
                Messages.Message("FOU.NaniteAbility.WeavableThingAdded".Translate(info.LabelCap.Colorize(ColoredText.NameColor), FOUDefOf.FOU_MatterWeave.LabelCap.Colorize(ColoredText.NameColor)).Resolve(),
                    MessageTypeDefOf.PositiveEvent, historical: false);
            }
        }

        public static void WeaveThing(this Caravan caravan, ThingInfo thingInfo, int count)
        {
            var thing = new ThingWeaveInfo(thingInfo).Weave(count);
            caravan.AddPawnOrItem(thing, false);
        }

        public static void WeaveThing(this Map map, IntVec3 pos, ThingInfo thingInfo, int count)
        {
            var things = new ThingWeaveInfo(thingInfo).Weave(count);

            void OnPlaced(Thing thing, int stackCount)
            {
                map.effecterMaintainer.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(thing, map), thing.Position, 60);
                FOUDefOf.FOU_TransportExit.PlayOneShot(new TargetInfo(thing));
            }

            GenPlace.TryPlaceThing(things, pos, map, ThingPlaceMode.Near, OnPlaced);
        }

        public static void WeaveThing(this Pawn pawn, ThingInfo thingInfo, int count)
        {
            var weaveInfo = new ThingWeaveInfo(thingInfo);

            if (pawn.apparel is null)
            {
                pawn.inventory.innerContainer.TryAdd(weaveInfo.Weave(count));
                return;
            }

            if (weaveInfo.Def.IsApparel)
            {
                if (weaveInfo.Def.apparel.PawnCanWear(pawn) && ApparelUtility.HasPartsToWear(pawn, weaveInfo.Def))
                {
                    count--;

                    var body = pawn.RaceProps.body;
                    var wornApparel = pawn.apparel.WornApparel;

                    for (int i = wornApparel.Count - 1; i >= 0; i--)
                    {
                        var apparel = wornApparel[i];

                        if (!ApparelUtility.CanWearTogether(weaveInfo.Def, apparel.def, body))
                        {
                            pawn.apparel.TryMoveToInventory(apparel);
                        }
                    }

                    pawn.apparel.Wear((Apparel)weaveInfo.Weave(1));
                }
            }
            else if (weaveInfo.Def.IsWeapon)
            {
                if (!pawn.Downed && !pawn.WorkTagIsDisabled(WorkTags.Violent) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                {
                    count--;

                    if (weaveInfo.Def.equipmentType is EquipmentType.Primary && pawn.equipment.Primary != null)
                    {
                        pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.inventory.innerContainer);
                    }

                    pawn.equipment.AddEquipment((ThingWithComps)weaveInfo.Weave(1));
                }
            }

            if (count > 0)
            {
                pawn.inventory.innerContainer.TryAdd(weaveInfo.Weave(count));
            }
        }
    }
}
