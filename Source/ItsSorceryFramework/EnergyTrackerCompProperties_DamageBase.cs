using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_DamageBase : EnergyTrackerCompProperties
    {
        public EnergyTrackerCompProperties_DamageBase() => compClass = typeof(EnergyTrackerComp_DamageBase);

        //public float energyFactor = 1f;

        public StatDef scalingStatDef;

        public List<DamageDef> damageDefs = new List<DamageDef>();
    }

    /*public class EnergyDamageRecord // retain for opportunity to do flat values?
    {
        public DamageDef damageDef;

        public float flatEnergy = 0f;
    }*/
}
