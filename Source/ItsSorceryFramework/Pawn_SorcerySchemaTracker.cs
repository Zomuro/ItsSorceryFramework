using System;
using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class Pawn_SorcerySchemaTracker : IExposable
    {
        public Pawn pawn;

        public List<SorcerySchema> sorcerySchemas = new List<SorcerySchema>();

        public List<GizmoEntry_QuickEnergy> quickEnergyEntries = new List<GizmoEntry_QuickEnergy>();

        private Gizmo gizmo;

        /*public List<Tuple<SorcerySchemaDef, SorcerySchemaDef>> incompatibleSchemas = 
            new List<Tuple<SorcerySchemaDef, SorcerySchemaDef>>();*/

        public Pawn_SorcerySchemaTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void SchemaTrackerTick()
        {
            if (sorcerySchemas.NullOrEmpty()) return;

            foreach (SorcerySchema schema in sorcerySchemas)
            {
                schema.SchemaTick();
            }
        }

        public Gizmo GetGizmo()
        {
            if (gizmo == null) gizmo = new Gizmo_QuickEnergy(this);
            return gizmo;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Collections.Look(ref sorcerySchemas, "sorcerySchemas", LookMode.Deep, new object[]
            {
                pawn
            });
            Scribe_Collections.Look(ref quickEnergyEntries, "quickEnergyEntries", LookMode.Deep, new object[] { });

            // save this concept for later - prevent a schema from being learned if another is there
            // maybe even anti psycast option
            //Scribe_Collections.Look(ref incompatibleSchemas, "incompatibleSchemas", LookMode.Deep, LookMode.Deep);
        }
    }

}


