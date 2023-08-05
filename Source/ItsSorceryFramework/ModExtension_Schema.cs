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
        public List<SchemaReq> schema;

        public SchemaReq GetRandSchema()
        {
            if (schema.NullOrEmpty()) return null;
            return schema.RandomElementByWeight(x => x.WeightConstrained);
        }
    }

    public class SchemaReq
    {
        public SorcerySchemaDef schema;

        public List<LearningTreeNodeDef> requiredNodes;

        public float WeightConstrained
        {
            get
            {
                return weight >= 0 ? weight : 1f;
            }
        }

        public float weight = 1;
    }
}
