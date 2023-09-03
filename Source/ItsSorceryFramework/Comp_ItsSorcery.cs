using Verse;

namespace ItsSorceryFramework
{
    // Add the tracker/gizmo to the pawn list
    [StaticConstructorOnStartup]
    public class Comp_ItsSorcery : ThingComp
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
			if (parent is Pawn p)
			{
				pawn = p;
				SchemaTrackerInitalize();
			}
		}


        /*public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (parent is Pawn p)
			{
				pawn = p;
				SchemaTrackerInitalize();
			}
		}*/

		public virtual void SchemaTrackerInitalize()
		{
			if(pawn != null)
            {
				if(schemaTracker is null) schemaTracker = new Pawn_SorcerySchemaTracker(pawn);				
			}
			
		}

		public override void CompTick()
		{
			if (schemaTracker != null) schemaTracker.SchemaTrackerTick();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref pawn, "pawn");
			Scribe_Deep.Look(ref schemaTracker, true, "schemaTracker", this.parent as Pawn);
		}

		public Pawn pawn;

		public Pawn_SorcerySchemaTracker schemaTracker;

	}
}
