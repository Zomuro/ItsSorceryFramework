using RimWorld;

namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_PerTick : EnergyTrackerCompProperties
    {
        public EnergyTrackerCompProperties_PerTick() => compClass = typeof(EnergyTrackerComp_PerTick);

        // Energy related stats and recovery information.
        public StatDef energyRecoveryStatDef;

        public float overchargeRecoveryFactor = -0.5f;

        public float deficitRecoveryFactor = 0.5f;
    }
}
