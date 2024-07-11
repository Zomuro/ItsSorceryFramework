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

				int priorLevel = schema.progressTracker.CurrLevel;
				schema.progressTracker.ForceLevelUp();
				
				if(Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
                {
					Log.Message($"[It's Sorcery!] Prior level: {priorLevel}; Current level: {schema.progressTracker.CurrLevel}");
				}
				

			}

		}


	}
}
