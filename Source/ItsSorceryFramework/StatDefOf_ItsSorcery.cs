using RimWorld;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class StatDefOf_ItsSorcery
    {
        // EnergyTracker default stats
        public static StatDef MaxEnergy_ItsSorcery;
        public static StatDef MinEnergy_ItsSorcery;
        public static StatDef AbsMaxEnergy_ItsSorcery;
        public static StatDef AbsMinEnergy_ItsSorcery;
        public static StatDef EnergyRecovery_ItsSorcery;
        public static StatDef EnergyFactor_ItsSorcery;
        public static StatDef EnergyCostFactor_ItsSorcery;
        public static StatDef CastFactor_ItsSorcery;

        // SorceryDef stats
        public static StatDef Sorcery_EnergyCost;
        public static StatDef Sorcery_MaxCasts;

        static StatDefOf_ItsSorcery()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf_ItsSorcery));
        }
    }
}
