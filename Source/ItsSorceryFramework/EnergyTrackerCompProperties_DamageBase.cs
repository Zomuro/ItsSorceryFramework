using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_DamageBase : EnergyTrackerCompProperties
    {
        public EnergyTrackerCompProperties_DamageBase() => compClass = typeof(EnergyTrackerComp_DamageBase);

        public StatDef scalingStatDef;

        public List<DamageDef> damageDefs = new List<DamageDef>();
    }
}
