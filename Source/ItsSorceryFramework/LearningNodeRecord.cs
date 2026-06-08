using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class LearningNodeRecord : IExposable
	{
        public Pawn pawn;

        public SorcerySchema schema;

        private Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> cacheExclusive;

        public Dictionary<LearningTreeNodeDef, bool> completion = new Dictionary<LearningTreeNodeDef, bool>();

        public Dictionary<LearningTreeNodeDef, int> completionRepeated = new Dictionary<LearningTreeNodeDef, int>();

        public LearningNodeRecord(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public LearningNodeRecord(Pawn pawn, SorcerySchema schema)
        {
            this.pawn = pawn;
            this.schema = schema;
            InitializeCompletion(); // initialize completion dict when we first initialize this object
        }


        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value); // general completion dict
            Scribe_Collections.Look(ref completionRepeated, "completionRepeated", LookMode.Def, LookMode.Value); // repeated completion dict
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref schema, "schema");

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs) // on loading, update completion/completionRepeated dicts with new nodes
            {
                InitializeCompletion();
            }
        }


        public void InitializeCompletion()
        {
            // consider simplifying system to only one dict w/ int

            // get hashlist of all nodes
            HashSet<LearningTreeNodeDef> nodesHashList = new List<LearningTreeNodeDef>(from def in DefDatabase<LearningTreeNodeDef>.AllDefsListForReading
                                                                            where schema.def.learningTrackerDefs.Contains(def.learningTrackerDef)
                                                                            select def).ToHashSet();

            HashSet<LearningTreeNodeDef> completionHashList = completion.Keys.ToHashSet();

            HashSet<LearningTreeNodeDef> nodesRepeatedHashList = nodesHashList.Where(x => x.repeatable == true).ToHashSet();
            HashSet<LearningTreeNodeDef> completionRepeatedHashList = completionRepeated.Keys.ToHashSet();

            IEnumerable<LearningTreeNodeDef> completionAdd = nodesHashList.Except(completionHashList);
            IEnumerable<LearningTreeNodeDef> completionRemove = completionHashList.Except(nodesHashList);
            IEnumerable<LearningTreeNodeDef> completionRepeatAdd = nodesHashList.Except(completionRepeatedHashList);
            IEnumerable<LearningTreeNodeDef> completionRepeatRemove = completionRepeatedHashList.Except(nodesHashList);

            foreach (var n in completionAdd) completion[n] = false;
            foreach (var n in completionRemove) completion.Remove(n);
            foreach (var n in completionRepeatAdd) completionRepeated[n] = completion[n] ? 1 : 0;
            foreach (var n in completionRepeatRemove) completionRepeated.Remove(n);

            InitializeMetrics();
        }

        public virtual void InitializeMetrics()
        {
            if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
                Log.Message($"{schema.def.LabelCap} Stats; Learning node count: {AllNodes.Count}; " +
                    $"Completion list count {completion.Count}; " +
                    $"Repeatable completion list count {completionRepeated.Count}");
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

        public string CompletionNodeTreeLabel(LearningTreeNodeDef node)
        {
            // creates correct node labels on the learningtracker_tree
            if (!node.repeatable) return node.LabelCap; // if not repeatable just the label

            string labelPostFix = ""; // otherwise
            if (node.repeatLimit <= 0) labelPostFix = $" ({completionRepeated[node] + 1})";
            else labelPostFix = $" ({Mathf.Min(completionRepeated[node] + 1, node.repeatLimit)}/{node.repeatLimit})";

            return node.LabelCap + labelPostFix;
        }

        public string CompletionNodePrereqLabel(LearningTreeNodeDef node)
        {
            // gets correct node labels at the node's own defined level of completion
            if (!node.repeatable) return node.LabelCap; // if not repeatable just the label

            string labelPostFix = ""; // otherwise
            if (node.repeatLimit <= 0) labelPostFix = $" ({node.repeatCompPrereq})";
            else labelPostFix = $" ({node.RepeatCompPrereqClamped}/{node.repeatLimit})";

            return node.LabelCap + labelPostFix;
        }

        public LearningTreeNodeDef GetRepeatNodeDef(LearningTreeNodeDef node)
        {
            return node.GetRepeatNodeDef(completionRepeated[node]);
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

        public bool ValidateCompletionPrereqs(LearningTreeNodeDef node)
        {
            if (!PrereqFufilledProhibit(node) && !PrereqResearchFufilledProhibit(node) &&
                !PrereqGenesFulfilledProhibit(node) && !PrereqTraitsFulfilledProhibit(node) &&
                !PrereqXenotypeFulfilledProhibit(node) && !PrereqAgeFulfilledProhibit(node) &&
                !PrereqLevelFulfilledProhibit(node) && !PrereqStatFulfilledProhibit(node) &&
                !PrereqSkillFulfilledProhibit(node) && !PrereqHediffFulfilledProhibit(node) &&
                PrereqFufilled(node) && PrereqResearchFufilled(node) &&
                PrereqGenesFulfilled(node) && PrereqTraitsFulfilled(node) &&
                PrereqXenotypeFulfilled(node) && PrereqAgeFulfilled(node) &&
                PrereqLevelFulfilled(node) && PrereqStatFulfilled(node) &&
                PrereqSkillFulfilled(node) && PrereqHediffFulfilled(node) &&
                ExclusiveNodeFulfilled(node)) return true;

            return false;
        }

        public bool ValidateCompletable(LearningTreeNodeDef node) // determine if a learningtreenodedef is completeable
        {
            if (!completion[node]) return true;
            if (node.repeatable && (node.repeatLimit <= 0 || completionRepeated[node] < node.repeatLimit)) return true;

            return false;
        }

        public bool ValidateNodeCanFufillPrereq(LearningTreeNodeDef node) // figure out if a node qualifies as a prereq
        {
            if (!completion[node]) return false; // if completion is false it's not good for prereq
            if (node.repeatable && completionRepeated[node] < node.RepeatCompPrereqClamped) return false; // if it hasn't been completed x amount of times then false

            return true;
        }

        public void CompletionRecordUpdate(LearningTreeNodeDef node)
        {
            // do NOT attempt to complete this completion record
            completion[node] = true;
            if (node.repeatable) completionRepeated[node] += 1;
        }


    }
}
