﻿using RimWorld;
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
            return PrereqUtility.PrereqNodeFufilled(this, node.prereqNodes, node.prereqNodeMode, node.prereqNodeModeMin);
        }

        public bool PrereqResearchFufilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqResearchFufilled(node.prereqResearch, node.prereqResearchMode, node.prereqResearchModeMin);
        }

        public bool PrereqGenesFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqGenesFufilled(pawn.genes.GenesListForReading.Select(x => x.def).ToHashSet(), node.prereqGenes,
                node.prereqGeneMode, node.prereqGeneModeMin);
        }

        public bool PrereqTraitsFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqTraitsFufilled(pawn, node.prereqTraits,
                node.prereqTraitMode, node.prereqTraitModeMin);
        }

        public bool PrereqXenotypeFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqXenotypeFufilled(pawn, node.prereqXenotype);
        }

        public bool PrereqLevelFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqLevelFufilled(schema.progressTracker, node.prereqLevel, node.prereqLevelMode);
        }

        public bool PrereqAgeFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqAgeFufilled(pawn, node.prereqAge, node.prereqAgeMode, node.prereqCheckBioAge);
        }

        public bool PrereqStatFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqStatFufilled(pawn, node.prereqStats);
        }

        public bool PrereqSkillFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqSkillFufilled(pawn, node.prereqSkills);
        }

        public bool PrereqHediffFulfilled(LearningTreeNodeDef node)
        {
            return PrereqUtility.PrereqHediffFufilled(pawn, node.prereqHediffs);
        }

        public bool PrereqFufilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqNodesProhibit.NullOrEmpty()) return false;
            return PrereqUtility.PrereqNodeFufilled(this, node.prereqNodesProhibit, node.prereqNodeModeProhibit, node.prereqNodeModeMinProhibit);
        }

        public bool PrereqResearchFufilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqResearchProhibit.NullOrEmpty()) return false;
            return PrereqUtility.PrereqResearchFufilled(node.prereqResearchProhibit, node.prereqResearchModeProhibit, node.prereqResearchModeMinProhibit);
        }

        public bool PrereqGenesFulfilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqGenesProhibit.NullOrEmpty()) return false;
            return PrereqUtility.PrereqGenesFufilled(pawn.genes.GenesListForReading.Select(x => x.def).ToHashSet(), node.prereqGenesProhibit,
                node.prereqGeneModeProhibit, node.prereqGeneModeMinProhibit);
        }

        public bool PrereqTraitsFulfilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqTraitsProhibit.NullOrEmpty()) return false;
            return PrereqUtility.PrereqTraitsFufilled(pawn, node.prereqTraitsProhibit,
                node.prereqTraitModeProhibit, node.prereqTraitModeMinProhibit);
        }

        public bool PrereqXenotypeFulfilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqXenotypeProhibit is null) return false;
            return PrereqUtility.PrereqXenotypeFufilled(pawn, node.prereqXenotypeProhibit);
        }

        public bool PrereqLevelFulfilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqLevelProhibit <= 0) return false;
            return PrereqUtility.PrereqLevelFufilled(schema.progressTracker, node.prereqLevelProhibit, node.prereqLevelModeProhibit);
        }

        public bool PrereqAgeFulfilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqAgeProhibit <= 0) return false;
            return PrereqUtility.PrereqAgeFufilled(pawn, node.prereqAgeProhibit, node.prereqAgeModeProhibit, node.prereqCheckBioAgeProhibit);
        }

        public bool PrereqStatFulfilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqStatsProhibit.NullOrEmpty()) return false;
            return PrereqUtility.PrereqStatFufilled(pawn, node.prereqStatsProhibit);
        }

        public bool PrereqSkillFulfilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqSkillsProhibit.NullOrEmpty()) return false;
            return PrereqUtility.PrereqSkillFufilled(pawn, node.prereqSkillsProhibit);
        }

        public bool PrereqHediffFulfilledProhibit(LearningTreeNodeDef node)
        {
            if (node.prereqHediffsProhibit.NullOrEmpty()) return false;
            return PrereqUtility.PrereqHediffFufilled(pawn, node.prereqHediffsProhibit);
        }

        public bool ExclusiveNodeFulfilled(LearningTreeNodeDef node)
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
            schema.progressTracker.AdjustModifiers(ref classLedger, node.statOffsets, node.statFactorOffsets, node.capMods); // update list of statMods and capMods
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
