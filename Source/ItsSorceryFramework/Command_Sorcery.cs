using System;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class Command_Sorcery : Command_Ability
    {
		public Command_Sorcery(Ability ability) : base(ability)
		{
			this.shrinkable = true;
		}

		protected override void DisabledCheck()
		{
			SorceryDef def = (this.ability as Sorcery)?.sorceryDef;
			Pawn pawn = this.ability.pawn;
			this.disabled = false;

			if(Schema == null)
            {
				base.DisableWithReason("ISF_CommandDisableNoSchema".Translate(pawn.LabelShort, def.sorcerySchema.LabelCap));
				return;
			}

			// disabled for now - will work on later when properly costing
			/*EnergyTracker energyTracker = Schema.energyTracker;

			if (energyTracker.WouldReachLimitEnergy(def.EnergyCost, def))
            {
				base.DisableWithReason(energyTracker.DisableCommandReason().Translate(pawn.NameFullColored));
				// base.DisableWithReason(eg.DisableCommandReason().Translate(def));
				return;
			}*/

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
				string text = "";
				SorceryDef def = (this.ability as Sorcery)?.sorceryDef;

				if (Schema == null)
				{
					text += TempRightLabel(def);
					return text.TrimEndNewlines();
				}
                else
                {
					// disabled for now
					/*text += Schema.energyTracker.TopRightLabel(def);*/
					return text.TrimEndNewlines();
				}

				/*SorceryDef def = (this.ability as Sorcery)?.sorceryDef;
				EnergyTracker energyTracker = Schema.energyTracker;
				string text = "";
				text += (def?.sorcerySchema.energyTrackerDef.energyStatLabel.CapitalizeFirst()[0]) + ": " + 
					Math.Round(def.EnergyCost * energyTracker.EnergyCostFactor, 2).ToString();
				//text = "R";
				text += energyTracker.TopRightLabel(def);

				return text.TrimEndNewlines();*/
			}
		}

		public string TempRightLabel(SorceryDef sorceryDef)
		{
			// temporarily comment this out to solve getting energy properly costed
			/*return (sorceryDef?.sorcerySchema.energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst()[0]) + ": " +
					Math.Round(sorceryDef.EnergyCost, 2).ToString();*/

			return "";
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

	}
}
