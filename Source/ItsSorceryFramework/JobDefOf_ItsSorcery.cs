using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class JobDefOf_ItsSorcery
    {
        public static JobDef ISF_ChargeSchema;

        static JobDefOf_ItsSorcery()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_ItsSorcery));
        }
    }
}
