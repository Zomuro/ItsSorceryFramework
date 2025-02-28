using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompUseEffect_Schema : CompUseEffect
	{
		public CompProperties_UseEffectSchema Props
		{
			get
			{
				return (CompProperties_UseEffectSchema)this.props;
			}
		}

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);

			SorcerySchemaUtility.AddSorcerySchema(usedBy, Props.schemaDef, Props.progressTrackerClassDef);
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
			{
				// adjust translated string to deal w/ this
				Messages.Message("ISF_UsedSchemaItem".Translate(usedBy.Named("USER"), Props.schemaDef.label), usedBy, MessageTypeDefOf.PositiveEvent, true);
			}
		}

		public override AcceptanceReport CanBeUsedBy(Pawn p)
		{
			if (SorcerySchemaUtility.FindSorcerySchema(p, Props.schemaDef) != null)
			{
				return "ISF_UseSchemaItemFail".Translate(p.Named("USER"), Props.schemaDef.label);
			}
			return false;
		}
	}
}
