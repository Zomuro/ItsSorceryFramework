using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningNodeRecord : IExposable
	{
        /// <summary>
        /// Used to keep a record of node completion across a schema's trackers for a pawn.
        /// Why this method? 
        /// 1) Allows learningtrackers to package requirements in a easy to view format 
        /// (i.e. putting in magic fundamentals in one tree, and fire magic in the other)
        /// 2) Prevents duplicate nodes from interacting with each other- by design, all nodes are unique within a schema.
        /// </summary>

        public LearningNodeRecord(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public LearningNodeRecord(Pawn pawn, SorcerySchemaDef schemaDef)
        {
            this.pawn = pawn;
            this.schemaDef = schemaDef;
            InitializeCompletion();
        }

        public LearningNodeRecord(Pawn pawn, SorcerySchema schema) // temp for now to test initalizing and saving
        {
            this.pawn = pawn;
            schemaDef = schema.def;
            cachedSchema = schema;
            InitializeCompletion();
        }

        public virtual void Initialize()
        {
            Log.Message("node count: " + AllNodes.Count.ToString());
            Log.Message("completion list count: " + completion.Count.ToString());
        }

        public SorcerySchema Schema
        {
            get
            {
                if (cachedSchema == null) cachedSchema = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef);

                return cachedSchema;
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value);
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref schemaDef, "schemaDef");
        }


        public void InitializeCompletion()
        {
            List<LearningTreeNodeDef> nodes = new List<LearningTreeNodeDef>(from def in DefDatabase<LearningTreeNodeDef>.AllDefsListForReading
                                                           where schemaDef.learningTrackerDefs.Contains(def.learningTrackerDef)
                                                           select def);

            foreach (LearningTreeNodeDef node in nodes) // if completion doesn't contain the node, include it and set node to false
            {
                completion[node] = false;
            }
        }

        public List<LearningTreeNodeDef> AllNodes
        {
            get
            {
                /*if (cachedAllNodes.NullOrEmpty()) // if cached nodes are empty
                {
                    // get all nodes that use the learningTrackerDefs outlined in the schema
                    cachedAllNodes = new List<LearningTreeNodeDef>(from def in DefDatabase<LearningTreeNodeDef>.AllDefsListForReading
                                                                   where Schema.def.learningTrackerDefs.Contains(def.learningTrackerDef)
                                                                   select def);

                    foreach (LearningTreeNodeDef node in cachedAllNodes) // if completion doesn't contain the node, include it and set node to false
                    {
                        if (!completion.Keys.Contains(node)) completion[node] = false;
                    }
                }

                return cachedAllNodes;*/

                return completion.Keys.ToList();
            }
        }

        public void RefreshAllNodes()
        {
            cachedAllNodes = null;
        }

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
            switch (node.prereqMode)
            {
                case LearningNodePrereqMode.All:
                    foreach (LearningTreeNodeDef prereq in node.prereqs)
                    {
                        if (!completion[prereq]) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (LearningTreeNodeDef prereq in node.prereqs)
                    {
                        if (completion[prereq]) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (node.prereqModeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(node.prereqModeMin, node.prereqs.Count());
                    foreach (LearningTreeNodeDef prereq in node.prereqs)
                    {
                        if (completion[prereq]) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public bool PrereqResearchFufilled(LearningTreeNodeDef node)
        {
            switch (node.prereqResearchMode)
            {
                case LearningNodePrereqMode.All:
                    foreach (ResearchProjectDef prereq in node.prereqsResearch)
                    {
                        if (!prereq.IsFinished) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (ResearchProjectDef prereq in node.prereqsResearch)
                    {
                        if (prereq.IsFinished) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (node.prereqResearchModeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(node.prereqResearchModeMin, node.prereqs.Count());
                    foreach (ResearchProjectDef prereq in node.prereqsResearch)
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

        public Tuple<int, int> PrereqsDone(LearningTreeNodeDef node)
        {
            int prereqCount = 0;
            if (!node.prereqs.NullOrEmpty()) prereqCount = node.prereqs.Where(x => completion[x]).Count();

            int prereqResearchCount = 0;
            if (!node.prereqs.NullOrEmpty()) prereqResearchCount = node.prereqsResearch.Where(x => x.IsFinished).Count();

            return new Tuple<int, int>(prereqCount, prereqResearchCount);
        }

        public string PrereqsModeNotif(LearningNodePrereqMode mode, int min = 0, int done = 0)
        {
            if (mode == LearningNodePrereqMode.Min && min > 0)
                return " (" + done + "/" + min + ")";
            if (mode == LearningNodePrereqMode.Or)
                return " (" + done + "/1)";
            return "";
        }

        public bool PrereqStatFufilled(LearningTreeNodeDef node)
        {
            if (node.prereqsStats.NullOrEmpty()) return true;
            foreach (var statReqsCase in node.prereqsStats)
            {
                foreach (var statMod in statReqsCase.statReqs)
                {
                    if (PrereqFailStatCase(statMod, statReqsCase.mode)) return false;
                }
            }

            return true;
        }

        public bool PrereqFailStatCase(StatModifier statMod, LearningNodeStatPrereqMode mode)
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

        public bool PrereqSkillFufilled(LearningTreeNodeDef node)
        {
            if (node.prereqsSkills.NullOrEmpty()) return true;
            foreach (var skillReqsCase in node.prereqsSkills)
            {
                foreach (var skillLevel in skillReqsCase.skillReqs)
                {
                    if (PrereqFailSkillCase(skillLevel.skillDef, skillLevel.ClampedLevel, skillReqsCase.mode)) return false;
                }
            }

            return true;
        }

        public bool PrereqFailSkillCase(SkillDef skillDef, int level, LearningNodeStatPrereqMode mode)
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

        public string PrereqsStatsModeNotif(LearningNodeStatPrereqMode mode)
        {
            switch (mode)
            {
                case LearningNodeStatPrereqMode.Equal:
                    return " = ";

                case LearningNodeStatPrereqMode.NotEqual:
                    return " != ";

                case LearningNodeStatPrereqMode.Greater:
                    return " > ";

                case LearningNodeStatPrereqMode.GreaterEqual:
                    return " >= ";

                case LearningNodeStatPrereqMode.Lesser:
                    return " < ";

                case LearningNodeStatPrereqMode.LesserEqual:
                    return " <= ";

                default:
                    break;
            }
            return "";
        }

        public bool PrereqHediffFufilled(LearningTreeNodeDef node)
        {
            Hediff hediff;
            foreach (var pair in node.prereqsHediff)
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(pair.Key);
                if (hediff == null) return false;
                else if (hediff.Severity < pair.Value) return false;
            }

            return true;
        }

        public bool PrereqLevelFulfilled(LearningTreeNodeDef node)
        {
            if (node.prereqLevel <= Schema.progressTracker.CurrLevel) return true;
            return false;
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

        public void CompletionModifiers(LearningTreeNodeDef node)
        {
            ProgressTracker progressTracker = Schema.progressTracker; // get progresstracker
            progressTracker.AdjustModifiers(node.statOffsets, node.statFactors, node.capMods); // update list of statMods and capMods
            progressTracker.hediff.curStage = progressTracker.RefreshCurStage(); // rebuild hediffstage with adjusted stats & set hediff curstage to it
        }

        

        public Pawn pawn;

        public SorcerySchemaDef schemaDef;

        private SorcerySchema cachedSchema;

        private List<LearningTreeNodeDef> cachedAllNodes;

        private Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> cacheExclusive;

        public Dictionary<LearningTreeNodeDef, bool> completion = new Dictionary<LearningTreeNodeDef, bool>();
    }
}
