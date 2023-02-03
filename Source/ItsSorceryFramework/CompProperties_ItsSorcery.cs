using Verse;

namespace ItsSorceryFramework
{

	public class CompProperties_ItsSorcery : CompProperties
	{
		//Applies Comp_ItsSorcery to all pawns, giving them a list of SorcerySchemas (magic systems) to add to

		public CompProperties_ItsSorcery() 
		{
			this.compClass = typeof(Comp_ItsSorcery);
		}
	}

}
