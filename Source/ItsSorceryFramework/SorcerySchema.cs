using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class SorcerySchema : IExposable
    {
        public SorcerySchema(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public SorcerySchema(Pawn pawn, SorcerySchemaDef def)
        {
            this.pawn = pawn;
            this.def = def;
            this.InitializeTrackers();
        }
        
        public virtual void InitializeTrackers()
        {
            this.energyTracker = Activator.CreateInstance(def.energyTrackerDef.energyTrackerClass,
                new object[] { pawn, def}) as EnergyTracker;

            foreach (LearningTrackerDef ltDef in def.learningTrackerDefs)
            {
                learningTrackers.Add(Activator.CreateInstance(ltDef.learningTrackerClass,
                    new object[] { pawn, ltDef, def }) as LearningTracker);
            }

            this.progressTracker = Activator.CreateInstance(def.progressTrackerDef.progressTrackerClass,
                new object[] { pawn, def }) as ProgressTracker;

        }

        public virtual void SchemaTick()
        {
            if (energyTracker != null) 
            {
                energyTracker.EnergyTrackerTick();
            }

            if(progressTracker != null)
            {
                progressTracker.ProgressTrackerTick();
            }
            //Log.Message("how is it empty");
            
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Deep.Look(ref energyTracker, "energyTracker", new object[] {pawn});
            Scribe_Collections.Look(ref learningTrackers, "learningTrackers", LookMode.Deep, new object[] { pawn });
            Scribe_Deep.Look(ref progressTracker, "progressTracker", new object[] { pawn });
            Scribe_Values.Look(ref favorited, "favorited", false);
        }

        public Pawn pawn;

        public SorcerySchemaDef def;

        public EnergyTracker energyTracker;

        public List<LearningTracker> learningTrackers = new List<LearningTracker>();

        public ProgressTracker progressTracker;

        public bool favorited = false;
    }
}
