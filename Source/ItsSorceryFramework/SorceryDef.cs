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

		public SorcerySchema Schema
        {
            get
            {
				if(cachedSchema == null)
                {
					cachedSchema = SorcerySchemaUtility.FindSorcerySchema(cachedTooltipPawn, this);
				}

				return cachedSchema;
            }
        }

		public IEnumerable<string> SorceryStatSummary(Pawn forPawn = null)
		{
			if (this.EnergyCost != 0) // if the ability costs energy
			{
				if(Schema != null) // if the pawn has the appropiate magic system
				{
					yield return this.sorcerySchema.energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst() + ": " +
					Math.Round(this.EnergyCost * Schema.energyTracker.EnergyCostFactor, 2);
				}
				else // otherwise give the base energy cost
				{
					yield return this.sorcerySchema.energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst() + ": " +
						Math.Round(this.EnergyCost, 2);
				}
			}
			
			if (this.verbProperties.warmupTime > 0) // if warmuptime > 0, display it
			{
				yield return "AbilityCastingTime".Translate() + ": " + this.verbProperties.warmupTime + "LetterSecond".Translate();
			}
			float num = this.EffectDuration(forPawn);
			if (num > 0) // if effect duration lasts > 0, show it
			{
				int num2 = num.SecondsToTicks();
				yield return "AbilityDuration".Translate() + ": " + ((num2 >= 2500) ? num2.ToStringTicksToPeriod(true, false, true, true) : (num + "LetterSecond".Translate()));
			}
			if (this.HasAreaOfEffect) // if there's an aoe/radius to the ability, show it
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

		private SorcerySchema cachedSchema;

		
	}
}
