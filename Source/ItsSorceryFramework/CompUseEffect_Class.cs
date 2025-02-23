using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompUseEffect_Class : CompUseEffect
	{
		public CompProperties_UseEffectClass Props
		{
			get
			{
				return (CompProperties_UseEffectClass)this.props;
			}
		}

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);

			SorcerySchema schema = SorcerySchemaUtility.FindSorcerySchema(usedBy, Props.schemaDef);
			if (schema is null) return;

			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
			{
				// adjust translated string to deal w/ this
				Messages.Message("ISF_UsedClassItem".Translate(usedBy.Named("USER"), Props.classMapping.classDef.label), usedBy, MessageTypeDefOf.PositiveEvent, true);
			}
		}

		public override AcceptanceReport CanBeUsedBy(Pawn p)
		{
			SorcerySchema schema = SorcerySchemaUtility.FindSorcerySchema(p, Props.schemaDef);

			// no schema? error
			if (schema is null) 
				return "ISF_UseItemMissingSchema".Translate(p.Named("USER"), Props.schemaDef.label);

			// does the current class Def have a mapping linked to ours?
			if (!schema.progressTracker.currClassDef.linkedClasses.Contains(Props.classMapping))
				return "ISF_UseClassItemFailNoLink".Translate(p.Named("USER"), Props.classMapping.classDef.label);

			// does the class opportunity already exist?
			if (schema.progressTracker.classChangeOpps.Contains(Props.classMapping))
				return "ISF_UseClassItemFail".Translate(p.Named("USER"), Props.classMapping.classDef.label);

			return base.CanBeUsedBy(p);
		}
	}
}
