﻿using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class Command_Sorcery : Command_Ability
    {
		public Command_Sorcery(Ability ability, Pawn pawn) : base(ability, pawn)
		{
			this.shrinkable = true;
		}

		// public method for later; clear string caches
		public void ClearCachedStrings()
        {

        }

		public override bool Visible
		{
			get
			{
				return Ability is Sorcery sorcery && sorcery != null ? sorcery.visible : true;
			}
		}

		protected override void DisabledCheck()
		{
			SorceryDef def = (ability as Sorcery)?.sorceryDef;
			Pawn pawn = ability.pawn;
			disabled = false;

			if(Schema is null) // no schema? disable this sorcery- the person can't cast it
			{
				DisableWithReason("ISF_CommandDisableNoSchema".Translate(pawn.LabelShort, def.sorcerySchema.LabelCap));
				return;
			}

			if(Schema.energyTrackers.NullOrEmpty()) // don't bother cost checking if no energy tracker
            {
				base.DisabledCheck();
				return;
			}

			
			List<string> disableByEnergyTrackers = new List<string>();
			bool disableCheck = false;
			foreach(var et in Schema.energyTrackers) // run through energytrackers and check if it's castable
			{
				// if it'd hit the energy limit, disable the gizmo and add to the reasons
				if (et.WouldReachLimitEnergy(def.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0), def))
				{
					disableByEnergyTrackers.Add(et.DisableCommandReason().Translate(pawn.NameFullColored));
					disableCheck = true;
				}
			}

            if (disableCheck) // if it turns out it isn't castable
            {
				// join together string list and set it as the reason
				DisableWithReason(string.Join("\n", disableByEnergyTrackers));
				return;
			}

			base.DisabledCheck();
		}

		public override string Tooltip
		{
			get
			{
				return (this.ability as Sorcery)?.SorceryTooltip ?? "failure";
			}
		}

		public override string TopRightLabel
		{
			get
			{
				// future zom's problem- created cached strings for this + whenever energycostfactor gets changed, clear cache of text constructed

				string text = "";
				SorceryDef def = (this.ability as Sorcery)?.sorceryDef;

				if (Schema == null)
				{
					text += TempRightLabel(def);
					return text.TrimEndNewlines();
				}
                else
                {
					foreach (var et in Schema.energyTrackers)
					{
						if(def.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0) != 0)
							text += et.TopRightLabel(def) + "\n";
					}
					return text.TrimEndNewlines();
				}
			}
		}

		public string TempRightLabel(SorceryDef sorceryDef)
		{
			// future zom's problem- created cached strings for this + whenever energycostfactor gets changed, clear cache of text constructed

			string text = "";
			float tempVal = 0;

			foreach (var et in sorceryDef.sorcerySchema.energyTrackerDefs)
			{
				tempVal = sorceryDef.statBases.GetStatValueFromList(et.energyUnitStatDef, 0) * Schema.pawn.GetStatValue(et.energyCostFactorStatDef);
				if (tempVal != 0) text += TempRightLabelPart(et, tempVal) + "\n";
			}

			return text.TrimEndNewlines();
		}

		public string TempRightLabelPart(EnergyTrackerDef energyTrackerDef, float value)
        {
			return (energyTrackerDef.LabelCap[0]) + ": " +
					Math.Round(value, 2).ToString();
		}

		public SorcerySchema Schema
        {
            get
            {
				if(cachedSchema == null)
                {
					cachedSchema = SorcerySchemaUtility.FindSorcerySchema(ability.pawn, (this.ability as Sorcery)?.sorceryDef);
				}

				return cachedSchema;
            }
        }

		private SorcerySchema cachedSchema;

		private string cachedRightLabel;

	}
}
