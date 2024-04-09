using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace ItsSorceryFramework
{
    public class EnergyTrackerDef : Def
    {
        // EnergyTracker class
        public Type energyTrackerClass = typeof(EnergyTracker);

        // EnergyTracker unit variables
        public StatDef energyUnitStatDef; // doubles as both energy cost AND cast cost

        // EnergyTracker general stats
        public bool inverse = false;

        public StatDef energyMaxStatDef;

        public StatDef energyMinStatDef;

        public StatDef energyAbsMaxStatDef;

        public StatDef energyAbsMinStatDef;

        public StatDef energyCostFactorStatDef;

        public HediffDef sideEffect;

        // EnergyTracker visual components
        public Color emptyBarColor = new Color(0.03f, 0.035f, 0.05f); // HEX #08090D

        public Color underBarColor = new Color(0.6f, 0f, 0f); // HEX #990000 | crimson red

        public Color normalBarColor = new Color(.518f, .427f, .239f); // HEX #846D3D | dull gold

        public Color overBarColor = new Color(0f, .659f, .42f); // HEX #00A86B | jade

        // Translation keys
        public string disableReasonKey;

        public string hitLimitKey = "ISF_MessagePastLimit";

        // Comps
        public List<EnergyTrackerCompProperties> comps;

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            base.SpecialDisplayStats(req);
            yield break;
        }
    }
}
