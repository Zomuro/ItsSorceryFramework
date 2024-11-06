using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTrackerClassLedger
    {
        public string progressTrackerClass = "";
        
        public Dictionary<StatDef, float> detStatOffsetsTotal = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> detStatFactorsTotal = new Dictionary<StatDef, float>();

        public Dictionary<PawnCapacityDef, float> detCapModsTotal = new Dictionary<PawnCapacityDef, float>();

        public Dictionary<HediffDef, float> detHediffModsTotal = new Dictionary<HediffDef, float>();

        public List<AbilityDef> detAbilityTotal = new List<AbilityDef>();

        public Dictionary<StatDef, float> nondetStatOffsetsTotal = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> nondetStatFactorsTotal = new Dictionary<StatDef, float>();

        public Dictionary<PawnCapacityDef, float> nondetCapModsTotal = new Dictionary<PawnCapacityDef, float>();

        public Dictionary<HediffDef, float> nondetHediffModsTotal = new Dictionary<HediffDef, float>();

        public List<AbilityDef> nondetAbilityTotal = new List<AbilityDef>();

        public ProgressTrackerClassLedger(string progressTrackerClass, Dictionary<StatDef, float> detStatOffsetsTotal, Dictionary<StatDef, float> detStatFactorsTotal,
            Dictionary<PawnCapacityDef, float> detCapModsTotal, Dictionary<HediffDef, float> detHediffModsTotal, List<AbilityDef> detAbilityTotal,
            Dictionary<StatDef, float> nondetStatOffsetsTotal, Dictionary<StatDef, float> nondetStatFactorsTotal,
            Dictionary<PawnCapacityDef, float> nondetCapModsTotal, Dictionary<HediffDef, float> nondetHediffModsTotal, List<AbilityDef> nondetAbilityTotal)
        {
            this.progressTrackerClass = progressTrackerClass;
            this.detStatOffsetsTotal = detStatOffsetsTotal;
            this.detStatFactorsTotal = detStatFactorsTotal;
            this.detCapModsTotal = detCapModsTotal;
            this.detHediffModsTotal = detHediffModsTotal;
            this.detAbilityTotal = detAbilityTotal;
            this.nondetStatOffsetsTotal = nondetStatOffsetsTotal;
            this.nondetStatFactorsTotal = nondetStatFactorsTotal;
            this.nondetCapModsTotal = nondetCapModsTotal;
            this.nondetHediffModsTotal = nondetHediffModsTotal;
            this.nondetAbilityTotal = nondetAbilityTotal;
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref progressTrackerClass, "progressTrackerClass", "");
            Scribe_Collections.Look(ref detStatOffsetsTotal, "detStatOffsetsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref detStatFactorsTotal, "detStatFactorsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref detCapModsTotal, "detCapModsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref detHediffModsTotal, "detHediffModsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref detAbilityTotal, "detAbilityTotal", LookMode.Def);
            Scribe_Collections.Look(ref nondetStatOffsetsTotal, "nondetStatOffsetsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref nondetStatFactorsTotal, "nondetStatFactorsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref nondetCapModsTotal, "nondetCapModsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref nondetHediffModsTotal, "nondetHediffModsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref nondetAbilityTotal, "nondetAbilityTotal", LookMode.Def);
        }
    }
}
