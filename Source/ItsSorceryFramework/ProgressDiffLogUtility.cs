using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public static class ProgressDiffLogUtility
    {
        public static Dictionary<K, float> DiffDictSum<K, V>(this Dictionary<K, float> baseDict, Dictionary<K,float> dict)
        {
            foreach (var i in dict)
            {
                if (baseDict.ContainsKey(i.Key)) baseDict[i.Key] += i.Value;
                else baseDict[i.Key] = i.Value;
            }

            return baseDict;
        }

        public static Dictionary<K, float> DiffDictSum<K, V>(Dictionary<K, float> dict)
        {
            Dictionary<K, float> returnDict = new Dictionary<K, float>() { };
            foreach (var i in dict)
            {
                if (returnDict.ContainsKey(i.Key)) returnDict[i.Key] += i.Value;
                else returnDict[i.Key] = i.Value;
            }

            return returnDict;
        }

        public static Dictionary<K, int> DiffDictSum<K, V>(this Dictionary<K, int> baseDict, Dictionary<K, int> dict)
        {
            foreach (var i in dict)
            {
                if (baseDict.ContainsKey(i.Key)) baseDict[i.Key] += i.Value;
                else baseDict[i.Key] = i.Value;
            }

            return baseDict;
        }

        public static Dictionary<K, int> DiffDictSum<K, V>(Dictionary<K, int> dict)
        {
            Dictionary<K, int> returnDict = new Dictionary<K, int>() { };
            foreach (var i in dict)
            {
                if (returnDict.ContainsKey(i.Key)) returnDict[i.Key] += i.Value;
                else returnDict[i.Key] = i.Value;
            }

            return returnDict;
        }

        public static string DebugDictLog<K, V>(Dictionary<K, V> baseDict)
        {
            return string.Join("\n", baseDict);
        }

        // consider incorporating this into the adjust method itself
        public static Dictionary<StatDef, float> ListToDiffDict(List<StatModifier> statMods)
        {
            Dictionary<StatDef, float> returnDict = new Dictionary<StatDef, float>() { };

            if (statMods.NullOrEmpty())
            {
                Log.Message("Stat mods found empty somehow.");
                return returnDict;
            }
            foreach (var s in statMods)
            {
                if (returnDict.ContainsKey(s.stat)) returnDict[s.stat] += s.value;
                else returnDict[s.stat] = s.value;
            }

            Log.Message(ProgressDiffLogUtility.DebugDictLog(returnDict));

            return returnDict;
        }

        // consider incorporating this into the adjust method itself
        public static Dictionary<PawnCapacityDef, float> ListToDiffDict(List<PawnCapacityModifier> capMods)
        {
            Dictionary<PawnCapacityDef, float> returnDict = new Dictionary<PawnCapacityDef, float>() { };
            
            if (capMods.NullOrEmpty())
            {
                Log.Message("Cap mods found empty somehow.");
                return returnDict;
            }
            foreach (var c in capMods)
            {
                if (returnDict.ContainsKey(c.capacity)) returnDict[c.capacity] += c.offset;
                else returnDict[c.capacity] = c.offset;
            }
            Log.Message(ProgressDiffLogUtility.DebugDictLog(returnDict));

            return returnDict;
        }

        // consider incorporating this into the adjust method itself
        public static Dictionary<AbilityDef, int> ListToDiffDict(List<AbilityDef> abilityGain, List<AbilityDef> abilityRemove)
        {
            Dictionary<AbilityDef, int> returnDict = new Dictionary<AbilityDef, int>() { };
            
            if (!abilityGain.NullOrEmpty())
            {
                foreach (var g in abilityGain) returnDict[g] = 1; // not perfect - increments when an ability already is added
            }

            if (!abilityRemove.NullOrEmpty())
            {
                foreach (var r in abilityRemove) returnDict[r] = -1; // not perfect - deincrements when an ability already is removed
            }

            return returnDict;
        }

        // consider incorporating this into the adjust method itself
        public static Dictionary<HediffDef, float> ListToDiffDict(Pawn pawn, List<NodeHediffProps> hediffAdd, List<NodeHediffProps> hediffAdjust, List<HediffDef> hediffRemove)
        {
            Dictionary<HediffDef, float> returnDict = new Dictionary<HediffDef, float>() { };

            if (!hediffAdd.NullOrEmpty())
            {
                foreach (NodeHediffProps props in hediffAdd)
                {
                    if (returnDict.ContainsKey(props.hediffDef)) returnDict[props.hediffDef] += props.severity;
                    else returnDict[props.hediffDef] = props.severity;
                }
            }

            if (!hediffAdjust.NullOrEmpty())
            {
                foreach (NodeHediffProps props in hediffAdjust)
                {
                    //HealthUtility.AdjustSeverity(pawn, props.hediffDef, props.severity);
                    if (returnDict.ContainsKey(props.hediffDef)) returnDict[props.hediffDef] += props.severity;
                    else returnDict[props.hediffDef] = props.severity;
                }
            }

            if (!hediffRemove.NullOrEmpty())
            {
                Hediff hediff;
                foreach (HediffDef hediffDef in hediffRemove)
                {
                    hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (hediff is null) returnDict[hediffDef] = 0;
                    if (returnDict.ContainsKey(hediffDef)) returnDict[hediffDef] += 0;
                    else returnDict[hediffDef] = -hediff.Severity;
                }
            }
            

            return returnDict;
        }

        public static Dictionary<K, float> DiffDictClean<K, V>(this Dictionary<K, float> baseDict)
        {
            var returnDict = new Dictionary<K, float>();

            if (baseDict.NullOrEmpty()) return returnDict;
            foreach (var i in baseDict)
            {
                if (i.Value == 0) continue;
                returnDict[i.Key] = i.Value;
            }

            return returnDict;
        }




    }
}
