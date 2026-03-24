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

            // unused at the moment

            bool error = false;
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
