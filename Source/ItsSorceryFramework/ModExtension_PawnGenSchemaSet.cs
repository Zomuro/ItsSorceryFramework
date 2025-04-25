using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ModExtension_PawnGenSchemaSet : DefModExtension
    {
        public override IEnumerable<string> ConfigErrors()
        {
            foreach(var set in schemaSets) foreach (var configError in set.ConfigErrors()) yield return configError;
            yield break;
        }

        public List<SchemaSet> schemaSets;
    }

    public class SchemaSet
    {
        public List<SchemaNodeMap> schemaMappings;

        public IEnumerable<string> ConfigErrors()
        {
            bool error = false;
            foreach (var mapping in schemaMappings)
            {
                if(mapping.forceLevel && mapping.level > mapping.schema.progressTrackerDef.progressHediff.maxSeverity)
                {
                    yield return "Cannot force level greater than the maximum level of the magic system.";
                }

                if (mapping.requiredNodes.NullOrEmpty()) continue;
                foreach (var configError in mapping.ConfigErrors()) yield return configError;
            }

            if (error) Log.Error("A SchemaSet has an error: see the warnings above.");

            yield break;
        }

        public SchemaNodeMap GetRandSchema()
        {
            if (schemaMappings.NullOrEmpty()) return null;
            return schemaMappings.RandomElementByWeight(x => x.WeightConstrained);
        }
    }

    public class SchemaNodeMap
    {
        public SorcerySchemaDef schema;

        public bool forceLevel = false; // instead of leveling pawn as needed, level to the set level

        public int level = 1;

        public bool forcePoints = false; // instead of getting the necessary skill points as needed, level to the set level

        public int points = 1;

        public float weight = 1;

        public List<SchemaNodeReq> requiredNodes;

        public ProgressTrackerClassDef baseClassDef;
        
        public List<ProgressLinkedClassMap> classChanges;

        private List<LearningTreeNodeDef> cachedNodes;

        public float WeightConstrained => weight > 0 ? weight : 1f;

        public List<LearningTreeNodeDef> NodeList
        {
            get
            {
                if (cachedNodes is null) cachedNodes = requiredNodes.Select(x => x.nodeDef).ToList();
                return cachedNodes;
            }
        }

        public IEnumerable<string> ConfigErrors()
        {
            int tempIndex = -1;
            for (int i = NodeList.Count() - 1; i >= 0; i--)
            {
                if (NodeList[i].prereqNodes.NullOrEmpty()) continue;

                switch (NodeList[i].prereqNodeMode)
                {
                    case LearningNodePrereqMode.All: // all prereqs required? 
                        foreach (var prereq in NodeList[i].prereqNodes)
                        {
                            if (NodeList.FindIndex(x => x == prereq) <= -1) yield return NodeList[i].label + " is missing the " + prereq.label + " prereq in the SchemaNodeMap.";
                            if (NodeList.FindIndex(x => x == prereq) >= i) yield return NodeList[i].label + " must be placed after the " + prereq.label + " prereq in the SchemaNodeMap.";
                        }
                        break;

                    case LearningNodePrereqMode.Or: // only one prereq required? 
                        bool missing = true;

                        foreach (var prereq in NodeList[i].prereqNodes)
                        {
                            tempIndex = NodeList.FindIndex(x => x == prereq);
                            if (tempIndex > -1 && tempIndex < i)
                            {
                                missing = false;
                                break;
                            }
                            else if (tempIndex >= i) 
                                yield return NodeList[i].label + " must be placed after the " + prereq.label + " prereq in the SchemaNodeMap.";
                        }
                        if (missing) yield return "No prerequisites have been detected for the node " + NodeList[i].label + ".";
                        break;

                    case LearningNodePrereqMode.Min: // a min number required? 
                        int reqCount = Mathf.Clamp(0, NodeList[i].prereqNodeModeMin, NodeList[i].prereqNodes.Count());
                        int count = 0;
                        
                        foreach (var prereq in NodeList[i].prereqNodes)
                        {
                            tempIndex = NodeList.FindIndex(x => x == prereq);
                            if (tempIndex >= i)
                            {
                                yield return NodeList[i].label + " must be placed after the " + prereq.label + " prereq in the SchemaNodeMap.";
                                continue;
                            }
                            else if (tempIndex > -1 && tempIndex < i) count++;
                        }
                        if (count < reqCount) yield return NodeList[i].label + " does not have enough prerequsites present in the SchemaNodeMap.";
                        break;

                    default:
                        break;
                }
            }
            yield break;
        }

    }

    public class SchemaNodeReq
    {
        public LearningTreeNodeDef nodeDef;

        public bool forceHediff = false;

        public bool forceSkill = false;

        public bool forceLevel = false;
    }
}
