using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class JobDefOf_ItsSorcery
    {
        public static JobDef ISF_ChargeSchema;
        public static JobDef ISF_GainEXPSchema;

        static JobDefOf_ItsSorcery()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_ItsSorcery));
        }
    }
}
