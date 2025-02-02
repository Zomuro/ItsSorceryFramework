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

        //public string progressClass = "";

       // public ProgressTrackerClassDef currClassDef;

        public ProgressDiffLog()
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();
            //progressClass = "";
        }

        public ProgressDiffLog(ProgressTrackerClassDef initClassDef)
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();
            //currClassDef = initClassDef;
            //progressClass = "";
        }

        public ProgressDiffLog(string progressClass)
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();
            //this.progressClass = progressClass;
        }

        public ProgressDiffLog(ProgressTracker progressTracker)
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();
            //progressClass = progressTracker.currClass;
            /*progressDiffLedgers.Add(new ProgressDiffLedger(0, progressTracker.CurrLevel, "",
                new Dictionary<string, ProgressDiffClassLedger>() {
                    { "", new ProgressDiffClassLedger()}
                }));*/

            progressDiffLedgers.Add(new ProgressDiffLedger(0, progressTracker.CurrLevel, ISF_DefOf.ISF_Generic_Class,
                new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>() {
                    { ISF_DefOf.ISF_Generic_Class, new ProgressDiffClassLedger()}
                }));
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref progressDiffLedgers, "progressLedgers", LookMode.Deep);
        }

        /*public virtual bool ValidateClassChange(ProgressTracker progressTracker, ProgressTrackerClassDef targetClassDef)
        {
            // get relevant alternative and prerequisite classes
            List<ProgressTrackerClassDef> altClasses = targetClassDef.AltClassDefs;
            if (targetClassDef.prereqClasses.Contains(progressTracker.currClassDef) ||
                progressTracker.currClassDef.AltClassDefs.Contains(targetClassDef)) return false;

            return false;
        }
        
        public virtual void AdjustClass(ProgressTracker progressTracker, ProgressTrackerClassDef targetClassDef)
        {
            // handle current class diff info
            ProgressDiffClassLedger currClassDiff = TotalDiff(progressTracker.currClassDef); // get current class diff

            // in the case that class is exclusive, invert diff to counter class effects and add new ledger
            ProgressDiffClassLedger negateDiff = InvertDiff(currClassDiff); 
            AddNewLedger(progressTracker, negateDiff.statOffsetsTotal, negateDiff.statFactorsTotal, negateDiff.capModsTotal, negateDiff.hediffModsTotal, negateDiff.abilityTotal);

            // change class
            progressTracker.currClassDef = targetClassDef;

            // if class is exclusive, reset level to starting level of target class 
            progressTracker.level = targetClassDef.levelRange.TrueMin;
        }*/

        public virtual void AddNewLedger(ProgressTracker progressTracker, Dictionary<StatDef, float> statOffsetsTotal, Dictionary<StatDef, float> statFactorsTotal,
            Dictionary<PawnCapacityDef, float> capModsTotal, Dictionary<HediffDef, float> hediffModsTotal, Dictionary<AbilityDef, int> abilityTotal)
        {
            ProgressDiffClassLedger newClassLedger = new ProgressDiffClassLedger(statOffsetsTotal, 
                statFactorsTotal, capModsTotal, hediffModsTotal, abilityTotal);

            ProgressDiffLedger newLedger = new ProgressDiffLedger(GetNextLedgerID, progressTracker.CurrLevel, progressTracker.currClassDef,
                new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>() {
                    { progressTracker.currClassDef, newClassLedger}
                });

            progressDiffLedgers.Add(newLedger);
        }

        public virtual ProgressDiffLedger PrepNewLedger(ProgressTracker progressTracker)
        {
            ProgressDiffLedger newLedger = new ProgressDiffLedger(GetNextLedgerID, progressTracker.CurrLevel, progressTracker.currClassDef,
                new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>() { });

            return newLedger;
        }

        public virtual void AddLedger(ProgressDiffLedger progressDiffLedger) => progressDiffLedgers.Add(progressDiffLedger);

        public virtual ProgressDiffLedger GetLastLedgerOrDefault(ProgressTracker progressTracker)
        {
            ProgressDiffLedger lastLedger = progressDiffLedgers.LastOrDefault();
            if (lastLedger is null)
            {
                /*return new ProgressDiffLedger(0, progressTracker.CurrLevel, progressTracker.currClass, new Dictionary<string, ProgressDiffClassLedger>() {
                    { "", new ProgressDiffClassLedger()}});*/
                return new ProgressDiffLedger(0, progressTracker.CurrLevel, ISF_DefOf.ISF_Generic_Class, new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>() {
                    { ISF_DefOf.ISF_Generic_Class, new ProgressDiffClassLedger()}});
            }
            return lastLedger;
        }

        public virtual int GetNextLedgerID => progressDiffLedgers.Count();

        // Logs modifiers from the ProgressLevelModifier
        public virtual void LogModifiers(ProgressLevelModifier modulo, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(modulo.statOffsets));
            classLedger.statFactorsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(modulo.statFactorOffsets));
            classLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(ProgressDiffLogUtility.ListToDiffDict(modulo.capMods));
        }

        // Logs modifiers from the ProgressLevelOption
        public virtual void LogModifiers(ProgressLevelOption option, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(option.statOffsets));
            classLedger.statFactorsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(option.statFactorOffsets));
            classLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(ProgressDiffLogUtility.ListToDiffDict(option.capMods));
        }

        public virtual void LogModifiers(ref ProgressDiffClassLedger classLedger, List<StatModifier> offsets = null, List<StatModifier> factorOffsets = null,
            List<PawnCapacityModifier> capMods = null)
        {
            classLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(offsets));
            classLedger.statFactorsTotal.DiffDictSum<StatDef, float>(ProgressDiffLogUtility.ListToDiffDict(factorOffsets));
            classLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(ProgressDiffLogUtility.ListToDiffDict(capMods));
        }

        public virtual void LogAbilities(ProgressLevelModifier modifier, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.abilityTotal.DiffDictSum<AbilityDef, int>(ProgressDiffLogUtility.ListToDiffDict(modifier.abilityGain, modifier.abilityRemove));
        }

        public virtual void LogAbilities(ProgressLevelOption option, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.abilityTotal.DiffDictSum<AbilityDef, int>(ProgressDiffLogUtility.ListToDiffDict(option.abilityGain, option.abilityRemove));
        }

        public virtual void LogAbilities(LearningTreeNodeDef node, ref ProgressDiffClassLedger classLedger)
        {
            classLedger.abilityTotal.DiffDictSum<AbilityDef, int>(ProgressDiffLogUtility.ListToDiffDict(node.abilityGain, node.abilityRemove));
        }

        /*public ProgressDiffClassLedger TotalDiff(string progressClass = "")
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
        }*/

        public ProgressDiffClassLedger TotalDiff(ProgressTrackerClassDef progressDiffClass)
        {
            ProgressDiffClassLedger totalLedger = new ProgressDiffClassLedger();

            // for each diff ledger
            foreach (var l in progressDiffLedgers)
            {
                // if the progressledger somehow has no assiociated class ledger, ignore it in the calc
                if (l.classDiffLedgers.NullOrEmpty()) continue;

                // in the case we specify the generic case (or null), we want everything
                if (progressDiffClass is null || progressDiffClass == ISF_DefOf.ISF_Generic_Class)
                {
                    // for each diff ledger, add class diff ledgers to the summing dicts
                    foreach (var cl in l.classDiffLedgers.Values)
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
                if (l.classDiffLedgers.ContainsKey(progressDiffClass))
                {
                    totalLedger.statOffsetsTotal.DiffDictSum<StatDef, float>(l.classDiffLedgers[progressDiffClass].statOffsetsTotal);
                    totalLedger.statFactorsTotal.DiffDictSum<StatDef, float>(l.classDiffLedgers[progressDiffClass].statFactorsTotal);
                    totalLedger.capModsTotal.DiffDictSum<PawnCapacityDef, float>(l.classDiffLedgers[progressDiffClass].capModsTotal);
                    totalLedger.hediffModsTotal.DiffDictSum<HediffDef, float>(l.classDiffLedgers[progressDiffClass].hediffModsTotal);
                    totalLedger.abilityTotal.DiffDictSum<AbilityDef, int>(l.classDiffLedgers[progressDiffClass].abilityTotal);
                    continue;
                }
            }

            return totalLedger;
        }

        public ProgressDiffClassLedger InvertDiff(ProgressDiffClassLedger totalDiff)
        {
            ProgressDiffClassLedger invertedLedger = new ProgressDiffClassLedger();
            invertedLedger.statOffsetsTotal = ProgressDiffLogUtility.InvertDictValues<StatDef, float>(totalDiff.statOffsetsTotal);
            invertedLedger.statFactorsTotal = ProgressDiffLogUtility.InvertDictValues<StatDef, float>(totalDiff.statFactorsTotal);
            invertedLedger.capModsTotal = ProgressDiffLogUtility.InvertDictValues<PawnCapacityDef, float>(totalDiff.capModsTotal);
            invertedLedger.hediffModsTotal = ProgressDiffLogUtility.InvertDictValues<HediffDef, float>(totalDiff.hediffModsTotal);
            invertedLedger.abilityTotal = ProgressDiffLogUtility.InvertDictValues<AbilityDef, float>(totalDiff.abilityTotal);
            return invertedLedger;
        }

        public void ApplyTotalDiff(ref ProgressTracker progressTracker)
        {
            // get the sum of all entries in the diff log
            ProgressDiffClassLedger totalDiff = TotalDiff(null);

            // see ProgressDiffLogUtility for extension methods
            progressTracker.statOffsetsTotal = totalDiff.statOffsetsTotal.DiffDictClean<StatDef, float>(); // stat offset assignment
            progressTracker.statFactorsTotal = totalDiff.statFactorsTotal.DiffDictClean<StatDef, float>(); // stat factor assignment           
            progressTracker.capModsTotal = totalDiff.capModsTotal.DiffDictClean<PawnCapacityDef, float>(); // cap mod offset assignment

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
