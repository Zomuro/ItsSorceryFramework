using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTrackerLedger : IExposable
    {
        public int index = 0;

        public int level = 1;

        public string currClass = "";

        public Dictionary<string, ProgressTrackerClassLedger> classLedgers = new Dictionary<string, ProgressTrackerClassLedger>();

        public ProgressTrackerLedger()
        {
            index = 0;
            level = 1;
            currClass = "";
            classLedgers = new Dictionary<string, ProgressTrackerClassLedger>();
        }

        // instantiate ledger
        public ProgressTrackerLedger(int index, int level, string currClass, Dictionary<string, ProgressTrackerClassLedger> classLedgers)
        {
            this.index = index;
            this.level = level;
            this.currClass = currClass;
            this.classLedgers = classLedgers;
        }

        // use another ledger as a base to create the next ledger entry
        public ProgressTrackerLedger(ProgressTrackerLedger baseLedger)
        {
            index = baseLedger.index + 1;
            level = baseLedger.level;
            currClass = baseLedger.currClass;
            classLedgers = baseLedger.classLedgers;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref index, "index", 1);
            Scribe_Values.Look(ref level, "level", 1);
            Scribe_Values.Look(ref currClass, "currClass");
            Scribe_Collections.Look(ref classLedgers, "classLedgers", LookMode.Value, LookMode.Deep);
        }

    }
}
