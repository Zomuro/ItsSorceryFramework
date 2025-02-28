using RimWorld;

namespace ItsSorceryFramework
{
    public class CompProperties_ClassChoiceItem : CompProperties_Usable
	{
		public CompProperties_ClassChoiceItem()
		{
			this.compClass = typeof(CompClassChoiceItem);
		}

		public SorcerySchemaDef schemaDef;

		public ProgressLinkedClassMap classMapping;
	}
}
