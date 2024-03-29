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
        [MustTranslate]
        public string energyLabelKey = "ISF_EnergyTrackerUnitLabel";

        [MustTranslate]
        public string energyDescKey = "ISF_EnergyStatDesc";

        public StatDef energyUnitStatDef; // doubles as both energy cost AND cast cost

        public StatDef energyMaxCastStatDef; // define the stat the energytracker uses to figure out max casts

        // EnergyTracker general stats
        public bool inverse = false;

        public StatDef energyMaxStatDef;

        public StatDef energyMinStatDef;

        public StatDef energyAbsMaxStatDef;

        public StatDef energyAbsMinStatDef;

        public StatDef energyRecoveryStatDef;

        public StatDef energyCostFactorStatDef;

        public StatDef castFactorStatDef;

        public float overBarRecoveryFactor = 1f;

        public float underBarRecoveryFactor = 1f;

        public HediffDef sideEffect;

        // EnergyTracker visual components
        public Color emptyBarColor = new Color(0.03f, 0.035f, 0.05f); // HEX #08090D

        public Color underBarColor = new Color(0.6f, 0f, 0f); // HEX #990000 | crimson red

        public Color normalBarColor = new Color(.518f, .427f, .239f); // HEX #846D3D | dull gold

        public Color overBarColor = new Color(0f, .659f, .42f); // HEX #00A86B | jade





        // TurnBased EnergyTrackers
        [MustTranslate]
        public string turnInfoKey = "ISF_EnergyTrackerTurnInfo";

        [MustTranslate]
        public string turnInfoDescKey = "ISF_EnergyTrackerTurnInfoDesc";

        public int turnTicks = 60;

        // Vancian EnergyTracker Stats
        [MustTranslate]
        public string refreshNotifKey = "ISF_EnergyTrackerVancianRefresh";

        [MustTranslate]
        public string refreshInfoKey = "ISF_EnergyTrackerRefreshInfo";

        [MustTranslate]
        public string refreshInfoDescKey = "ISF_EnergyTrackerRefreshInfoDesc";

        public int refreshTicks = 60000;

        //VancianLifetime EnergyTracker string
        [MustTranslate]
        public string castCountKey = "ISF_EnergyTrackerVancianCastsLeft";

        // Command disable string
        public string disableReasonKey;

        // Consumable EnerrgyTracker ammo dictionary
        //public List<EnergyConsumable> consumables;

        // Cooldown EnergyTracker string
        [MustTranslate]
        public string cooldownKey = "ISF_EnergyTrackerCooldown";

        public List<EnergyTrackerCompProperties> comps;

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            base.SpecialDisplayStats(req);
            yield break;
        }
    }

    /*public class EnergyConsumable
    {
        public ThingDef thingDef;

        public float energy = 1f;
    }*/
}
