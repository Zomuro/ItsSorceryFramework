using RimWorld;
using System.Collections.Generic;
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

		public IEnumerable<StatDrawEntry> specialDisplayMods()
		{
			if (!capMods.NullOrEmpty())
			{
				foreach (PawnCapacityModifier capMod in capMods)
				{
					if (capMod.offset != 0f)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
							capMod.capacity.GetLabelFor(true, true).CapitalizeFirst(),
							(capMod.offset * 100f).ToString("+#;-#") + "%",
							capMod.capacity.description, 4060, null, null, false);
					}
				}
			}

			if (!statOffsets.NullOrEmpty())
			{
				foreach (StatModifier statMod in statOffsets)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
						statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Offset),
						statMod.stat.description, 4070, null, null, false);
				}
			}

			if (!statFactorOffsets.NullOrEmpty())
			{
				foreach (StatModifier statMod in statFactorOffsets)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
						statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Factor),
						statMod.stat.description, 4070, null, null, false);
				}
			}

			yield break;

		}
	}
}
