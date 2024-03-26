using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_PerTick : EnergyTrackerComp
    {

        public EnergyTrackerCompProperties_PerTick Props => (EnergyTrackerCompProperties_PerTick)props;

        public override void CompExposeData() { } // saving values to comp, if needed

        public override void CompPostTick() 
        {
/*            float tempEnergy;
            if (parent.currentEnergy < 0)
            {
                tempEnergy = Math.Min(parent.currentEnergy + 1.TicksToSeconds() * EnergyRecoveryRate * UnderBarRecoveryFactor,
                    MaxEnergy);
            }
            else if (currentEnergy <= MaxEnergy) // when energy is under or equal the normal max
            {
                tempEnergy = Math.Min(currentEnergy + 1.TicksToSeconds() * EnergyRecoveryRate, MaxEnergy);
            }
            else // when energy is over the normal max
            {
                tempEnergy = Math.Min(currentEnergy - 1.TicksToSeconds() * EnergyRecoveryRate * OverBarRecoveryFactor,
                    OverMaxEnergy);
            }

            parent.currentEnergy = Math.Max(tempEnergy, parent.MinEnergy);*/



        } // for effects over time

        public override IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req) // provides special display stats, which show how energy gets recovered
        {
            yield break;
        }
 
    }

}
