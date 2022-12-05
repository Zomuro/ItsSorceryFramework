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

        public Type progressTrackerClass = typeof(ProgressTracker);

        public HediffDef progressHediff;

        public float baseEXP = 100f;

        public float scaling = 1.1f;

        public float maxEXP = 1000f;

        public List<ProgressLevelModulo> levelModulos = new List<ProgressLevelModulo>();

        public List<ProgressTrackerCompProperties> progressComps = new List<ProgressTrackerCompProperties>();

        public string progressLevelUpTransKey = "levelup";

        public string progressLevelUpDescTransKey = "levelup";
    }

    public class ProgressLevelModulo
    {
        public int levelFactor = 1;

        public List<StatModifier> statOffsets;

        public List<StatModifier> statFactors;

        public List<PawnCapacityModifier> capMods = new List<PawnCapacityModifier>();

        public int pointGain = 1;
    }

    public class ProgressTrackerCompProperties : CompProperties
    {

    }



}
