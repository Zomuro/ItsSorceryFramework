using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_OnDamage : EnergyTrackerCompProperties
    {
        public EnergyTrackerCompProperties_OnDamage() => compClass = typeof(EnergyTrackerComp_OnDamage);

        // Energy related stats and recovery information.
        public StatDef energyRecoveryStatDef;

        public float overchargeRecoveryFactor = -0.5f;

        public float deficitRecoveryFactor = 0.5f;

        public List<DamageDef> damageDefs = new List<DamageDef>();

        // Translated string keys
        /*public string overchargeLabelKey = "ISF_EnergyTrackerOverchargeLabel";

        public string overchargeDescKey = "ISF_EnergyTrackerOverchargeDesc";

        public string deficitLabelKey = "ISF_EnergyTrackerDeficitLabel";

        public string deficitDescKey = "ISF_EnergyTrackerDeficitDesc";*/



        //public StatDef RecoveryRateStatDef => energyRecoveryStatDef ?? StatDefOf_ItsSorcery.EnergyRecovery_ItsSorcery;

        //public bool continuous = true;
    }
}
