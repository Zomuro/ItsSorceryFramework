using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_PerTick : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_PerTick Props => (EnergyTrackerCompProperties_PerTick)props;

        public override void CompExposeData() { } // saving values to comp, if needed

        public float ToTick => 1.TicksToSeconds(); // set to conversion value from tick to second.

        public float InvFactor => parent.InvMult; // if inverse energy tracker, this is -1f; otherwise, it is 1f.

        public float RecoveryRate => parent.pawn.GetStatValue(Props.RecoveryRateStatDef);

        public override void CompPostTick() 
        {
            float energyChange = parent.InvMult * ToTick * RecoveryRate;

            if (parent.InDeficit)
            {
                if (!parent.def.inverse) parent.currentEnergy = Mathf.Max(parent.currentEnergy + energyChange * Props.deficitRecoveryFactor, parent.AbsMinEnergy);
                else parent.currentEnergy = Mathf.Min(parent.currentEnergy + energyChange * Props.deficitRecoveryFactor, parent.AbsMaxEnergy);
            }
            else if (parent.InOvercharge)
            {
                if (!parent.def.inverse) parent.currentEnergy = Mathf.Min(parent.currentEnergy + energyChange * Props.overchargeRecoveryFactor, parent.AbsMaxEnergy);
                else parent.currentEnergy = Mathf.Max(parent.currentEnergy + energyChange * Props.overchargeRecoveryFactor, parent.AbsMinEnergy);
            }
            else parent.currentEnergy = Mathf.Clamp(parent.currentEnergy + energyChange, parent.MinEnergy, parent.MaxEnergy);
        } 

        public override IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            StatDef statDef;
            StatRequest pawnReq = StatRequest.For(parent.pawn);
            StatCategoryDef finalCat = catDef ?? StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF;

            // shows a pawn's multiplier on relevant sorcery cost
            statDef = Props.RecoveryRateStatDef;
            yield return new StatDrawEntry(finalCat,
                    Props.RecoveryRateStatDef, RecoveryRate, pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            //over and under bar drain multiplier
            if (parent.HasDeficitZone)
            {
                yield return new StatDrawEntry(finalCat,
                    Props.deficitLabelKey.Translate(), Props.deficitRecoveryFactor.ToStringPercent(),
                    Props.deficitDescKey.Translate(parent.EnergyLabel), 40, null, null, false);
            }
            
            if (parent.HasOverchargeZone)
            {
                yield return new StatDrawEntry(finalCat,
                    Props.overchargeLabelKey.Translate(), Props.overchargeRecoveryFactor.ToStringPercent(),
                    Props.overchargeDescKey.Translate(parent.EnergyLabel),
                    30, null, null, false);
            }

            yield break;
        }
 
    }

}
