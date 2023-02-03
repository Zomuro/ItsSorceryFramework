using RimWorld;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class StatCategoryDefOf_ItsSorcery
    {
        public static StatCategoryDef SorcerySchema_ISF;
        public static StatCategoryDef EnergyTracker_ISF;
        public static StatCategoryDef LearningTracker_ISF;
        public static StatCategoryDef ProgressTracker_ISF;

        static StatCategoryDefOf_ItsSorcery()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatCategoryDefOf_ItsSorcery));
        }
    }
}
