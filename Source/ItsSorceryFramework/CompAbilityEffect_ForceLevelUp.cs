using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompAbilityEffect_ForceLevelUp : CompAbilityEffect_GiveHediff
    {
		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			//base.Apply(target, dest);
			
			if(parent as Sorcery != null)
            {
				Sorcery sorcery = parent as Sorcery;

				Pawn caster = parent.pawn;

				SorcerySchema schema = SorcerySchemaUtility.FindSorcerySchema(caster, sorcery.sorceryDef.sorcerySchema);

				schema.progressTracker.forceLevelUp();

				Log.Message(schema.progressTracker.hediff.Severity.ToString());
				Log.Message(caster.health.hediffSet.GetFirstHediffOfDef(schema.progressTracker.hediff.def).Severity.ToString());

			}

		}


	}
}
