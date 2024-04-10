using System;
using System.Collections.Generic;

namespace ItsSorceryFramework
{
    public static class EnergyTrackerContext
    {
        public static Dictionary<string, Tuple<SorcerySchema, EnergyTrackerDef, float>> onConsumeContext = new Dictionary<string, Tuple<SorcerySchema, EnergyTrackerDef, float>>();

        public static Tuple<SorcerySchema, EnergyTrackerDef, float> GetConsumeContext(string pawnID)
        {
            return onConsumeContext.TryGetValue(pawnID, out var result)? result : new Tuple<SorcerySchema, EnergyTrackerDef, float>(null, null, 0f);
        }

        public static bool RemoveConsumeContext(string pawnID) => onConsumeContext.Remove(pawnID);
    }
}
