using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class SorceryDef : AbilityDef
    {
		public float EnergyCost
		{
			get
			{
				return this.statBases.GetStatValueFromList(StatDefOf_ItsSorcery.Sorcery_EnergyCost, 0f);
			}
		}

		public int MaximumCasts
		{
			get
			{
				return (int) this.statBases.GetStatValueFromList(StatDefOf_ItsSorcery.Sorcery_MaxCasts, 1);
			}
		}

		public IEnumerable<string> SorceryStatSummary(Pawn forPawn = null)
		{
			//yield return  "Schema : " + this.sorcerySchema.label.CapitalizeFirst();
			if (this.EnergyCost != 0)
			{
				EnergyTracker energyTracker = SorcerySchemaUtility.FindSorcerySchema(forPawn, this).energyTracker;
				yield return this.sorcerySchema.energyTrackerDef.energyLabelTranslationKey.Translate().CapitalizeFirst() + ": "+ 
					Math.Round(this.EnergyCost * energyTracker.EnergyCostFactor, 2);
			}
			if (this.verbProperties.warmupTime > 1.401298E-45f)
			{
				yield return "AbilityCastingTime".Translate() + ": " + this.verbProperties.warmupTime + "LetterSecond".Translate();
			}
			float num = this.EffectDuration(forPawn);
			if (num > 1.401298E-45f)
			{
				int num2 = num.SecondsToTicks();
				yield return "AbilityDuration".Translate() + ": " + ((num2 >= 2500) ? num2.ToStringTicksToPeriod(true, false, true, true) : (num + "LetterSecond".Translate()));
			}
			if (this.HasAreaOfEffect)
			{
				yield return "AbilityEffectRadius".Translate() + ": " + Mathf.Ceil(this.EffectRadius);
			}
			yield break;
		}

		public string GetSorceryTooltip(Pawn pawn = null)
		{
			if (this.cachedTooltip == null || this.cachedTooltipPawn != pawn)
			{
				this.cachedTooltip = this.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n" + this.sorcerySchema.label.CapitalizeFirst() + 
					((this.level > 0) ? ("\n" + "Level".Translate().CapitalizeFirst() + " " + this.level) : "") + 
					"\n\n" +this.description;
				this.cachedTooltipPawn = pawn;
				string text = this.SorceryStatSummary(pawn).ToLineList(null, false);
				if (!text.NullOrEmpty())
				{
					this.cachedTooltip = this.cachedTooltip + "\n\n" + text;
				}
			}
			
			return this.cachedTooltip;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			base.SpecialDisplayStats(req);
			yield break;
		}

		public SorcerySchemaDef sorcerySchema; // links sorcery to a magic system

		//public StatDef energyCostStat;

		public string cachedTooltip;

		public Pawn cachedTooltipPawn;

		
	}
}
