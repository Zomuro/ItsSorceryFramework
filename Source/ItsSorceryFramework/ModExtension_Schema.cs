using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ModExtension_SchemaSet : DefModExtension
    {
        public List<SchemaSet> schemaSets;
    }

    public class SchemaSet
    {
        public List<SchemaNodeMap> schemaMappings;

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

        //public List<LearningTreeNodeDef> requiredNodes;

        public List<SchemaNodeReq> requiredNodes;

        public float WeightConstrained
        {
            get
            {
                return weight >= 0 ? weight : 1f;
            }
        }

        
    }

    public class SchemaNodeReq
    {
        public LearningTreeNodeDef nodeDef;

        public bool forceHediff = false;

        public bool forceSkill = false;
    }
}
