﻿using System;
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

        public List<ProgressLevelModifier> levelFactors = new List<ProgressLevelModifier>();

        public List<ProgressLevelModifier> levelSpecifics = new List<ProgressLevelModifier>();

        public List<ProgressTrackerCompProperties> progressComps = new List<ProgressTrackerCompProperties>();

        public string progressLevelUpTransKey = "levelup";

        public string progressLevelUpDescTransKey = "levelup";
    }

    public class ProgressTrackerCompProperties : CompProperties
    {

    }



}
