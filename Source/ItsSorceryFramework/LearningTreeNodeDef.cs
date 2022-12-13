using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningTreeNodeDef : Def
    {
		public float ViewX
		{
			get
			{
				return this.coordX;
			}
		}

		public float ViewY
		{
			get
			{
				return this.coordY;
			}
		}

		public IEnumerable<StatDrawEntry> specialDisplayMods()
        {
            if (!capMods.NullOrEmpty())
            {
				foreach(PawnCapacityModifier capMod in capMods)
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
				foreach(StatModifier statMod in statOffsets)
                {
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
						statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Offset), 
						statMod.stat.description, 4070, null, null, false);
				}
            }

            if (!statFactors.NullOrEmpty())
            {
				foreach (StatModifier statMod in statFactors)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
						statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Factor),
						statMod.stat.description, 4070, null, null, false);
				}
			}

			yield break;
        }

		public List<LearningTreeNodeDef> prereqs = new List<LearningTreeNodeDef>();

		public List<ResearchProjectDef> prereqsResearch = new List<ResearchProjectDef>();

		public LearningTrackerDef learningTracker;

		public float coordX = 0;

		public float coordY = 0;

		public int level;

		public int pointReq = 1;

		public bool condVisiblePrereq = false;

		[NoTranslate]
		public string iconPath;

		public List<LearningTreeNodeDef> exclusiveNodes = new List<LearningTreeNodeDef>();

		public List<AbilityDef> abilityGain = new List<AbilityDef>();

		public List<AbilityDef> abilityRemove = new List<AbilityDef>();

		public Dictionary<HediffDef, float> hediffAdjust = new Dictionary<HediffDef, float>();

		public List<HediffDef> hediffRemove = new List<HediffDef>();

		public List<StatModifier> statOffsets;

		public List<StatModifier> statFactors;

		public List<PawnCapacityModifier> capMods;

	}
}
