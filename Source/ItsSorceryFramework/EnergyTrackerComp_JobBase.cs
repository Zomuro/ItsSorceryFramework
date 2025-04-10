using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_JobBase : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_JobBase Props => (EnergyTrackerCompProperties_JobBase)props;

        public StatDef ScalingStatDef => Props.scalingStatDef ?? StatDefOf_ItsSorcery.ISF_ScalingStat;

        public override void CompExposeData() { } // saving values to comp, if needed

        public override void CompPostDamageDealt(DamageInfo damageInfo) { } // for effects activated when dealing damage

        public override IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            yield break;
        }

        public override float CompDrawGUI(Rect rect) => 0f; // enables LearningTracker_Progress to draw information about EnergyTrackers
    }

}
