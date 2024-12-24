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
            /*new ProgressDiffLedger(0, progressTracker.CurrLevel, progressTracker.currClass, 
                new Dictionary<string, ProgressDiffClassLedger>() {
                    { "", new ProgressDiffClassLedger("")}
                });*/
        }

        public ProgressDiffLog(string progressClass)
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();
            this.progressClass = progressClass;
        }

        public ProgressDiffLog(ProgressTracker progressTracker)
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();
            progressClass = progressTracker.currClass;
            progressDiffLedgers.Add(new ProgressDiffLedger(0, progressTracker.CurrLevel, "",
                new Dictionary<string, ProgressDiffClassLedger>() {
                    { "", new ProgressDiffClassLedger()}
                }));
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref progressDiffLedgers, "progressLedgers", LookMode.Deep);
        }

        public string CurrClass
        {
            get { return progressClass; }
            set { progressClass = value; }
        }

        public virtual void AddNewLedger(ProgressTracker progressTracker, Dictionary<StatDef, float> statOffsetsTotal, Dictionary<StatDef, float> statFactorsTotal,
            Dictionary<PawnCapacityDef, float> capModsTotal, Dictionary<HediffDef, float> hediffModsTotal, Dictionary<AbilityDef, int> abilityTotal)
        {
            ProgressDiffClassLedger newClassLedger = new ProgressDiffClassLedger(statOffsetsTotal, 
                statFactorsTotal, capModsTotal, hediffModsTotal, abilityTotal);

            ProgressDiffLedger newLedger = new ProgressDiffLedger(GetNextLedgerID, progressTracker.CurrLevel, progressTracker.currClass, 
                new Dictionary<string, ProgressDiffClassLedger>() {
                    { progressTracker.currClass, newClassLedger}});

            progressDiffLedgers.Add(newLedger);
        }

        public virtual ProgressDiffLedger PrepNewLedger(ProgressTracker progressTracker)
        {
            ProgressDiffLedger newLedger = new ProgressDiffLedger(GetNextLedgerID, progressTracker.CurrLevel, progressTracker.currClass,
                new Dictionary<string, ProgressDiffClassLedger>() {});

            return newLedger;
        }

        public virtual void AddLedger(ProgressDiffLedger progressDiffLedger) => progressDiffLedgers.Add(progressDiffLedger);

        public virtual ProgressDiffLedger GetLastLedgerOrDefault(ProgressTracker progressTracker)
        {
            ProgressDiffLedger lastLedger = progressDiffLedgers.LastOrDefault();
            if (lastLedger is null)
            {
                return new ProgressDiffLedger(0, progressTracker.CurrLevel, progressTracker.currClass, new Dictionary<string, ProgressDiffClassLedger>() {
                    { "", new ProgressDiffClassLedger()}});
            }
            return lastLedger;
        }

        public virtual int GetNextLedgerID => progressDiffLedgers.Count();

        // Logs modifiers from the ProgressLevelModifier
        public virtual void LogModifiers(ProgressLevelModifier modulo, ref ProgressDiffClassLedger classLedger)
        {
            //ProgressDiffClassLedger classLedger = new ProgressDiffClassLedger();
            classLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(modulo.statOffsets));
            classLedger.statFactorsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(modulo.statFactorOffsets));
            classLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(ProgressDiffLogUtility.ListToDiffDict(modulo.capMods));

            /*classLedger.statOffsetsTotal = ProgressDiffLogUtility.ListToDiffDict(modulo.statOffsets);
            classLedger.statFactorsTotal = ProgressDiffLogUtility.ListToDiffDict(modulo.statFactorOffsets);
            classLedger.capModsTotal = ProgressDiffLogUtility.ListToDiffDict(modulo.capMods);*/
        }

        // Logs modifiers from the ProgressLevelOption
        public virtual void LogModifiers(ProgressLevelOption option, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(option.statOffsets));
            classLedger.statFactorsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(option.statFactorOffsets));
            classLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(ProgressDiffLogUtility.ListToDiffDict(option.capMods));

            //ProgressDiffClassLedger classLedger = new ProgressDiffClassLedger();
            /*classLedger.statOffsetsTotal = ProgressDiffLogUtility.ListToDiffDict(option.statOffsets);
            classLedger.statFactorsTotal = ProgressDiffLogUtility.ListToDiffDict(option.statFactorOffsets);
            classLedger.capModsTotal = ProgressDiffLogUtility.ListToDiffDict(option.capMods);*/
        }

        public virtual void LogModifiers(ref ProgressDiffClassLedger classLedger, List<StatModifier> offsets = null, List<StatModifier> factorOffsets = null,
            List<PawnCapacityModifier> capMods = null)
        {
            classLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(offsets));
            classLedger.statFactorsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(factorOffsets));
            classLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(ProgressDiffLogUtility.ListToDiffDict(capMods));

            /*classLedger.statOffsetsTotal = ProgressDiffLogUtility.ListToDiffDict(offsets);
            classLedger.statFactorsTotal = ProgressDiffLogUtility.ListToDiffDict(factorOffsets);
            classLedger.capModsTotal = ProgressDiffLogUtility.ListToDiffDict(capMods);*/
        }

        public virtual void LogHediffs(ProgressLevelModifier modifier, ref ProgressDiffClassLedger classLedger)
        {
            // integrated into the progress tracker itself
        }


        public virtual void LogAbilities(ProgressLevelModifier modifier, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.abilityTotal.DiffDictSum<AbilityDef, int>(ProgressDiffLogUtility.ListToDiffDict(modifier.abilityGain, modifier.abilityRemove));
            //classLedger.abilityTotal = ProgressDiffLogUtility.ListToDiffDict(modifier.abilityGain, modifier.abilityRemove);
        }

        public virtual void LogAbilities(ProgressLevelOption option, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.abilityTotal.DiffDictSum<AbilityDef, int>(ProgressDiffLogUtility.ListToDiffDict(option.abilityGain, option.abilityRemove));
            //classLedger.abilityTotal = ProgressDiffLogUtility.ListToDiffDict(option.abilityGain, option.abilityRemove);
        }

        public virtual void LogAbilities(LearningTreeNodeDef node, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.abilityTotal.DiffDictSum<AbilityDef, int>(ProgressDiffLogUtility.ListToDiffDict(node.abilityGain, node.abilityRemove));
            //classLedger.abilityTotal = ProgressDiffLogUtility.ListToDiffDict(option.abilityGain, option.abilityRemove);
        }

        public ProgressDiffClassLedger TotalDiff(string progressClass = "")
        {
            ProgressDiffClassLedger totalLedger = new ProgressDiffClassLedger();

            // for each diff ledger
            foreach (var l in progressDiffLedgers)
            {
                // if the progressledger somehow has no assiociated class ledger, ignore it in the calc
                if (l.classLedgers.NullOrEmpty()) continue;

                // in the case we specify the generic case, we want everything
                if (progressClass == "")
                {
                    // for each diff ledger, add class diff ledgers to the summing dicts
                    foreach (var cl in l.classLedgers.Values)
                    {
                        totalLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(cl.statOffsetsTotal);
                        totalLedger.statFactorsTotal.DiffDictSum<StatDef, float>(cl.statFactorsTotal);
                        totalLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(cl.capModsTotal);
                        totalLedger.hediffModsTotal.DiffDictSum<HediffDef, float>(cl.hediffModsTotal);
                        totalLedger.abilityTotal.DiffDictSum<AbilityDef, int>(cl.abilityTotal);
                    }
                    continue;
                }

                // otherwise, only get diffs for a specific class - the general/base class ("") is not included
                if (l.classLedgers.ContainsKey(progressClass))
                {
                    totalLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(l.classLedgers[progressClass].statOffsetsTotal);
                    totalLedger.statFactorsTotal.DiffDictSum<StatDef, float>(l.classLedgers[progressClass].statFactorsTotal);
                    totalLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(l.classLedgers[progressClass].capModsTotal);
                    totalLedger.hediffModsTotal.DiffDictSum<HediffDef, float>(l.classLedgers[progressClass].hediffModsTotal);
                    totalLedger.abilityTotal.DiffDictSum<AbilityDef, int>(l.classLedgers[progressClass].abilityTotal);
                    continue;
                }
            }

            return totalLedger;
        }

        public void ApplyTotalDiff(ref ProgressTracker progressTracker)
        {
            // get the sum of all entries in the diff log
            ProgressDiffClassLedger totalDiff = TotalDiff();

            // see ProgressDiffLogUtility for extension methods
            // stat offset assignment
            progressTracker.statOffsetsTotal = totalDiff.statOffsetsTotal.DiffDictClean<StatDef, float>();

            // stat factor assignment
            progressTracker.statFactorsTotal = totalDiff.statFactorsTotal.DiffDictClean<StatDef, float>();

            // cap mod offset assignment
            progressTracker.capModsTotal = totalDiff.capModsTotal.DiffDictClean<PawnCapacityDef, float>();

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



    }
}
