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
    public class LearningTracker: IExposable
    {
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

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref schemaDef, "schemaDef");
        }

        public virtual void DrawLeftGUI(Rect rect)
        {

        }

        public virtual void DrawRightGUI(Rect rect)
        {

        }


        public Pawn pawn;

        public LearningTrackerDef def;

        public SorcerySchemaDef schemaDef;

    }
}
