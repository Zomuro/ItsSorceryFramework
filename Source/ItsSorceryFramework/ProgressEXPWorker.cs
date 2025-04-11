using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker
    {
        public virtual bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {           
            if (progressTracker.Maxed) return false;
            progressTracker.AddExperience(Math.Abs(exp));
            return true;
        }

        public virtual float DrawWorker(Rect rect)
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

        public ProgressEXPTagDef def;

        //public SorcerySchema schema;
    }
}
