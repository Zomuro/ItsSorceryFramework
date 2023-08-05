using System.Collections.Generic;
using System.Linq;
using RimWorld;
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
        }

        public LearningNodeRecord(Pawn pawn, SorcerySchema schema) // temp for now to test initalizing and saving
        {
            this.pawn = pawn;
            schemaDef = schema.def;
            cachedSchema = schema;
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

        public List<LearningTreeNodeDef> AllNodes
        {
            get
            {
                if (cachedAllNodes == null) // if cached nodes are empty
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

                return cachedAllNodes;
            }
        }

        /*
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
        }*/

        public void RefreshAllNodes()
        {
            // set the cached nodes list to null - the next time AllNodes is called, it'll recache itself and adjust completion when new learningtrackers are added
            cachedAllNodes = null;
        }

        public Pawn pawn;

        public SorcerySchemaDef schemaDef;

        private SorcerySchema cachedSchema;

        private List<LearningTreeNodeDef> cachedAllNodes;

        public Dictionary<LearningTreeNodeDef, bool> completion = new Dictionary<LearningTreeNodeDef, bool>();
    }
}
