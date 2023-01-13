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
    public abstract class ProgressComp
    {
        public virtual void Initialize(ProgressCompProperties props)
        {
            this.props = props;
        }

        public virtual bool showCompSummary
        {
            get
            {
                return false;
            }
        }

        public virtual void PostExposeData()
        {
        }

        public virtual float CompDisplayUI(Rect rect) // used by progress
        {
            return 0f;
        }

        public virtual void CompTick()
        {
        }

        public ProgressTracker parent;

        public ProgressCompProperties props;

    }
}
