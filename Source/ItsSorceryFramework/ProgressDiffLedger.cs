﻿using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressDiffLedger : IExposable
    {
        public int index = 0;

        public int level = 1;

        //public string currClass = "";

        public ProgressTrackerClassDef currClassDef;

        //public Dictionary<string, ProgressDiffClassLedger> classLedgers = new Dictionary<string, ProgressDiffClassLedger>();

        public Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger> classDiffLedgers = new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>();

        public ProgressDiffLedger()
        {
            index = 0;
            level = 1;
            /*currClass = "";
            classLedgers = new Dictionary<string, ProgressDiffClassLedger>();*/

            this.currClassDef = ISF_DefOf.ISF_Generic_Class;
            classDiffLedgers = new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>();
        }

        public ProgressDiffLedger(int index, int level)
        {
            this.index = index;
            this.level = level;
            /*currClass = "";
            classLedgers = new Dictionary<string, ProgressDiffClassLedger>();*/

            this.currClassDef = ISF_DefOf.ISF_Generic_Class;
            classDiffLedgers = new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>();
        }

        public ProgressDiffLedger(int index, int level, string currClass)
        {
            this.index = index;
            this.level = level;
            /*this.currClass = currClass;
            classLedgers = new Dictionary<string, ProgressDiffClassLedger>();*/

            this.currClassDef = ISF_DefOf.ISF_Generic_Class;
            classDiffLedgers = new Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger>();
        }

        // instantiate ledger
        /*public ProgressDiffLedger(int index, int level, string currClass, Dictionary<string, ProgressDiffClassLedger> classLedgers)
        {
            this.index = index;
            this.level = level;
            this.currClass = currClass;
            this.classLedgers = classLedgers;
        }*/

        public ProgressDiffLedger(int index, int level, ProgressTrackerClassDef currClassDef, Dictionary<ProgressTrackerClassDef, ProgressDiffClassLedger> classDiffLedgers)
        {
            this.index = index;
            this.level = level;
            this.currClassDef = currClassDef;
            this.classDiffLedgers = classDiffLedgers;
        }

        /*// use another ledger as a base to create the next ledger entry
        public ProgressDiffLedger(ProgressDiffLedger baseLedger)
        {
            index = baseLedger.index + 1;
            level = baseLedger.level;
            currClass = baseLedger.currClass;
            classLedgers = baseLedger.classLedgers;
        }*/

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref index, "index", 1);
            Scribe_Values.Look(ref level, "level", 1);
            //Scribe_Values.Look(ref currClass, "currClass");
            Scribe_Defs.Look(ref currClassDef, "currClassDef");
            //Scribe_Collections.Look(ref classLedgers, "classLedgers", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref classDiffLedgers, "classDiffLedgers", LookMode.Def, LookMode.Deep);
        }


    }
}
