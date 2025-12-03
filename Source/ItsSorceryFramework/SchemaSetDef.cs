using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class SchemaSetDef : Def
    {
        public List<SchemaNodeMapDef> schemaMappings;

        public override IEnumerable<string> ConfigErrors()
        {
            base.ConfigErrors();

            bool error = false;
            /*foreach (var mapping in schemaMappings)
            {
                if(mapping.forceLevel && mapping.level > mapping.schema.progressTrackerDef.progressHediff.maxSeverity)
                {
                    yield return "Cannot force level greater than the maximum level of the magic system.";
                }

                if (mapping.requiredNodes.NullOrEmpty()) continue;
                foreach (var configError in mapping.ConfigErrors()) yield return configError;
            }*/

            if (error) Log.Error("A SchemaSet has an error: see the warnings above.");

            yield break;
        }

        public SchemaNodeMapDef GetRandSchema()
        {
            if (schemaMappings.NullOrEmpty()) return null;
            return schemaMappings.RandomElementByWeight(x => x.WeightConstrained);
        }
    }
}
