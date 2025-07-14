using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningTreeNodeDef : ISF_PrereqDef //Def
	{
		public LearningTrackerDef learningTrackerDef;

		public float coordX = 0;

		public float coordY = 0;

		public int level;

		public int pointReq = 1;

		public bool condVisiblePrereq = false;

		/*[NoTranslate]
		public string iconPath;

		private Texture2D uiIcon = null;*/

		public List<LearningTreeNodeDef> exclusiveNodes = new List<LearningTreeNodeDef>();

		public List<AbilityDef> abilityGain = new List<AbilityDef>();

		public List<AbilityDef> abilityRemove = new List<AbilityDef>();

		public List<NodeHediffProps> hediffAdd = new List<NodeHediffProps>();

		public List<NodeHediffProps> hediffAdjust = new List<NodeHediffProps>();

		public List<HediffDef> hediffRemove = new List<HediffDef>();

		public List<StatModifier> statOffsets;

		public List<StatModifier> statFactorOffsets;

		public List<PawnCapacityModifier> capMods;

		public List<LearningTrackerDef> unlocks;

		private string cachedTip;

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

		// commented out for now- revisit another time to add icons to the tree
		/*public Texture2D Icon
		{
			get
			{
				if (this.uiIcon == null)
				{
					if (!iconPath.NullOrEmpty())
					{
						this.uiIcon = ContentFinder<Texture2D>.Get(iconPath, true);
					}
					else
					{
						this.uiIcon = BaseContent.BadTex;
					}
				}
				return this.uiIcon;
			}
		}*/

		public IEnumerable<StatDrawEntry> SpecialDisplayMods()
		{
			if (!capMods.NullOrEmpty())
			{
				foreach (PawnCapacityModifier capMod in capMods)
				{
					if (capMod.offset != 0f)
					{
						yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
							capMod.capacity.GetLabelFor().CapitalizeFirst(),
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
						statMod.stat.LabelCap, (statMod.value * 100f).ToString("+#;-#") + "%", //statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Factor),
						statMod.stat.description, 4070, null, null, false);
				}
			}

			yield break;
		}

		public string GetTip()
		{
			if (this.cachedTip == null)
			{
				this.cachedTip = this.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n" + this.description;
				
				if (this.modContentPack != null && !this.modContentPack.IsCoreMod)
				{
					Color color = this.modContentPack.IsOfficialMod ? ModLister.GetExpansionWithIdentifier(this.modContentPack.PackageId.ToLower()).primaryColor : ColoredText.SubtleGrayColor;
					this.cachedTip = this.cachedTip + "\n\n" + ("Stat_Source_Label".Translate().ToString() + ": " + this.modContentPack.Name).Colorize(color);
				}
			}
			return this.cachedTip;
		}

        public override void ResolveReferences()
        {
            base.ResolveReferences();

			bool error = false;

			if(learningTrackerDef == null)
            {
				error = true;
				Log.Message("The LearningTrackerDef cannot be null.");
			}

			if(error) Log.Error("The LearningTrackerDef " + defName + " has errors.");

		}
	}

	public class NodeHediffProps
    {
		public HediffDef hediffDef;

		public float severity = 1;
    }

	
}
