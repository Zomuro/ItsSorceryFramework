using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompClassChoiceItem : CompUsable
	{
		public override void PostExposeData() => base.PostExposeData();

		public CompProperties_ClassChoiceItem ClassProps => (CompProperties_ClassChoiceItem)props;

		protected override string FloatMenuOptionLabel(Pawn pawn)
		{
			if (SorcerySchemaUtility.GetSorceryComp(pawn) is null)
				return "Pawn does not have a It's Sorcery! comp.";

			if (ClassProps.schemaDef == null) 
				return "Need to add a sorcery schema def.";

			if (ClassProps.classMapping is null)
				return "Class mapping cannot be null.";

			if (ClassProps.schemaDef.progressTrackerDef != ClassProps.classMapping.classDef.progressTrackerDef)
				return "The specified ProgressTrackerClassDef must be linked to the schema's ProgressTrackerDef.";

			string classFinalLabel = $"{ClassProps.schemaDef.label} ({ClassProps.classMapping.classDef.label})";

			return string.Format(Props.useLabel, classFinalLabel);
		}
	}
}
