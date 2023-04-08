using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class SorceryDef : AbilityDef
    {
		/*public float EnergyCost
		{
			get
			{
				return this.statBases.GetStatValueFromList(StatDefOf_ItsSorcery.Sorcery_EnergyCost, 0f);
			}
		}*/

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

		/*public Dictionary<StatModifier, List<EnergyTracker>> EnergyCostDictionary
        {
            get
            {
				if(cachedEnergyTrackers is null)
                {
					if (Schema is null) cachedEnergyTrackers = null;
					cachedEnergyTrackers = new Dictionary<StatModifier, List<EnergyTracker>>();
					foreach (var statMod in statBases)
					{
						if (statMod.stat.category != StatCategoryDefOf_ItsSorcery.Sorcery_ISF) continue;
						foreach (var et in Schema.energyTrackers)
						{
							if (et.def.energyUnitStatDef == statMod.stat)
							{
								if (!cachedEnergyTrackers.ContainsKey(statMod)) cachedEnergyTrackers.Add(statMod, new List<EnergyTracker>());
								cachedEnergyTrackers[statMod].Add(et);
							}
						}
					}

				}
				return cachedEnergyTrackers;
            }
        }

		public Dictionary<StatModifier, List<EnergyTrackerDef>> EnergyDefCostDictionary
		{
			get
			{
				if (cachedEnergyTrackerDefs is null)
				{
					cachedEnergyTrackerDefs = new Dictionary<StatModifier, List<EnergyTrackerDef>>();
					foreach (var statMod in statBases)
					{
						if (statMod.stat.category != StatCategoryDefOf_ItsSorcery.Sorcery_ISF) continue;
						foreach (var et in sorcerySchema.energyTrackerDefs)
						{
							if (et.energyUnitStatDef == statMod.stat)
							{
								if (!cachedEnergyTrackerDefs.ContainsKey(statMod)) cachedEnergyTrackerDefs.Add(statMod, new List<EnergyTrackerDef>());
								cachedEnergyTrackerDefs[statMod].Add(et);
							}
						}
					}

				}
				return cachedEnergyTrackerDefs;
			}
		}*/

		public IEnumerable<string> SorceryStatSummary(Pawn forPawn = null)
		{
			/*if (this.EnergyCost != 0) // if the ability costs energy
			{
				// temporarily comment this out- work on this later
				/*
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
			}*/

			float val = 0;
			if(Schema != null)
            {
				foreach(var et in Schema.energyTrackers)
                {
					val = statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0);
					if (val == 0) continue;
					yield return et.def.energyLabelKey.Translate().CapitalizeFirst() + ": " +
						   Math.Round(val * et.EnergyCostFactor, 2);
				}
            }
            else
            {
				foreach (var et in sorcerySchema.energyTrackerDefs)
				{
					val = statBases.GetStatValueFromList(et.energyUnitStatDef, 0);
					if (val == 0) continue;
					yield return et.energyLabelKey.Translate().CapitalizeFirst() + ": " +
						   Math.Round(val, 2);
				}
			}


            /*if (!EnergyCostDictionary.NullOrEmpty())
            {
				foreach (var pair in EnergyCostDictionary)
                {
					if (pair.Key.value == 0) continue;
					foreach(var et in pair.Value)
                    {
						yield return et.def.energyLabelKey.Translate().CapitalizeFirst() + ": " +
						   Math.Round(pair.Key.value * et.EnergyCostFactor, 2);
					}
					
				}
			}
            else if (!EnergyDefCostDictionary.NullOrEmpty())
			{
				foreach (var pair in EnergyDefCostDictionary)
				{
					if (pair.Key.value == 0) continue;
					foreach (var et in pair.Value)
					{
						yield return et.energyLabelKey.Translate().CapitalizeFirst() + ": " +
						   Math.Round(pair.Key.value, 2);
					}

				}
			}*/
			
			
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
			
			return this.cachedTooltip;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			base.SpecialDisplayStats(req);
			yield break;
		}

		public SorcerySchemaDef sorcerySchema; // links sorcery to a magic system

		public string cachedTooltip;

		private Dictionary<StatModifier, List<EnergyTracker>> cachedEnergyTrackers;

		private Dictionary<StatModifier, List<EnergyTrackerDef>> cachedEnergyTrackerDefs;

		public Pawn cachedTooltipPawn;

		private SorcerySchema cachedSchema;

		
	}
}
