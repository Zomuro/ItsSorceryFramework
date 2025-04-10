using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_JobBase : EnergyTrackerCompProperties
    {
        public EnergyTrackerCompProperties_JobBase() => compClass = typeof(EnergyTrackerComp_JobBase);

        public float baseEnergy = 0f;

        public StatDef scalingStatDef;

        public string jobSetLabel = "";

        public List<JobDef> jobDefs = new List<JobDef>();
    }
}
