using RimWorld;

namespace ItsSorceryFramework
{
	public class CompProperties_UseEffectSchema : CompProperties_UseEffect
	{
		public CompProperties_UseEffectSchema()
		{
			this.compClass = typeof(CompUseEffect_Schema);
		}

		// input sorcery schema for the use effect
		public SorcerySchemaDef schemaDef;
	}
}
