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
        public virtual bool TryExecute(ProgressTracker progressTracker, float exp)
        {
            return true;
        }

        public virtual float drawWorker(Rect rect)
        {
            return 0f;
        }

        public ProgressEXPDef def;

        public SorcerySchema schema;
    }
}
