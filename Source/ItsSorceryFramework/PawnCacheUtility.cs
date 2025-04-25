using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public static class PawnCacheUtility
    {
        // PawnCacheUtility is used for general caching of stat values on-demand.
        // However, for performance critical applications (stuff that regularly ticks or updates during normal gameplay), avoid using this.
        // It is better to cache the value itself in class so it doesn't need to spend time finding and retrieving the value here.

        public static Dictionary<Pawn, PawnStatCacheDict> statCacheDict = new Dictionary<Pawn, PawnStatCacheDict>();

        public static void ClearCacheDicts()
        {
            statCacheDict = new Dictionary<Pawn, PawnStatCacheDict>();
        }

        public static PawnStatCacheDict GetStatCacheDict(Pawn pawn)
        {
            PawnStatCacheDict pawnStatCacheDict;
            if (!statCacheDict.TryGetValue(pawn, out pawnStatCacheDict))
            {
                pawnStatCacheDict = new PawnStatCacheDict(pawn);
                statCacheDict[pawn] = pawnStatCacheDict;
                return statCacheDict[pawn];
            }

            return pawnStatCacheDict;
        }

        public static float GetStatCacheVal(Pawn pawn, StatDef statDef) => GetStatCacheDict(pawn).GetStatValue(statDef);

        public static int GetGeneralTickOffset()
        {
            int minTickOffset = Math.Max(1, ItsSorceryUtility.settings.GeneralStatCacheTicks - 3);
            int maxTickOffset = Math.Max(1, ItsSorceryUtility.settings.GeneralStatCacheTicks + 3);
            return UnityEngine.Random.Range(minTickOffset, maxTickOffset);
        }

        public static int GetEnergyTickOffset()
        {
            int minTickOffset = Math.Max(1, ItsSorceryUtility.settings.EnergyStatCacheTicks - 3);
            int maxTickOffset = Math.Max(1, ItsSorceryUtility.settings.EnergyStatCacheTicks + 3);
            return UnityEngine.Random.Range(minTickOffset, maxTickOffset);
        }
    }

    public abstract class PawnCacheDict
    {
        public Pawn pawn;

        public PawnCacheDict(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public virtual void ClearCacheDict(){ }
    }

    public class PawnStatCacheDict : PawnCacheDict
    {
        public Dictionary<StatDef, PawnStatCache> statCacheDict = new Dictionary<StatDef, PawnStatCache>();

        public PawnStatCacheDict(Pawn pawn) : base(pawn) { }

        public override void ClearCacheDict() => statCacheDict = new Dictionary<StatDef, PawnStatCache>();

        public float GetStatValue(StatDef statDef)
        {
            PawnStatCache pawnStatCache;
            if (!statCacheDict.TryGetValue(statDef, out pawnStatCache))
            {
                pawnStatCache = new PawnStatCache(pawn, statDef);
                pawnStatCache.Reset();
                statCacheDict[statDef] = pawnStatCache;
                return pawnStatCache.statVal;
            }

            if (pawnStatCache.Stale) pawnStatCache.Reset();
            return pawnStatCache.statVal;
        }

        public void ForceResetStatValue(StatDef statDef)
        {
            PawnStatCache pawnStatCache;
            if (!statCacheDict.TryGetValue(statDef, out pawnStatCache))
            {
                pawnStatCache = new PawnStatCache(pawn, statDef);
                pawnStatCache.Reset();
                statCacheDict[statDef] = pawnStatCache;
                return;
            }

            pawnStatCache.Reset();
        }
    }

    public abstract class PawnCache
    {
        public Pawn pawn;

        public bool Stale => Find.TickManager.TicksGame > recacheTick;

        public int recacheTick = -1;

        public PawnCache(Pawn pawn) 
        {
            this.pawn = pawn;
        }

        public virtual int GetTickOffset()
        {
            int minTickOffset = Math.Max(1, ItsSorceryUtility.settings.GeneralStatCacheTicks - 3);
            int maxTickOffset = Math.Max(1, ItsSorceryUtility.settings.GeneralStatCacheTicks + 3);
            return UnityEngine.Random.Range(minTickOffset, maxTickOffset);
        }

        public virtual void Reset() { }
    }


    public class PawnStatCache : PawnCache
    {
        public StatDef statDef;
        
        public float statVal = 0f;

        public PawnStatCache(Pawn pawn, StatDef statDef) : base(pawn) 
        {
            this.statDef = statDef;
        }

        public override void Reset() 
        {
            // calculate somewhat randomized tick offset
            int tickOffset = GetTickOffset();

            // determine tick to recache & set stat value
            recacheTick = Find.TickManager.TicksGame + tickOffset;
            float priorStatVal = statVal;
            statVal = pawn.GetStatValue(statDef);

            if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug) Log.Message($"[It's Sorcery!] PawnCacheUtility: {pawn.Name} {statDef.label} ({priorStatVal} => {statVal}) cached till game tick {recacheTick} for {tickOffset} ticks.");
        }
    }
}
