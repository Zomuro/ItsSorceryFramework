using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTrackerDef : Def
    {
        public float maxLevel 
        {
            get 
            {
                return progressHediff?.maxSeverity ?? 0f;
            }
        }
      
        public HediffDef progressHediff;

        public float baseEXP = 100f;

        public float scaling = 1.1f;

        public float maxEXP = 1000f;

        public List<ProgressLevelProperties> moduloStatGain = new List<ProgressLevelProperties>();

        public List<ProgressTrackerCompProperties> progressComps = new List<ProgressTrackerCompProperties>();
    }

    public class ProgressLevelProperties
    {
        public int levelFactor = 1;

        public List<StatModifier> statOffsets;

        public List<StatModifier> statFactors;
    }

    public class ProgressTrackerCompProperties : CompProperties
    {

    }



}
