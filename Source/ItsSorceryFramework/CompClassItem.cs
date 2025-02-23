using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompClassItem : CompUsable
	{
		public override void PostExposeData() => base.PostExposeData();

		public CompProperties_ClassItem ClassProps => (CompProperties_ClassItem)props;

		protected override string FloatMenuOptionLabel(Pawn pawn)
		{
			if (ClassProps.schemaDef == null) 
				return "Need to add a sorcery schema def.";

			if (ClassProps.classMapping is null)
				return "Class mapping cannot be null";

			if (!ClassProps.schemaDef.progressTrackerDef.AllClasses.Contains(ClassProps.classMapping.classDef))
				return "The specified ProgressTrackerClassDef must be in the ProgressTracker's list of classes.";

			string classLabel = $" ({ClassProps.classMapping.classDef.label})";

			return string.Format(Props.useLabel, ClassProps.schemaDef.label) + classLabel;
		}
	}
}
