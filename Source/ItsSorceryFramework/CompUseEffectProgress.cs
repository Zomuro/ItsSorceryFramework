using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
	public class CompUseEffectProgress : CompUseEffect
	{
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			CompProperties_UseEffectProgress compProperties_UseEffectProgress = (CompProperties_UseEffectProgress)props;
			sorcerySchemaDef = compProperties_UseEffectProgress.sorcerySchemaDef;

		}

		public override void DoEffect(Pawn usedBy) // adds exp to pawn qi-tracker
		{
			SorcerySchema schema = SorcerySchemaUtility.FindSorcerySchema(usedBy, sorcerySchemaDef);
			if (schema != null)
            {
				schema.progressTracker.forceLevelUp();
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			failReason = null;

			if (SorcerySchemaUtility.FindSorcerySchema(p, sorcerySchemaDef) == null)
            {
				failReason = "Pawn doesn't have the right magic system.";
				return false;
			}
			
			return true;
		}

		public SorcerySchemaDef sorcerySchemaDef;

	}
}
