using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompLearningTreeItem : CompUsable
	{
		public override void PostExposeData()
		{
			base.PostExposeData();
		}

		public CompProperties_LearningTreeItem LearningProps
		{
			get
			{
				return (CompProperties_LearningTreeItem)this.props;
			}
		}

		protected override string FloatMenuOptionLabel(Pawn pawn)
		{
			if (SorcerySchemaUtility.GetSorceryComp(pawn) is null) return "Pawn does not have a It's Sorcery! comp.";
			if (LearningProps.schemaDef == null) return "Need to add a sorcery schema def.";
			if (LearningProps.learningTrackerDef == null) return "Need to add a learning tracker def to unlock.";
			return string.Format(base.Props.useLabel, $"{LearningProps.learningTrackerDef.label} ({LearningProps.schemaDef.label})");
		}
	}
}
