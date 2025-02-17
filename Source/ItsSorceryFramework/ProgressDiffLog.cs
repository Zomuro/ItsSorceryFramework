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
        }

        public ProgressDiffLog(ProgressTrackerClassDef initClassDef)
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();
        }

        public ProgressDiffLog(ProgressTracker progressTracker)
        {
            progressDiffLedgers = new List<ProgressDiffLedger>();

            progressDiffLedgers.Add(new ProgressDiffLedger(0, progressTracker.CurrLevel, ISF_DefOf.ISF_Generic_Class,
                new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>() {
                    { ISF_DefOf.ISF_Generic_Class, new ProgressDiffClassLedger()}
                }));
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref progressDiffLedgers, "progressLedgers", LookMode.Deep);
        }

        public bool PrereqClassesFufilled(ProgressTrackerClassDef targetClassDef)
        {
            HashSet<ProgressTrackerClassDef> priorClassDefs = GetClassSet;
            
            switch (targetClassDef.prereqClassMode)
            {
                case LearningNodePrereqMode.All:
                    // if even a single class def isn't in the history, return false
                    foreach (var prereqClassDef in targetClassDef.prereqsClassDefs)
                        if (!priorClassDefs.Contains(prereqClassDef)) return false;
                    
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (var prereqClassDef in targetClassDef.prereqsClassDefs)
                        if (priorClassDefs.Contains(prereqClassDef)) return false;

                    return false;

                case LearningNodePrereqMode.Min:
                    if (targetClassDef.prereqClassModeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(targetClassDef.prereqClassModeMin, targetClassDef.prereqClasses.Count());
                    foreach (var prereqClassDef in targetClassDef.prereqsClassDefs)
                    {
                        if (priorClassDefs.Contains(prereqClassDef)) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public bool PrereqResearchFufilled(ProgressTrackerClassDef targetClassDef)
        {
            switch (targetClassDef.prereqResearchMode)
            {
                case LearningNodePrereqMode.All:
                    foreach (ResearchProjectDef prereq in targetClassDef.prereqsResearch)
                    {
                        if (!prereq.IsFinished) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (ResearchProjectDef prereq in targetClassDef.prereqsResearch)
                    {
                        if (prereq.IsFinished) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (targetClassDef.prereqResearchModeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(targetClassDef.prereqResearchModeMin, targetClassDef.prereqsResearch.Count());
                    foreach (ResearchProjectDef prereq in targetClassDef.prereqsResearch)
                    {
                        if (prereq.IsFinished) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public bool PrereqLevelFulfilled(ProgressTracker progressTracker, ProgressTrackerClassDef targetClassDef)
        {
            if (targetClassDef.prereqLevel <= 0 || targetClassDef.prereqLevel <= progressTracker.CurrLevel) return true;
            return false;
        }

        public bool PrereqHediffFufilled(ProgressTracker progressTracker, ProgressTrackerClassDef targetClassDef)
        {
            Hediff hediff;
            foreach (var pair in targetClassDef.prereqsHediff)
            {
                hediff = progressTracker.pawn.health.hediffSet.GetFirstHediffOfDef(pair.Key);
                if (hediff == null) return false;
                else if (hediff.Severity < pair.Value) return false;
            }

            return true;
        }

        public bool PrereqFailSkillCase(Pawn pawn, SkillDef skillDef, int level, LearningNodeStatPrereqMode mode)
        {
            switch (mode)
            {
                case LearningNodeStatPrereqMode.Equal:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() != level) return true;
                    break;

                case LearningNodeStatPrereqMode.NotEqual:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() == level) return true;
                    break;

                case LearningNodeStatPrereqMode.Greater:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() <= level) return true;
                    break;

                case LearningNodeStatPrereqMode.GreaterEqual:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() < level) return true;
                    break;

                case LearningNodeStatPrereqMode.Lesser:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() >= level) return true;
                    break;

                case LearningNodeStatPrereqMode.LesserEqual:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() > level) return true;
                    break;

                default:
                    break;
            }

            return false;
        }

        public bool PrereqSkillFufilled(ProgressTracker progressTracker, ProgressTrackerClassDef targetClassDef)
        {
            if (targetClassDef.prereqsSkills.NullOrEmpty()) return true;
            foreach (var skillReqsCase in targetClassDef.prereqsSkills)
            {
                foreach (var skillLevel in skillReqsCase.skillReqs)
                {
                    if (PrereqFailSkillCase(progressTracker.pawn, skillLevel.skillDef, skillLevel.ClampedLevel, skillReqsCase.mode)) return false;
                }
            }

            return true;
        }

        public bool PrereqFailStatCase(Pawn pawn, StatModifier statMod, LearningNodeStatPrereqMode mode)
        {
            switch (mode)
            {
                case LearningNodeStatPrereqMode.Equal:
                    if (pawn.GetStatValue(statMod.stat) != statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.NotEqual:
                    if (pawn.GetStatValue(statMod.stat) == statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.Greater:
                    if (pawn.GetStatValue(statMod.stat) <= statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.GreaterEqual:
                    if (pawn.GetStatValue(statMod.stat) < statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.Lesser:
                    if (pawn.GetStatValue(statMod.stat) >= statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.LesserEqual:
                    if (pawn.GetStatValue(statMod.stat) > statMod.value) return true;
                    break;

                default:
                    break;
            }

            return false;
        }

        public bool PrereqStatFufilled(ProgressTracker progressTracker, ProgressTrackerClassDef targetClassDef)
        {
            if (targetClassDef.prereqsStats.NullOrEmpty()) return true;
            foreach (var statReqsCase in targetClassDef.prereqsStats)
            {
                foreach (var statMod in statReqsCase.statReqs)
                {
                    if (PrereqFailStatCase(progressTracker.pawn, statMod, statReqsCase.mode)) return false;
                }
            }

            return true;
        }

        public virtual bool ValidateClassChange(ProgressTracker progressTracker, ProgressTrackerClassDef targetClassDef, out string failString)
        {
            // failstring and fail bool
            failString = "";
            bool success = true;

            // get relevant linked classes - if the target class def isn't in it, it's not validated
            List<ProgressTrackerClassDef> linkedClasses = progressTracker.currClassDef.LinkedClassDefs;
            if (!linkedClasses.Contains(targetClassDef))
            {
                failString = $"Target class ({targetClassDef.label}) is not linked to current class ({progressTracker.currClassDef.label}).";
                return false;
            }

            List<string> fails = new List<string>();
            if (!PrereqClassesFufilled(targetClassDef)) // if the pawn hasn't been through all the class defs, fail val     
            {
                success = false;
                fails.Append("Missing class requirements.");
            }
                   
            if (!PrereqResearchFufilled(targetClassDef)) // if all research is not fufilled, fail val
            {
                success = false;
                fails.Append("Missing research requirements.");
            }

            if (!PrereqLevelFulfilled(progressTracker, targetClassDef)) // if curr level < prereq level, fail val
            {
                success = false;
                fails.Append("Below level requirement.");
            }

            if (!PrereqHediffFufilled(progressTracker, targetClassDef)) // if all hediffs are not fufilled, fail val
            {
                success = false;
                fails.Append("Unfulfilled hediff requirements.");
            }

            if (!PrereqSkillFufilled(progressTracker, targetClassDef)) // if all skills not fufilled
            {
                success = false;
                fails.Append("Unfulfilled skill requirements.");
            }

            if (!PrereqStatFufilled(progressTracker, targetClassDef)) // if all stats not fufilled
            {
                success = false;
                fails.Append("Unfulfilled stat requirements.");
            }

            // construct fail string
            failString = fails.NullOrEmpty() ? "" : string.Join("\n", fails);

            return success;
        }

        public virtual void AdjustClass(ProgressTracker progressTracker, ProgressTrackerClassDef targetClassDef, bool resetLevel = true, bool resetBenefits = false)
        {
            // handle benefits
            if (resetBenefits) // if true, add a new ledger removing the benefits of the current class
            {
                // handle current class diff info
                ProgressDiffClassLedger currClassDiff = TotalDiff(progressTracker.currClassDef); // get current class diff

                // in the case that class is exclusive, invert diff to counter class effects and add new ledger
                ProgressDiffClassLedger negateDiff = InvertDiff(currClassDiff);
                AddNewLedger(progressTracker, negateDiff.statOffsetsTotal, negateDiff.statFactorsTotal, negateDiff.capModsTotal, negateDiff.hediffModsTotal, negateDiff.abilityTotal);

                // finally apply the finalized total diff info.
                ApplyTotalDiff(ref progressTracker);
            }

            // change class
            progressTracker.currClassDef = targetClassDef;

            // handle level
            int orgLevel = progressTracker.level;
            progressTracker.level = targetClassDef.levelRange.TrueMin; // set to min of target level

            if (!resetLevel) // if we do not want to reset level, instead changing back to the original
            { 
                int levelDiff = orgLevel - progressTracker.level;
                progressTracker.ForceLevelUp(levelDiff, false);
            }

        }

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

        // get all ProgressTrackerClassDef classes that have ever been in the diff log
        public HashSet<ProgressTrackerClassDef> GetClassSet => progressDiffLedgers.Select(x => x.currClassDef).ToHashSet();

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

            // reset progresstracker hediff to apply stat/capmod modifiers
            progressTracker.ResetHediff();
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
