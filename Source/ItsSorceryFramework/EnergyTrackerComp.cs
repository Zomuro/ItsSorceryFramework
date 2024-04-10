using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp
    {
        public EnergyTracker parent;

        public EnergyTrackerCompProperties props;

        public virtual void CompExposeData() { } // saving values to comp, if needed

        public virtual void CompPostTick() { } // for effects over time

        public virtual IEnumerable<FloatMenuOption> CompPostConsume(Vector3 vec3) { yield break; } // for effects when using a "consumption" item

        public virtual void CompPostAdd() { } // retain for future use

        public virtual void CompPostRemove() { } // retain for future use

        public virtual void CompPostDamageDealt(DamageInfo damageInfo) { } // for effects activated when dealing damage

        public virtual void CompPostDamageRecieved(DamageInfo damageInfo) { } // for effects activated when recieving damage

        public virtual void CompPostKill(DamageInfo? damageInfo) { } // for effects activated when killing a pawn

        public virtual IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            yield break;
        }

        public virtual float CompDrawGUI(Rect rect) => 0f; // enables LearningTracker_Progress to draw information about EnergyTrackers

    }

}
