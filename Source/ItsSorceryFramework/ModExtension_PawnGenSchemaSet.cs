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

        public List<SchemaSetDef> schemaSets;
    }

    
}
