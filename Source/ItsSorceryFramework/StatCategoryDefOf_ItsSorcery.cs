using RimWorld;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class StatCategoryDefOf_ItsSorcery
    {
        public static StatCategoryDef ISF_SorcerySchema;
        public static StatCategoryDef ISF_EnergyTracker;
        public static StatCategoryDef ISF_LearningTracker;
        public static StatCategoryDef ISF_ProgressTracker;
        public static StatCategoryDef ISF_Sorcery;

        static StatCategoryDefOf_ItsSorcery()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatCategoryDefOf_ItsSorcery));
        }
    }
}
