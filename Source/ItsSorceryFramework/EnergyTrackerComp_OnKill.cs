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
    public class EnergyTrackerComp_OnKill : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_OnKill Props => (EnergyTrackerCompProperties_OnKill)props;

        public override void CompPostKill(DamageInfo? damageInfo) 
        {
            if (damageInfo is null || damageInfo.Value.Instigator is null || damageInfo.Value.Instigator as Pawn != parent.pawn) return;

            if (Props.damageDefs.NullOrEmpty() || Props.damageDefs.Contains(damageInfo.Value.Def))
            {
                StatDef refStatDef = Props.scalingStatDef is null ? StatDefOf_ItsSorcery.Scaling_ItsSorcery : Props.scalingStatDef;
                float energyMaxChange = parent.InvMult * Props.baseEnergy * parent.pawn.GetStatValue(refStatDef);
                parent.currentEnergy = Mathf.Clamp(parent.currentEnergy + energyMaxChange, parent.AbsMinEnergy, parent.AbsMaxEnergy);
                // in the future, add effect activation here.
            }
        }

        public override IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            yield break;
        }

        public override float CompDrawWorker(Rect rect) => 0f; // enables LearningTracker_Progress to draw information about EnergyTrackers
    }

}
