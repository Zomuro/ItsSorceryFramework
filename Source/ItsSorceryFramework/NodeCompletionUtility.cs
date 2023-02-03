using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    // incomplete class- potentially work on this to implement cross-learning tracker completion for a pawn
	public class NodeCompletionUtility : IExposable
	{
		public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value);
        }

        // this is unused for the time being: maybe implement another day.
        public List<LearningTreeNodeDef> allNodes
        {
            get
            {
                if (cachedAllNodes == null)
                {
                    cachedAllNodes = new List<LearningTreeNodeDef>(from def in DefDatabase<LearningTreeNodeDef>.AllDefsListForReading
                                                                   select def);

                    foreach (LearningTreeNodeDef node in cachedAllNodes)
                    {
                        if (!completion.Keys.Contains(node)) completion[node] = false;
                    }
                }

                return cachedAllNodes;
            }
        }

        public List<LearningTreeNodeDef> cachedAllNodes;

        public Dictionary<LearningTreeNodeDef, bool> completion = new Dictionary<LearningTreeNodeDef, bool>();
    }
}
