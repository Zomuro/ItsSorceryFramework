using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class ProgressTracker
    {
        // initalizer- created via activator via SorcerySchema
        public ProgressTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public ProgressTracker(Pawn pawn, ProgressTrackerDef def)
        {
            this.pawn = pawn;
            this.def = def;
            this.sorcerySchemaDef = null;
        }

        public ProgressTracker(Pawn pawn, SorcerySchemaDef def)
        {
            this.pawn = pawn;
            this.def = def.progressTrackerDef;
            this.sorcerySchemaDef = def;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref sorcerySchemaDef, "sorcerySchemaDef");
            Scribe_Deep.Look(ref hediff, "hediff");
        }

        public virtual void Initialize()
        {
            if(pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff) == null)
                HealthUtility.AdjustSeverity(pawn, def.progressHediff, 1f);
            hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff);
        }

        public virtual void addHediffEXP(float exp)
        {
            float orgSev = hediff.Severity;
            float sevAdjust;

            while(exp > 0)
            {
                if(exp > currentLevelEXPReq - sevToCurrProgress(hediff.Severity))
                {
                    sevAdjust = currentLevelEXPReq - sevToCurrProgress(hediff.Severity);
                    exp -= sevAdjust;
                    HealthUtility.AdjustSeverity(pawn, def.progressHediff, sevAdjust / currentLevelEXPReq);
                    notifyLevelUp();
                }
                else
                {
                    HealthUtility.AdjustSeverity(pawn, def.progressHediff, exp / currentLevelEXPReq);
                    exp = 0;
                }
            }

            if(Mathf.Floor(hediff.Severity) > Mathf.Floor(orgSev))
            {
                notifyTotalLevelUp();
            }
        }

        public virtual void notifyLevelUp()
        {
            
        }

        public virtual void notifyTotalLevelUp()
        {
            
        }

        public float sevToCurrProgress(float currSev)
        {
            return (currSev - Mathf.Floor(currSev)) * currentLevelEXPReq;
        }

        public virtual float currentLevelEXPReq
        {
            get
            {
                return def.baseEXP * Mathf.Pow(def.scaling, Mathf.Floor(hediff.Severity) - 1f);
            }
        }

        public Pawn pawn;

        public ProgressTrackerDef def;

        public SorcerySchemaDef sorcerySchemaDef;

        public Hediff hediff;

        public List<StatModifier> statOffsetsTotal;

        public List<StatModifier> statFactorsTotal;
    }
}
