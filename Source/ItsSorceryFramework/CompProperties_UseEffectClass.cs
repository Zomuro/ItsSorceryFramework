using RimWorld;

namespace ItsSorceryFramework
{
    public class CompProperties_UseEffectClass : CompProperties_UseEffect
	{
		public CompProperties_UseEffectClass()
		{
			this.compClass = typeof(CompUseEffect_Class);
		}

		// input sorcery schema for the use effect
		public SorcerySchemaDef schemaDef;

		public ProgressLinkedClassMap classMapping;
	}
}
