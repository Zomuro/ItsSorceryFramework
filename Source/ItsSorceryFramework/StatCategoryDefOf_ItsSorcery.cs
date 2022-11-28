using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

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
