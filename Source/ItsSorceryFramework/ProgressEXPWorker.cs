using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker
    {
        public ProgressEXPTagDef def;        

        public int lastStatCacheTick = -1;
        
        public StatDef ScalingStatDef => def.scalingStatDef is null ? StatDefOf_ItsSorcery.ISF_ScalingStat : def.scalingStatDef;

        public virtual float ScalingStatValue(Pawn pawn) => PawnCacheUtility.GetStatCacheVal(pawn, ScalingStatDef);

        public virtual bool TryExecute(ProgressTracker progressTracker, float inputAmt = 0)
        {           
            if (progressTracker.Maxed) return false;
            progressTracker.AddExperience(Math.Abs(inputAmt));
            return true;
        }

        public virtual float DrawWorker(Pawn pawn, Rect rect)
        {
            return 0f;
        }

        public IEnumerable<String> LabelsFromDef(IEnumerable<Def> defs)
        {
            foreach(var def in defs)
            {
                yield return def.label;
            }
            yield break;
        }

        public virtual void FireEXPMote(Pawn pawn, float exp)
        {
            MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map,
                exp.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset) + " EXP");
        }
    }
}
