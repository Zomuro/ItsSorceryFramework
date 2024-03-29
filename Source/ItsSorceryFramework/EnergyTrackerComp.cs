using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp
    {
        public EnergyTracker parent;

        public EnergyTrackerCompProperties props;

        public virtual void CompExposeData() { } // saving values to comp, if needed

        public virtual void CompPostTick() { } // for effects over time

        public virtual IEnumerable<FloatMenuOption> CompPostConsume(Vector3 vec3) { yield break; } // for effects when using a "consumption" item

        public virtual void CompPostAdd() { }

        public virtual void CompPostRemove() { }

        public virtual void CompPostDamageDealt() { } // for effects activated when dealing damage

        public virtual void CompPostDamageRecieved() { } // for effects activated when recieving damage

        public virtual void CompPostKill() { } // for effects activated when killing a pawn

        public virtual IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            yield break;
        }
 
    }

}
