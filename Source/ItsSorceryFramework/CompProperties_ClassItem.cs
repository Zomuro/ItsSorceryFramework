using RimWorld;

namespace ItsSorceryFramework
{
    public class CompProperties_ClassItem : CompProperties_Usable
	{
		public CompProperties_ClassItem()
		{
			this.compClass = typeof(CompClassItem);
		}

		public SorcerySchemaDef schemaDef;

		public ProgressLinkedClassMap classMapping;
	}
}
