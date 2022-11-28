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
        public string energyLabelTranslationKey = "EnergyTrackerEnergyLabel_ISF";

        public string EnergyDescTranslationKey = "EnergyStatDesc_ISF";

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
        public string TurnInfoTranslationKey = "EnergyTrackerTurnInfo_ISF";

        public string TurnInfoDescTranslationKey = "EnergyTrackerTurnInfoDesc_ISF";

        public int turnTicks = 60;

        // Vancian EnergyTracker Stats
        public string RefreshNotifTranslationKey = "EnergyTrackerVancianRefresh_ItsSorcery";

        public string RefreshInfoTranslationKey = "EnergyTrackerRefreshInfo_ISF";

        public string RefreshInfoDescTranslationKey = "EnergyTrackerRefreshInfoDesc_ISF";

        public int refreshTicks = 60000;

        //VancianLifetime EnergyTracker string
        public string CastsCountTranslationKey = "EnergyTrackerVancianCastsLeft_ItsSorcery";

        // Command disable string
        public string DisableReasonTranslationKey;

        // Consumable EnerrgyTracker ammo dictionary
        public Dictionary<ThingDef, float> sorceryAmmoDict;

        // Cooldown EnergyTracker string
        public string CooldownTranslationKey = "EnergyTrackerCooldown_ItsSorcery";


    }
}
