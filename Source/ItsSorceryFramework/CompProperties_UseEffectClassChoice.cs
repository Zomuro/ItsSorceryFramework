using RimWorld;

namespace ItsSorceryFramework
{
    public class CompProperties_UseEffectClassChoice : CompProperties_UseEffect
	{
		public CompProperties_UseEffectClassChoice()
		{
			this.compClass = typeof(CompUseEffect_ClassChoice);
		}

		// input sorcery schema for the use effect
		public SorcerySchemaDef schemaDef;

		public ProgressLinkedClassMap classMapping;
	}
}
