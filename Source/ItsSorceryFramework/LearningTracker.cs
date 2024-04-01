using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningTracker: IExposable
    {
        public Pawn pawn;

        public LearningTrackerDef def;

        public SorcerySchemaDef schemaDef;

        public SorcerySchema schema;

        public LearningTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public LearningTracker(Pawn pawn, LearningTrackerDef def)
        {
            this.pawn = pawn;
            this.def = def;
        }

        public LearningTracker(Pawn pawn, LearningTrackerDef def, SorcerySchemaDef schemaDef)
        {
            this.pawn = pawn;
            this.def = def;
            this.schemaDef = schemaDef;
        }

        public LearningTracker(Pawn pawn, LearningTrackerDef def, SorcerySchema schema)
        {
            this.pawn = pawn;
            this.def = def;
            this.schema = schema;
            this.schemaDef = schema.def;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref schemaDef, "schemaDef");
            Scribe_References.Look(ref schema, "schema");
        }

        public virtual void DrawLeftGUI(Rect rect)
        {

        }

        public virtual void DrawRightGUI(Rect rect)
        {

        }
    }
}
