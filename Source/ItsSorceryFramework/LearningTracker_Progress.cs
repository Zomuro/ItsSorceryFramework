using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningTracker_Progress : LearningTracker
    {
        public SorcerySchema cachedSchema;

        public List<Sorcery> cachedSorceries;

        public bool cacheDirty = true;

        public LearningTracker_Progress(Pawn pawn) : base(pawn)
        {

        }

        public LearningTracker_Progress(Pawn pawn, LearningTrackerDef def) : base(pawn, def)
        {

        }

        public LearningTracker_Progress(Pawn pawn, LearningTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {

        }

        public LearningTracker_Progress(Pawn pawn, LearningTrackerDef def, SorcerySchema schema) : base(pawn, def, schema)
        {

        }

        public SorcerySchema Schema
        {
            get
            {
                if (cachedSchema == null) cachedSchema = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef);

                return cachedSchema;
            }
        }

        public ProgressTracker ProgressTracker
        {
            get
            {
                return Schema.progressTracker;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void DrawLeftGUI(Rect rect)
        {
            ProgressTracker.DrawLeftGUI(rect);
        }

        public override void DrawRightGUI(Rect rect)
        {
            ProgressTracker.DrawRightGUI(rect);
        }

    }
}
