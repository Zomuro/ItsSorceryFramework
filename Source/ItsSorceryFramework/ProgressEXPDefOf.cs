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
    public static class ProgressEXPDefOf
    {
        public static ProgressEXPDef ProgressEXPTag_CastEnergyCost;
        public static ProgressEXPDef ProgressEXPTag_OnDamage;
        public static ProgressEXPDef ProgressEXPTag_OnKill;
        public static ProgressEXPDef ProgressEXPTag_Passive;
        public static ProgressEXPDef ProgressEXPTag_DuringWork;
        public static ProgressEXPDef ProgressEXPTag_DuringMentalState;
        public static ProgressEXPDef ProgressEXPTag_UseItem;

        static ProgressEXPDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ProgressEXPDefOf));
        }

    }
}
