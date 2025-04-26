using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressDiffClassLedger : IExposable
    {
        public Dictionary<StatDef, float> statOffsetsTotal = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> statFactorsTotal = new Dictionary<StatDef, float>();

        public Dictionary<PawnCapacityDef, float> capModsTotal = new Dictionary<PawnCapacityDef, float>();

        public Dictionary<HediffDef, float> hediffModsTotal = new Dictionary<HediffDef, float>();

        public Dictionary<AbilityDef, int> abilityTotal = new Dictionary<AbilityDef, int>();

        public ProgressDiffClassLedger()
        {
            statOffsetsTotal = new Dictionary<StatDef, float>();
            statFactorsTotal = new Dictionary<StatDef, float>();
            capModsTotal = new Dictionary<PawnCapacityDef, float>();
            hediffModsTotal = new Dictionary<HediffDef, float>();
            abilityTotal = new Dictionary<AbilityDef, int>();
        }

        public ProgressDiffClassLedger(Dictionary<StatDef, float> statOffsetsTotal, Dictionary<StatDef, float> statFactorsTotal,
            Dictionary<PawnCapacityDef, float> capModsTotal, Dictionary<HediffDef, float> hediffModsTotal, Dictionary<AbilityDef, int> abilityTotal)
        {
            this.statOffsetsTotal = statOffsetsTotal;
            this.statFactorsTotal = statFactorsTotal;
            this.capModsTotal = capModsTotal;
            this.hediffModsTotal = hediffModsTotal;
            this.abilityTotal = abilityTotal;
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref statOffsetsTotal, "statOffsetsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref statFactorsTotal, "statFactorsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref capModsTotal, "capModsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref hediffModsTotal, "hediffModsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref abilityTotal, "abilityTotal", LookMode.Def, LookMode.Value);
        }

        public override string ToString()
        {
            return $"statOffsetsTotal = {{{ProgressDiffLogUtility.DebugDictLog(statOffsetsTotal)}}}" +
                $"\nstatFactorsTotal = {{{ProgressDiffLogUtility.DebugDictLog(statFactorsTotal)}}}" +
                $"\ncapModsTotal = {{{ProgressDiffLogUtility.DebugDictLog(capModsTotal)}}}" +
                $"\nhediffModsTotal = {{{ProgressDiffLogUtility.DebugDictLog(hediffModsTotal)}}}" +
                $"\nabilityTotal = {{{ProgressDiffLogUtility.DebugDictLog(abilityTotal)}}}";
        }

    }
}
