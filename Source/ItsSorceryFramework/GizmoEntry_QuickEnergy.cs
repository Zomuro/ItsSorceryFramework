using Verse;

namespace ItsSorceryFramework
{
    public class GizmoEntry_QuickEnergy : IExposable
    {
        public SorcerySchemaDef sorcerySchemaDef;

        public EnergyTrackerDef energyTrackerDef;

        public GizmoEntry_QuickEnergy() { }

        public GizmoEntry_QuickEnergy(SorcerySchemaDef sorcerySchemaDef, EnergyTrackerDef energyTrackerDef)
        {
            this.sorcerySchemaDef = sorcerySchemaDef;
            this.energyTrackerDef = energyTrackerDef;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref sorcerySchemaDef, "sorcerySchemaDef");
            Scribe_Defs.Look(ref energyTrackerDef, "energyTrackerDef");
        }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj) => EqualityHelper(obj as GizmoEntry_QuickEnergy);

        public bool EqualityHelper(GizmoEntry_QuickEnergy entry)
        {
            if (entry is null) return false;
            if (ReferenceEquals(this, entry)) return true;

            return (sorcerySchemaDef == entry.sorcerySchemaDef &&
                energyTrackerDef == entry.energyTrackerDef);
        }
    }
}
