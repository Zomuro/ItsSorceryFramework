using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningNodeRecord : IExposable
	{
        public Pawn pawn;

        public SorcerySchema schema;

        private Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> cacheExclusive;

        public Dictionary<LearningTreeNodeDef, bool> completion = new Dictionary<LearningTreeNodeDef, bool>();

        public LearningNodeRecord(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public LearningNodeRecord(Pawn pawn, SorcerySchema schema)
        {
            this.pawn = pawn;
            this.schema = schema;
            InitializeCompletion();
        }


        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value);
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref schema, "schema");
        }


        public void InitializeCompletion()
        {
            List<LearningTreeNodeDef> nodes = new List<LearningTreeNodeDef>(from def in DefDatabase<LearningTreeNodeDef>.AllDefsListForReading
                                                           where schema.def.learningTrackerDefs.Contains(def.learningTrackerDef)
                                                           select def);

            // if completion doesn't contain the node, include it and set node to false
            foreach (LearningTreeNodeDef node in nodes) completion[node] = false;

            InitializeMetrics();
        }

        public virtual void InitializeMetrics()
        {
            if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
                Log.Message($"{schema.def.LabelCap} Stats; Learning node count: {AllNodes.Count}; Completion list count {completion.Count}");
        }

        public List<LearningTreeNodeDef> AllNodes => completion.Keys.ToList();

        public void RefreshExclusiveNodes() => cacheExclusive = null;

        public Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> ExclusiveNodes
        {
            get
            {
                if (cacheExclusive == null)
                {
                    Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> exclusive = new Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>>();
                    foreach (LearningTreeNodeDef node in AllNodes)
                    {
                        if (!exclusive.ContainsKey(node)) exclusive[node] = node.exclusiveNodes.Distinct().ToList();

                        foreach (LearningTreeNodeDef conflict in node.exclusiveNodes)
                        {
                            if (!exclusive.ContainsKey(conflict)) exclusive[conflict] = new List<LearningTreeNodeDef>() { node };
                            else exclusive[conflict].AddDistinct(node);
                        }
                    }

                    cacheExclusive = exclusive;
                }

                return cacheExclusive;
            }
        }

        public bool PrereqFufilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqNodeFufilled(this, node.prereqs, node.prereqMode, node.prereqModeMin);
        }

        public bool PrereqResearchFufilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqResearchFufilled(node.prereqsResearch, node.prereqResearchMode, node.prereqResearchModeMin);
        }

        public Tuple<int, int> PrereqsDone(LearningTreeNodeDef node)
        {
            int prereqCount = 0;
            if (!node.prereqs.NullOrEmpty()) prereqCount = node.prereqs.Where(x => completion[x]).Count();

            int prereqResearchCount = 0;
            if (!node.prereqsResearch.NullOrEmpty()) prereqResearchCount = node.prereqsResearch.Where(x => x.IsFinished).Count();

            return new Tuple<int, int>(prereqCount, prereqResearchCount);
        }

        public string PrereqsModeNotif(LearningNodePrereqMode mode, int min = 0, int done = 0)
        {
            return PrereqUtility.PrereqsModeNotif(mode, min, done);
        }

        public bool PrereqStatFufilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqStatFufilled(pawn, node.prereqsStats);
        }

        public bool PrereqSkillFufilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqSkillFufilled(pawn, node.prereqsSkills);
        }

        public bool PrereqHediffFufilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqHediffFufilled(pawn, node.prereqsHediff);
        }

        public bool PrereqLevelFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqLevelFufilled(schema.progressTracker, node.prereqLevel);
        }

        public bool ExclusiveNodeFufilled(LearningTreeNodeDef node)
        {
            if (!ExclusiveNodes.ContainsKey(node)) return true;

            foreach (LearningTreeNodeDef ex in ExclusiveNodes[node])
            {
                if (completion[ex]) return false;
            }

            return true;
        }

        
        public void CompletionAbilities(LearningTreeNodeDef node)
        {
            Pawn_AbilityTracker abilityTracker = pawn.abilities;

            foreach (AbilityDef abilityDef in node.abilityGain)
            {
                abilityTracker.GainAbility(abilityDef);
            }

            foreach (AbilityDef abilityDef in node.abilityRemove)
            {
                abilityTracker.RemoveAbility(abilityDef);
            }
        }

        public void CompletionAbilities(LearningTreeNodeDef node, ref ProgressDiffClassLedger classLedger)
        {
            Pawn_AbilityTracker abilityTracker = this.pawn.abilities;

            foreach (AbilityDef abilityDef in node.abilityGain)
            {
                abilityTracker.GainAbility(abilityDef);
            }

            foreach (AbilityDef abilityDef in node.abilityRemove)
            {
                abilityTracker.RemoveAbility(abilityDef);
            }

            schema.progressTracker.progressDiffLog.LogAbilities(node, ref classLedger);
        }

        public void CompletionHediffs(LearningTreeNodeDef node)
        {
            Hediff hediff;
            foreach (NodeHediffProps props in node.hediffAdd)
            {
                hediff = HediffMaker.MakeHediff(props.hediffDef, pawn, null);
                hediff.Severity = props.severity;

                pawn.health.AddHediff(hediff, null, null, null);
            }

            foreach (NodeHediffProps props in node.hediffAdjust)
            {
                HealthUtility.AdjustSeverity(pawn, props.hediffDef, props.severity);
            }

            foreach (HediffDef hediffDef in node.hediffRemove)
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null) pawn.health.RemoveHediff(hediff);
            }
        }

        public void CompletionHediffs(LearningTreeNodeDef node, ref ProgressDiffClassLedger classLedger)
        {
            Dictionary<HediffDef, float> returnDict = new Dictionary<HediffDef, float>() { };

            Hediff hediff;
            foreach (NodeHediffProps props in node.hediffAdd)
            {
                hediff = HediffMaker.MakeHediff(props.hediffDef, pawn, null);
                hediff.Severity = props.severity;

                if (returnDict.ContainsKey(props.hediffDef)) returnDict[props.hediffDef] += props.severity;
                else returnDict[props.hediffDef] = props.severity;

                pawn.health.AddHediff(hediff, null, null, null);
            }

            foreach (NodeHediffProps props in node.hediffAdjust)
            {
                HealthUtility.AdjustSeverity(pawn, props.hediffDef, props.severity);
                if (returnDict.ContainsKey(props.hediffDef)) returnDict[props.hediffDef] += props.severity;
                else returnDict[props.hediffDef] = props.severity;
            }

            foreach (HediffDef hediffDef in node.hediffRemove)
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null)
                {
                    if (returnDict.ContainsKey(hediffDef)) returnDict[hediffDef] -= hediff.Severity;
                    else returnDict[hediffDef] = -hediff.Severity;
                    pawn.health.RemoveHediff(hediff);
                }
            }

            classLedger.hediffModsTotal.DiffDictSum<HediffDef, float>(returnDict);
        }

        public void CompletionModifiers(LearningTreeNodeDef node, ref ProgressDiffClassLedger classLedger)
        {
            schema.progressTracker.AdjustModifiers(ref classLedger, node.statOffsets, node.statFactors, node.capMods); // update list of statMods and capMods
            schema.progressTracker.ResetHediff(); // refresh hediff
        }

        public void CompletionLearningUnlock(LearningTreeNodeDef node)
        {
            if (node.unlocks.NullOrEmpty()) return;

            foreach(var lt in schema.learningTrackers)
            {
                if (node.unlocks.Contains(lt.def)) lt.locked = false;
            }
        }

    }
}
