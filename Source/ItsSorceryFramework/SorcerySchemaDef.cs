using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class SorcerySchemaDef : Def
    {
        public Pawn TempPawn
        {
            get
            {
                return pawn;
            }
            set
            {
                this.pawn = value;
            }
        }
        
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            // no TempPawn in the def assigned = no special display stats.
            if (TempPawn == null) yield break;

            //Pawn held = TempPawn;

            base.SpecialDisplayStats(req);

            // more or less all energytrackers have a "unit" that is used
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    "ISF_EnergyTrackerUnit".Translate(), energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst(),
                    energyTrackerDef.energyDescKey.Translate(), 99999, null, null, false);

            // depending on energytrackers, alter which ones show up
            Log.Message((SorcerySchemaUtility.FindSorcerySchema(TempPawn, this) == null).ToString());
            foreach(StatDrawEntry entry in SorcerySchemaUtility.FindSorcerySchema(TempPawn, this).energyTracker.SpecialDisplayStats(req))
            {
                yield return entry;
            }

            yield break;
        }

        // don't know if we need this: most of the custom stuff arises from the trackers linked to the schema
        //public Type sorcerySchemaClass = typeof(SorcerySchema);

        //public StatDef energyStat; 

        public EnergyTrackerDef energyTrackerDef;

        public List<LearningTrackerDef> learningTrackerDefs = new List<LearningTrackerDef>();

        public ProgressTrackerDef progressTrackerDef;

        public List<SorcerySchemaDef> incompatibleSchemas;

        private Pawn pawn;

        
    }
}
