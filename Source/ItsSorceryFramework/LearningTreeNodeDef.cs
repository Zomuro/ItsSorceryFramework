using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

		public bool hidden = false;

		[NoTranslate]
		public string iconPath;

		private Texture2D uiIcon = null;

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

        public bool repeatable = false;

        public int repeatLimit = 0;

		public int repeatCompPrereq = 0;

		public List<LearningTreeNodeDef> repeatNodeFactors = new List<LearningTreeNodeDef>();

        private string cachedTip;

		public int RepeatCompPrereqClamped => repeatLimit <= 0 ? repeatCompPrereq : Mathf.Clamp(repeatCompPrereq, 0, repeatLimit); // clamped value for prereq validation

        public float ViewX => coordX;

		public float ViewY => coordY;


        public Texture2D Icon
		{
			get
			{
				if (uiIcon == null)
				{
					if (!iconPath.NullOrEmpty()) uiIcon = ContentFinder<Texture2D>.Get(iconPath, true);
					else uiIcon = BaseContent.BadTex;
				}
				return uiIcon;
			}
		}

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
					yield return new StatDrawEntry(statMod.stat.category,
						statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Offset),
						statMod.stat.description, 4070, null, null, false);
				}
			}

			if (!statFactorOffsets.NullOrEmpty())
			{
				foreach (StatModifier statMod in statFactorOffsets)
				{
					yield return new StatDrawEntry(statMod.stat.category,
						statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Factor),
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

        public LearningTreeNodeDef GetRepeatNodeDef(int repeats)
        {
            if (repeatable && !repeatNodeFactors.NullOrEmpty() && repeats > 0)
            {
                foreach (LearningTreeNodeDef factor in repeatNodeFactors.OrderByDescending(x => x.level))
                {
                    // if the level devided by the modulo leaves a remainder of 0
                    if (repeats % factor.level == 0) return factor;
                }
            }

            return this;
        }
    }

	public class NodeHediffProps
    {
		public HediffDef hediffDef;

		public float severity = 1;
    }



	
}
