using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class SchemaJobDefOf
    {
        public static SchemaJobDef ChargeSchema;

        static SchemaJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SchemaJobDefOf));
        }
    }
}
