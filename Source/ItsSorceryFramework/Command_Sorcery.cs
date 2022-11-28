using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

			EnergyTracker energyTracker = SorcerySchemaUtility.FindSorcerySchema(pawn, def).energyTracker;

			/*Log.Message(energyTracker.ToString());
			Log.Message((energyTracker.currentEnergy - def.EnergyCost).ToString());
			Log.Message((energyTracker.WouldReachLimitEnergy(def.EnergyCost)).ToString());*/

			if (energyTracker.WouldReachLimitEnergy(def.EnergyCost, def))
            {
				base.DisableWithReason(energyTracker.DisableCommandReason().Translate(pawn.NameFullColored));
				// base.DisableWithReason(eg.DisableCommandReason().Translate(def));
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
				SorceryDef def = (this.ability as Sorcery)?.sorceryDef;
				EnergyTracker energyTracker = SorcerySchemaUtility.FindSorcerySchema(this.ability.pawn, def).energyTracker;
				string text = "";
				/*text += (def?.sorcerySchema.energyTrackerDef.energyStatLabel.CapitalizeFirst()[0]) + ": " + 
					Math.Round(def.EnergyCost * energyTracker.EnergyCostFactor, 2).ToString();*/
				//text = "R";
				text += energyTracker.TopRightLabel(def);

				return text.TrimEndNewlines();
			}
		}

	}
}
