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
    public class EnergyTrackerComp_OnDamage : EnergyTrackerComp_DamageBase
    {
        //public EnergyTrackerCompProperties_OnDamage Props => (EnergyTrackerCompProperties_OnDamage)props;

        public override void CompExposeData() { } // saving values to comp, if needed

        //public float InvFactor => parent.InvMult; // if inverse energy tracker, this is -1f; otherwise, it is 1f.

        public override void CompPostDamageDealt(DamageInfo damageInfo) 
        {
            if (Props.damageDefs.NullOrEmpty() || Props.damageDefs.Contains(damageInfo.Def))
            {
                StatDef refStatDef = Props.scalingStatDef is null ? StatDefOf_ItsSorcery.Scaling_ItsSorcery: Props.scalingStatDef;
                float energyMaxChange = parent.InvMult * damageInfo.Amount * parent.pawn.GetStatValue(refStatDef);
                parent.currentEnergy = Mathf.Clamp(parent.currentEnergy + energyMaxChange, parent.AbsMinEnergy, parent.AbsMaxEnergy);
                // in the future, add effect activation here.
            }
        }

        public override IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            //parent.pawn.jobs.curJob.id

            yield break;
        }
 
    }

}
