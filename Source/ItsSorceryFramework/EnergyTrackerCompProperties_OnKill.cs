namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_OnKill : EnergyTrackerCompProperties_DamageBase
    {
        public EnergyTrackerCompProperties_OnKill() => compClass = typeof(EnergyTrackerComp_OnKill);

        public float baseEnergy = 0f;
    }
}
