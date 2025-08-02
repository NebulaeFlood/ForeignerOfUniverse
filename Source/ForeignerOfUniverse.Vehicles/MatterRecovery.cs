using Vehicles;
using Verse;

namespace ForeignerOfUniverse.Vehicles.Comps.AbilityEffects
{
    public sealed class MatterRecovery : ForeignerOfUniverse.Comps.AbilityEffects.MatterRecovery
    {
        protected override bool IsPawnNeedRecover(Pawn pawn)
        {
            if (pawn is VehiclePawn vehicle)
            {
                return vehicle.CompFueledTravel.FuelPercent < 1f || vehicle.statHandler.NeedsRepairs;
            }

            return base.IsPawnNeedRecover(pawn);
        }

        protected override void Recover(LocalTargetInfo target)
        {
            if (target.Thing is VehiclePawn vehicle)
            {
                vehicle.statHandler.components.ForEach(component =>
                {
                    component.HealComponent(float.MaxValue);
                });
                vehicle.CompFueledTravel.Refuel(vehicle.CompFueledTravel.FuelCapacity);
            }
            else
            {
                base.Recover(target);
            }
        }
    }
}
