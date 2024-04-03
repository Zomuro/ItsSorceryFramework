using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItsSorceryFramework
{
    public static class ProgressTrackerContext
    {
        public static Dictionary<string, Tuple<SorcerySchema, float>> onConsumeContext = new Dictionary<string, Tuple<SorcerySchema, float>>();

        public static Tuple<SorcerySchema, float> GetConsumeContext(string pawnID)
        {
            return onConsumeContext.TryGetValue(pawnID, out var result)? result : new Tuple<SorcerySchema, float>(null, 0f);
        }

        public static bool RemoveConsumeContext(string pawnID) => onConsumeContext.Remove(pawnID);
    }
}
