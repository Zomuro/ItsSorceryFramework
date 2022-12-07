using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressLevelModulo
    {
        public int levelFactor = 1;

        public List<StatModifier> statOffsets;

        public List<StatModifier> statFactors;

        public List<PawnCapacityModifier> capMods = new List<PawnCapacityModifier>();

        public int pointGain = 1;

        public List<ProgressLevelOption> options;
    }
}
