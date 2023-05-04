using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    // unused for the moment - when implemented, offer the option to select a stat option
    public class ProgressLevelOption
    {
        public string label = "option";

        public string description = "";

        public List<StatModifier> statOffsets;

        public List<StatModifier> statFactorOffsets;

        public List<PawnCapacityModifier> capMods = new List<PawnCapacityModifier>();

        public List<AbilityDef> abilityGain = new List<AbilityDef>();

        public List<AbilityDef> abilityRemove = new List<AbilityDef>();

        public List<NodeHediffProps> hediffAdd = new List<NodeHediffProps>();

        public List<NodeHediffProps> hediffAdjust = new List<NodeHediffProps>();

        public List<HediffDef> hediffRemove = new List<HediffDef>();

        public int pointGain = 0;

    }
}
