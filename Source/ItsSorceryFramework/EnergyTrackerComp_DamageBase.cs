using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_DamageBase : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_DamageBase Props => (EnergyTrackerCompProperties_DamageBase)props;

        public StatDef ScalingStatDef => Props.scalingStatDef is null ? StatDefOf_ItsSorcery.ISF_ScalingStat : Props.scalingStatDef;

        public virtual float ScalingStatValue => PawnCacheUtility.GetStatCacheVal(parent.pawn, ScalingStatDef);

        public IEnumerable<String> DamageDefsLabels(IEnumerable<Def> defs)
        {
            foreach (var def in defs) yield return def.label;
            yield break;
        }

        public override void CompExposeData() { } // saving values to comp, if needed

        public override void CompPostDamageDealt(DamageInfo damageInfo) { } // for effects activated when dealing damage

        public override IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            yield break;
        }

        public override float CompDrawGUI(Rect rect) => 0f; // enables LearningTracker_Progress to draw information about EnergyTrackers
    }

}
