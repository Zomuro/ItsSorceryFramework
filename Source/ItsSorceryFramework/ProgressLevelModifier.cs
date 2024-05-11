using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressLevelModifier
    {
        public int level = 1;

        public int pointGain = 1;

        public List<StatModifier> statOffsets;

        public List<StatModifier> statFactorOffsets;

        public List<PawnCapacityModifier> capMods = new List<PawnCapacityModifier>();

        public List<AbilityDef> abilityGain = new List<AbilityDef>();

        public List<AbilityDef> abilityRemove = new List<AbilityDef>();

        public List<NodeHediffProps> hediffAdd = new List<NodeHediffProps>();

        public List<NodeHediffProps> hediffAdjust = new List<NodeHediffProps>();

        public List<HediffDef> hediffRemove = new List<HediffDef>();

        public List<ProgressLevelOption> options = new List<ProgressLevelOption>();

        public int optionChoices = -1;

        public List<LearningTrackerDef> specialUnlocks = new List<LearningTrackerDef>();


    }
}
