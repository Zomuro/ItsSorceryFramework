using Verse;

namespace ItsSorceryFramework
{
    public class SchemaJobDef : JobDef
    {
        public SorcerySchema schema;

        public SorcerySchemaDef schemaDef;

        public EnergyTrackerDef energyTrackerDef;

        public float energyPerTarget = 0f;
    }
}
