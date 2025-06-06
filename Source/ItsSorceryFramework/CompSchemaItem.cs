﻿using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompSchemaItem : CompUsable
	{
		public override void PostExposeData() => base.PostExposeData();

		public CompProperties_SchemaItem SchemaProps => (CompProperties_SchemaItem)this.props;

		protected override string FloatMenuOptionLabel(Pawn pawn)
		{
			if (SorcerySchemaUtility.GetSorceryComp(pawn) is null)
				return "Pawn does not have a It's Sorcery! comp.";

			if (SchemaProps.schemaDef == null) 
				return "Need to add a sorcery schema def.";

			if (SchemaProps.progressTrackerClassDef != null && SchemaProps.schemaDef.progressTrackerDef != SchemaProps.progressTrackerClassDef.progressTrackerDef) 
				return "The specified ProgressTrackerClassDef must be linked to the schema's ProgressTrackerDef.";

            string classLabel = SchemaProps.progressTrackerClassDef is null ? $" ({SchemaProps.schemaDef.progressTrackerDef.baseClass.label})" : $" ({SchemaProps.progressTrackerClassDef.label})";

			return string.Format(Props.useLabel, SchemaProps.schemaDef.label) + classLabel;
		}
	}
}
