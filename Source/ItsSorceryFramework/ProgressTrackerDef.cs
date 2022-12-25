using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTrackerDef : Def
    {
        public float maxLevel 
        {
            get 
            {
                return progressHediff?.maxSeverity ?? 0f;
            }
        }

        public IEnumerable<StatDrawEntry> specialDisplayMods(ProgressLevelModifier levelMod)
        {
            if (levelMod == null) yield break;

            if (!levelMod.capMods.NullOrEmpty())
            {
                foreach (PawnCapacityModifier capMod in levelMod.capMods)
                {
                    if (capMod.offset != 0f)
                    {
                        yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
                            capMod.capacity.GetLabelFor(true, true).CapitalizeFirst(),
                            (capMod.offset * 100f).ToString("+#;-#") + "%",
                            capMod.capacity.description, 4060, null, null, false);
                    }
                }
            }

            if (!levelMod.statOffsets.NullOrEmpty())
            {
                foreach (StatModifier statMod in levelMod.statOffsets)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
                        statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Offset),
                        statMod.stat.description, 4070, null, null, false);
                }
            }

            if (!levelMod.statFactorOffsets.NullOrEmpty())
            {
                foreach (StatModifier statMod in levelMod.statFactorOffsets)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects,
                        statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value+1, false, ToStringNumberSense.Factor),
                        statMod.stat.description, 4070, null, null, false);
                }
            }

            yield break;
        }

        public ProgressLevelModifier getLevelFactor(float severity) 
		{
            if (levelFactors.NullOrEmpty()) return null;

            foreach (ProgressLevelModifier factor in levelFactors.OrderByDescending(x => x.level))
            {
                // if the level devided by the modulo leaves a remainder of 0
                if (severity % factor.level == 0)
                {
                    return factor;
                }
            }
            return null;
		}

        public ProgressLevelModifier getLevelSpecific(float severity)
        {
            if (levelSpecifics.NullOrEmpty()) return null;

            foreach (ProgressLevelModifier factor in levelSpecifics.OrderByDescending(x => x.level))
            {
                // if the level devided by the modulo leaves a remainder of 0
                if (severity == factor.level)
                {
                    return factor;
                }
            }
            return null;
        }

        public Type progressTrackerClass = typeof(ProgressTracker);

        public HediffDef progressHediff;

        public float baseEXP = 100f;

        public float scaling = 1.1f;

        public float maxEXP = 1000f;

        public List<ProgressLevelModifier> levelFactors = new List<ProgressLevelModifier>();

        public List<ProgressLevelModifier> levelSpecifics = new List<ProgressLevelModifier>();

        public List<ProgressTrackerCompProperties> progressComps = new List<ProgressTrackerCompProperties>();

        public string progressLevelUpTransKey = "levelup";

        public string progressLevelUpDescTransKey = "levelup";
    }

    public class ProgressTrackerCompProperties : CompProperties
    {

    }



}
