using RimWorld;

namespace ItsSorceryFramework
{
    [DefOf]
    public static class ISF_DefOf
    {
        static ISF_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ISF_DefOf));
        }

        public static ProgressTrackerClassDef ISF_Generic_Class;

    }
}
