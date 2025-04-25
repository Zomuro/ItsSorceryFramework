using RimWorld;
using System.Collections.Generic;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_OnInteraction : EnergyTrackerCompProperties
    {
        public EnergyTrackerCompProperties_OnInteraction() => compClass = typeof(EnergyTrackerComp_OnConsume);

        public StatDef scalingStatDef;

        public float baseEnergy = 1f;

        public List<InteractionDef> interactionDefs;
    }

}
