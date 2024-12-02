using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressDiffLog : IExposable
    {
        public List<ProgressDiffLedger> progressDiffLedgers = new List<ProgressDiffLedger>();

        public string progressClass = "";

        public ProgressDiffLog()
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();
            progressClass = "";
        }

        public string CurrClass
        {
            get { return progressClass; }
            set { progressClass = value; }
        }

        public ProgressDiffClassLedger TotalDiff()
        {
            ProgressDiffClassLedger totalLedger = new ProgressDiffClassLedger();

            // for each diff ledger
            foreach (var l in progressDiffLedgers)
            {
                // in each diff ledger, iterate through class diff ledgers
                foreach (var cl in l.classLedgers.Values)
                {
                    // and sum them up in the total Ledger
                    /*totalLedger.statOffsetsTotal.AddRange(cl.statOffsetsTotal);
                    totalLedger.statFactorsTotal.AddRange(cl.statFactorsTotal);
                    totalLedger.capModsTotal.AddRange(cl.capModsTotal);
                    totalLedger.hediffModsTotal.AddRange(cl.hediffModsTotal);
                    totalLedger.abilityTotal.AddRange(cl.abilityTotal);*/

                    totalLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(cl.statOffsetsTotal);
                    totalLedger.statFactorsTotal.DiffDictSum<StatDef, float>(cl.statFactorsTotal);
                    totalLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(cl.capModsTotal);
                    totalLedger.hediffModsTotal.DiffDictSum<HediffDef, float>(cl.hediffModsTotal);
                    totalLedger.abilityTotal.DiffDictSum<AbilityDef, int>(cl.abilityTotal);
                }
            }

            return totalLedger;
        }

        public void ApplyTotalDiff(ref ProgressTracker progressTracker)
        {
            ProgressDiffClassLedger totalDiff = TotalDiff();

            /*var statModCopy = new Dictionary<StatDef, float>();
            foreach (var statModPair in totalDiff.statOffsetsTotal)
            {
                // if the summed stat value = 0, don't bother making stat mod for it 
                if (statModPair.Value == 0) continue;
                statModCopy[statModPair.Key] = statModPair.Value;
            }
            progressTracker.statOffsetsTotal = statModCopy;

            statModCopy = new Dictionary<StatDef, float>();
            foreach (var statModPair in totalDiff.statOffsetsTotal)
            {
                // if the summed stat value = 0, don't bother making stat mod for it 
                if (statModPair.Value == 0) continue;
                statModCopy[statModPair.Key] = statModPair.Value + 1;
            }
            progressTracker.statFactorsTotal = statModCopy;

            var capModCopy = new Dictionary<PawnCapacityDef, float>();
            foreach (var capModPair in totalDiff.statOffsetsTotal)
            {
                // if the summed stat value = 0, don't bother making stat mod for it 
                if (capModPair.Value == 0) continue;
                statModCopy[capModPair.Key] = capModPair.Value;
            }
            progressTracker.capModsTotal = capModCopy;*/

            // see ProgressDiffLogUtility for extension methods
            // stat offset assignment
            progressTracker.statOffsetsTotal = totalDiff.statOffsetsTotal.DiffDictClean<StatDef, float>();

            // stat factor assignment
            progressTracker.statFactorsTotal = totalDiff.statFactorsTotal.DiffDictClean<StatDef, float>();

            // cap mod offset assignment
            progressTracker.capModsTotal = totalDiff.capModsTotal.DiffDictClean<PawnCapacityDef, float>();

            // stat factor here
            // cap offset/mod here

            // hediff portion
            foreach (var hediffPair in totalDiff.hediffModsTotal)
            {
                /*// this adjust severity based on output - but we need to get final value
                HealthUtility.AdjustSeverity(progressTracker.pawn, hediffPair.Key, hediffPair.Value);*/
                
                // properly applies hediffs in total diff
                ApplyHediffSeverity(progressTracker.pawn, hediffPair.Key, hediffPair.Value);
            }

            // ability portion
            foreach (var abilityPair in totalDiff.abilityTotal) ApplyAbilities(progressTracker.pawn, abilityPair.Key, abilityPair.Value);
        }

        public void ApplyHediffSeverity(Pawn pawn, HediffDef hdDef, float newSev)
        {
            // try to get relevant hediff
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef, false);

            // if the hediff's min severity >= newSev, do not apply it
            // in fact: remove it if it exists: hediff MUST reach minimum severity
            if (hdDef.minSeverity >= newSev) 
            {
                if (hediff != null) pawn.health.RemoveHediff(hediff);
                return;
            }

            // assuming it meets the min severity
            // pre-existing hediff? set severity to new sev
            if (hediff != null)
            {
                hediff.Severity = newSev;
                return;
            }

            // hediff doesn't exist? make a new one
            if (newSev > 0f)
            {
                hediff = HediffMaker.MakeHediff(hdDef, pawn, null);
                hediff.Severity = newSev;
                pawn.health.AddHediff(hediff, null, null, null);
            }
        }

        public void ApplyAbilities(Pawn pawn, AbilityDef abilityDef, float diffSum)
        {
            bool addCheck = diffSum >= 1 ? true : false;
            if (addCheck) pawn.abilities.GainAbility(abilityDef);
            else pawn.abilities.RemoveAbility(abilityDef);
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref progressDiffLedgers, "progressLedgers", LookMode.Deep);
        }

    }
}
