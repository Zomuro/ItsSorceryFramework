using RimWorld;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class StatDefOf_ItsSorcery
    {
        // EnergyTracker default stats
        public static StatDef ISF_MaxEnergy;
        public static StatDef ISF_MinEnergy;
        public static StatDef ISF_AbsMaxEnergy;
        public static StatDef ISF_AbsMinEnergy;
        public static StatDef ISF_EnergyRecovery;
        public static StatDef ISF_EnergyValueFactor;
        public static StatDef ISF_EnergyCostFactor;
        //public static StatDef CastFactor_ItsSorcery;
        public static StatDef ISF_ScalingStat;

        // SorceryDef stats
        public static StatDef ISF_Sorcery_EnergyCost;
        //public static StatDef Sorcery_MaxCasts;

        static StatDefOf_ItsSorcery()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf_ItsSorcery));
        }
    }
}
