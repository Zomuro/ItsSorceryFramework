﻿namespace ItsSorceryFramework
{
    public class EnergyTrackerCompProperties_OnKill : EnergyTrackerCompProperties_DamageBase
    {
        public EnergyTrackerCompProperties_OnKill() => compClass = typeof(EnergyTrackerComp_OnKill);

        public float baseEnergy = 0f;

        // public StatDef scalingStatDef; // present in parent class

        // public List<DamageDef> damageDefs = new List<DamageDef>(); // present in parent class
    }
}
