using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningTracker: IExposable
    {
        public Pawn pawn;

        public LearningTrackerDef def;

        public SorcerySchema schema;

        public LearningTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public LearningTracker(Pawn pawn, LearningTrackerDef def, SorcerySchema schema)
        {
            this.pawn = pawn;
            this.def = def;
            this.schema = schema;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref schema, "schema");
        }

        public virtual void DrawLeftGUI(Rect rect) { }

        public virtual void DrawRightGUI(Rect rect) { }
    }
}
