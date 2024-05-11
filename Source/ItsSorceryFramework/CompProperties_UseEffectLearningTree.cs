using RimWorld;

namespace ItsSorceryFramework
{
    public class CompProperties_UseLearningTree : CompProperties_UseEffect
	{
		public CompProperties_UseLearningTree()
		{
			this.compClass = typeof(CompUseEffect_LearningTree);
		}

		// input sorcery schema for the use effect
		public SorcerySchemaDef schemaDef;

		public LearningTrackerDef learningTrackerDef;
	}
}
