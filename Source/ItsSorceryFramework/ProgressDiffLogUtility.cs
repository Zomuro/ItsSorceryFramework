using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItsSorceryFramework
{
    public static class ProgressDiffLogUtility
    {
        public static Dictionary<K, float> DiffDictSum<K, V>(this Dictionary<K, float> baseDict, Dictionary<K,float> dict)
        {
            foreach (var i in dict)
            {
                if (baseDict.ContainsKey(i.Key)) baseDict[i.Key] += i.Value;
                baseDict[i.Key] = i.Value;
            }

            return baseDict;
        }

        public static Dictionary<K, int> DiffDictSum<K, V>(this Dictionary<K, int> baseDict, Dictionary<K, int> dict)
        {
            foreach (var i in dict)
            {
                if (baseDict.ContainsKey(i.Key)) baseDict[i.Key] += i.Value;
                baseDict[i.Key] = i.Value;
            }

            return baseDict;
        }

        public static Dictionary<K, float> DiffDictClean<K, V>(this Dictionary<K, float> baseDict)
        {
            var returnDict = new Dictionary<K, float>();
            foreach (var i in baseDict)
            {
                if (i.Value == 0) continue;
                returnDict[i.Key] = i.Value;
            }

            return returnDict;
        }


    }
}
