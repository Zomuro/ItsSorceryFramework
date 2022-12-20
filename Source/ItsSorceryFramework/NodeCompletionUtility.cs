using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
	public class NodeCompletionUtility : IExposable
	{
		public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value);
            //base.ExposeData();
        }

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
