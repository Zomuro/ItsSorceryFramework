using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class Pawn_SorcerySchemaTracker : IExposable
    {
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
                //temporary
                //Log.Message(schema.def.defName);
            }
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Collections.Look(ref sorcerySchemas, "sorcerySchemas", LookMode.Deep, new object[]
            {
                this.pawn
            });
            //Scribe_Collections.Look(ref incompatibleSchemas, "incompatibleSchemas", LookMode.Deep, LookMode.Deep);
        }

        public Pawn pawn;

        public List<SorcerySchema> sorcerySchemas = new List<SorcerySchema>();

        /*public List<Tuple<SorcerySchemaDef, SorcerySchemaDef>> incompatibleSchemas = 
            new List<Tuple<SorcerySchemaDef, SorcerySchemaDef>>();*/

       
    }
}
