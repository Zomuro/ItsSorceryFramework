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
    public class EnergyTrackerComp_DamageBase : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_DamageBase Props => (EnergyTrackerCompProperties_DamageBase)props;

        public StatDef ScalingStatDef => Props.scalingStatDef is null ? StatDefOf_ItsSorcery.Scaling_ItsSorcery : Props.scalingStatDef;

        public IEnumerable<String> DamageDefsLabels(IEnumerable<Def> defs)
        {
            foreach (var def in defs) yield return def.label;
            yield break;
        }

        public override void CompExposeData() { } // saving values to comp, if needed

        //public float InvFactor => parent.InvMult; // if inverse energy tracker, this is -1f; otherwise, it is 1f.

        public override void CompPostDamageDealt(DamageInfo damageInfo) { } // for effects activated when dealing damage

        public override IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            yield break;
        }

        public override float CompDrawGUI(Rect rect) => 0f; // enables LearningTracker_Progress to draw information about EnergyTrackers
    }

}
