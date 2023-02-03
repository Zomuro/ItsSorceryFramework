using RimWorld;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class SchemaJobDefOf
    {
        public static SchemaJobDef ChargeSchema;
        public static SchemaJobDef GainEXPSchema;

        static SchemaJobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SchemaJobDefOf));
        }
    }
}
