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
    public static class StatDefOf_ItsSorcery
    {
        public static StatDef MaxEnergy_ItsSorcery;
        public static StatDef MinEnergy_ItsSorcery;
        public static StatDef OverMaxEnergy_ItsSorcery;
        public static StatDef EnergyRecovery_ItsSorcery;
        public static StatDef EnergyFactor_ItsSorcery;
        public static StatDef EnergyCostFactor_ItsSorcery;
        public static StatDef CastFactor_ItsSorcery;

        public static StatDef Sorcery_EnergyCost;
        public static StatDef Sorcery_MaxCasts;

        static StatDefOf_ItsSorcery()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf_ItsSorcery));
        }
    }
}
