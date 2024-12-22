using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressDiffClassLedger
    {
        //public string progressTrackerClass = "";

        public Dictionary<StatDef, float> statOffsetsTotal = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> statFactorsTotal = new Dictionary<StatDef, float>();

        public Dictionary<PawnCapacityDef, float> capModsTotal = new Dictionary<PawnCapacityDef, float>();

        public Dictionary<HediffDef, float> hediffModsTotal = new Dictionary<HediffDef, float>();

        public Dictionary<AbilityDef, int> abilityTotal = new Dictionary<AbilityDef, int>();

        public ProgressDiffClassLedger()
        {
            //progressTrackerClass = "";
            statOffsetsTotal = new Dictionary<StatDef, float>();
            statFactorsTotal = new Dictionary<StatDef, float>();
            capModsTotal = new Dictionary<PawnCapacityDef, float>();
            hediffModsTotal = new Dictionary<HediffDef, float>();
            abilityTotal = new Dictionary<AbilityDef, int>();
        }

        /*public ProgressDiffClassLedger(string progressTrackerClass)
        {
            //this.progressTrackerClass = progressTrackerClass;
            statOffsetsTotal = new Dictionary<StatDef, float>();
            statFactorsTotal = new Dictionary<StatDef, float>();
            capModsTotal = new Dictionary<PawnCapacityDef, float>();
            hediffModsTotal = new Dictionary<HediffDef, float>();
            abilityTotal = new Dictionary<AbilityDef, int>();
        }*/

        // string progressTrackerClass
        public ProgressDiffClassLedger(Dictionary<StatDef, float> statOffsetsTotal, Dictionary<StatDef, float> statFactorsTotal,
            Dictionary<PawnCapacityDef, float> capModsTotal, Dictionary<HediffDef, float> hediffModsTotal, Dictionary<AbilityDef, int> abilityTotal)
        {
            //this.progressTrackerClass = progressTrackerClass;
            this.statOffsetsTotal = statOffsetsTotal;
            this.statFactorsTotal = statFactorsTotal;
            this.capModsTotal = capModsTotal;
            this.hediffModsTotal = hediffModsTotal;
            this.abilityTotal = abilityTotal;
        }

        public virtual void ExposeData()
        {
            //Scribe_Values.Look(ref progressTrackerClass, "progressTrackerClass", "");
            Scribe_Collections.Look(ref statOffsetsTotal, "statOffsetsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref statFactorsTotal, "statFactorsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref capModsTotal, "capModsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref hediffModsTotal, "hediffModsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref abilityTotal, "abilityTotal", LookMode.Def, LookMode.Value);
        }

    }
}
