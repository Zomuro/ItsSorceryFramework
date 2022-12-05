using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompAbilityEffect_AddEXP : CompAbilityEffect_GiveHediff
    {
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			//base.Apply(target, dest);
			
			if(parent as Sorcery != null)
            {
				Sorcery sorcery = parent as Sorcery;

				Pawn caster = parent.pawn;

				SorcerySchema schema = SorcerySchemaUtility.FindSorcerySchema(caster, sorcery.sorceryDef.sorcerySchema);

				schema.progressTracker.addExperience(Props.severity);

			}

		}


	}
}
