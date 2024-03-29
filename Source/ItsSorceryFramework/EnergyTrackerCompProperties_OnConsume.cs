using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

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
