using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerDef : Def
    {
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            base.SpecialDisplayStats(req);
            yield break;
        }

        // EnergyTracker class
        public Type energyTrackerClass = typeof(EnergyTracker);

        // EnergyTracker unit variables
        [MustTranslate]
        public string energyLabelKey = "ISF_EnergyTrackerUnitLabel";

        [MustTranslate]
        public string energyDescKey = "ISF_EnergyStatDesc";

        // EnergyTracker general stats
        public StatDef energyMaxStatDef;

        public StatDef energyMinStatDef;

        public StatDef energyOverMaxStatDef;

        public StatDef energyRecoveryStatDef;

        public StatDef energyCostFactorStatDef;

        public StatDef castFactorStatDef;

        public float overBarRecoveryFactor = 1f;

        public float underBarRecoveryFactor = 1f;

        public HediffDef sideEffect;

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
        public Dictionary<ThingDef, float> sorceryAmmoDict;

        // Cooldown EnergyTracker string
        [MustTranslate]
        public string cooldownKey = "ISF_EnergyTrackerCooldown";


    }
}
