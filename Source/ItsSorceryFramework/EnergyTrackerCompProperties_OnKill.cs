using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_OnKill : EnergyTrackerCompProperties_DamageBase
    {
        public EnergyTrackerCompProperties_OnKill() => compClass = typeof(EnergyTrackerComp_OnKill);

        public float baseEnergy = 0f;

        // public StatDef scalingStatDef; // present in parent class

        // public List<DamageDef> damageDefs = new List<DamageDef>(); // present in parent class
    }
}
