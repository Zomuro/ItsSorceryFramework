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
            Initialize();
        }

        public ProgressTracker(Pawn pawn, SorcerySchemaDef def)
        {
            this.pawn = pawn;
            this.def = def.progressTrackerDef;
            this.sorcerySchemaDef = def;
            Initialize();
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref sorcerySchemaDef, "sorcerySchemaDef");
            Scribe_Deep.Look(ref hediff, "hediff");
            Scribe_Values.Look(ref points, "points", 0);
            Scribe_Collections.Look(ref statOffsetsTotal, "statOffsetsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref statFactorsTotal, "statFactorsTotal", LookMode.Def, LookMode.Value);
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
                    notifyLevelUp(hediff.Severity);
                }
                else
                {
                    HealthUtility.AdjustSeverity(pawn, def.progressHediff, exp / currentLevelEXPReq);
                    exp = 0;
                }
            }

            if(Mathf.Floor(hediff.Severity) > Mathf.Floor(orgSev))
            {
                notifyTotalLevelUp(orgSev);
            }
        }

        public virtual void notifyLevelUp(float sev)
        {
            int sevInt = (int)sev;
            bool check = false;
            foreach(ProgressLevelModulo modulo in def.levelModulo.OrderByDescending(x => x.levelFactor))
            {
                if(sevInt % modulo.levelFactor == 0)
                {
                    adjustTotalStatMods(statOffsetsTotal, modulo.statOffsets);
                    adjustTotalStatMods(statFactorsTotal, modulo.statFactors);

                    points += modulo.pointGain;
                    check = true;
                    break;
                }
            }
            if(!check) points += 1;

            HediffStage newStage = new HediffStage();
            newStage.minSeverity = sevInt;
            newStage.statOffsets = createStatModifiers(statOffsetsTotal).ToList();
            newStage.statFactors = createStatModifiers(statFactorsTotal).ToList();
            hediff.def.stages.Add(newStage);
        }

        public void adjustTotalStatMods(Dictionary<StatDef, float> stats, List<StatModifier> statMods)
        {
            foreach(StatModifier statMod in statMods)
            {
                if (stats.Keys.Contains(statMod.stat))
                {
                    stats[statMod.stat] += statMod.value;
                    continue;
                }

                stats[statMod.stat] = statMod.value;
            }
        }

        public IEnumerable<StatModifier> createStatModifiers(Dictionary<StatDef, float> stats)
        {
            StatModifier mod = new StatModifier();
            foreach (var pair in stats)
            {
                mod.stat = pair.Key; mod.value = pair.Value;
                yield return mod;
            }

            yield break;
        }

        public virtual void notifyTotalLevelUp(float sev)
        {
            Find.LetterStack.ReceiveLetter("Level up: "+ pawn.Name,
                "This pawn has leveled up.", LetterDefOf.NeutralEvent, null);
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

        // public List<StatModifier> statOffsetsTotal1;

        public Dictionary<StatDef, float> statOffsetsTotal = new Dictionary<StatDef, float>();

        //public List<StatModifier> statFactorsTotal1;

        public Dictionary<StatDef, float> statFactorsTotal = new Dictionary<StatDef, float>();

        public int points = 0;

        


    }
}
