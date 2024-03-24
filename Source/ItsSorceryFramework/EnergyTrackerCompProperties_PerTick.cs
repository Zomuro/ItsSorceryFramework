using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_PerTick : EnergyTrackerCompProperties
    {
        public EnergyTrackerCompProperties_PerTick() => compClass = typeof(EnergyTrackerComp_PerTick);

        public float chargeRatePerSecond = 0f;

        public bool continuous = true;
    }
}
