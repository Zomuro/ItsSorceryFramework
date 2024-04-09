using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class SorceryDef : AbilityDef
    {
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
			float val = 0;
			if(Schema != null)
            {
				foreach(var et in Schema.energyTrackers)
                {
					val = statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0);
					yield return et.EnergyLabel.CapitalizeFirst() + ": " +
						Math.Round(val * et.EnergyCostFactor, 2);
					
				}
            }
            else
            {
				foreach (var et in sorcerySchema.energyTrackerDefs)
				{
					val = statBases.GetStatValueFromList(et.energyUnitStatDef, 0);
					if (val != 0)
					{
						yield return et.energyUnitStatDef.label.CapitalizeFirst() + ": " +
								Math.Round(val, 2);
					}
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
			if (cachedTooltip == null || cachedTooltipPawn != pawn)
			{
				cachedTooltip = LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n" + sorcerySchema.label.CapitalizeFirst() + 
					((level > 0) ? ("\n" + "Level".Translate().CapitalizeFirst() + " " + level) : "") + 
					"\n\n" + description;
				cachedTooltipPawn = pawn;
				string text = SorceryStatSummary(pawn).ToLineList(null, false);
				if (!text.NullOrEmpty())
				{
					cachedTooltip = cachedTooltip + "\n\n" + text;
				}
			}
			
			return cachedTooltip;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			base.SpecialDisplayStats(req);
			yield break;
		}

		public SorcerySchemaDef sorcerySchema; // links sorcery to a magic system

		public string cachedTooltip;

		public Pawn cachedTooltipPawn;

		private SorcerySchema cachedSchema;

		
	}
}
