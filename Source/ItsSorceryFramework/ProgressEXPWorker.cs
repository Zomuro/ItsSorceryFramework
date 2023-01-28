using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker
    {
        public virtual bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            progressTracker.addExperience(Math.Abs(exp));
            return true;
        }

        public virtual float drawWorker(Rect rect)
        {
            return 0f;
        }

        public IEnumerable<String> labelsFromDef(IEnumerable<Def> defs)
        {
            foreach(var def in defs)
            {
                yield return def.label;
            }
            yield break;
        }

        public virtual void fireEXPMote(Pawn pawn, float exp)
        {
            MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map,
                exp.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset) + " EXP");
        }

        public ProgressEXPTagDef def;

        //public SorcerySchema schema;
    }
}
