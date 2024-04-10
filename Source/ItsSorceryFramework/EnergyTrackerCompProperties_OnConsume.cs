using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_OnConsume : EnergyTrackerCompProperties
    {
        public EnergyTrackerCompProperties_OnConsume() => compClass = typeof(EnergyTrackerComp_OnConsume);

        public List<EnergyConsumable> consumables;
    }

    public class EnergyConsumable
    {
        public ThingDef thingDef;

        public float energy = 1f;
    }
}
