using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompUseEffect_LearningTree : CompUseEffect
	{
		public CompProperties_UseLearningTree Props
		{
			get
			{
				return (CompProperties_UseLearningTree)this.props;
			}
		}

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);

			SorcerySchema schema = SorcerySchemaUtility.FindSorcerySchema(usedBy, Props.schemaDef);
			if (schema is null)
            {
				Log.Warning($"{usedBy.ThingID}: {Props.schemaDef} not found; skipping.");
				return;
            }

			LearningTracker learningTracker = schema.learningTrackers.FirstOrDefault(x => x.def == Props.learningTrackerDef);
			if (learningTracker is null)
            {
				Log.Warning($"{usedBy.ThingID}: {Props.learningTrackerDef} not found; skipping.");
				return;
			}

			learningTracker.locked = false;
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
			{
				Messages.Message("ISF_UsedLearningTreeItem".Translate(usedBy.Named("USER"), Props.learningTrackerDef.label, Props.schemaDef.label), usedBy, MessageTypeDefOf.PositiveEvent, true);
			}
		}

		public override AcceptanceReport CanBeUsedBy(Pawn p)
		{
			SorcerySchema schema = SorcerySchemaUtility.FindSorcerySchema(p, Props.schemaDef);
			if (schema is null)
			{
				return "ISF_UseLearningTreeItemFail_Schema".Translate(p.Named("USER"), Props.schemaDef.label);
			}

			LearningTracker learningTracker = schema.learningTrackers.FirstOrDefault(x => x.def == Props.learningTrackerDef);
			if (learningTracker is null)
			{
				return "ISF_UseLearningTreeItemFail_Learning".Translate(p.Named("USER"), Props.learningTrackerDef.label, Props.schemaDef.label);
			}
			else if (!learningTracker.locked)
			{
				return "ISF_UseLearningTreeItemFail_Unlocked".Translate(p.Named("USER"), Props.learningTrackerDef.label, Props.schemaDef.label);
			}
			return base.CanBeUsedBy(p);
		}
	}
}
