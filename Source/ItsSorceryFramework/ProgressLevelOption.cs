using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    // unused for the moment - when implemented, offer the option to select a stat option
    public class ProgressLevelOption
    {
        public string label = "option";

        public List<StatModifier> statOffsets;

        public List<StatModifier> statFactors;

        public List<PawnCapacityModifier> capMods = new List<PawnCapacityModifier>();

        public int pointGain = 0;

    }
}
