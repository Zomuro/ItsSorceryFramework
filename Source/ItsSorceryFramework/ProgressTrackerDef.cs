﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTrackerDef : Def
    {
        public Type progressTrackerClass = typeof(ProgressTracker);

        public string bgPath;

        private Texture2D trackerBG;

        public HediffDef progressHediff;

        public IntRange levelRange = new IntRange(1, 10);

        public float baseEXP = 100f;

        public float scaling = 1.1f;

        //public float maxEXP = 1000f;

        public List<ProgressLevelModifier> levelFactors = new List<ProgressLevelModifier>();

        public List<ProgressLevelModifier> levelSpecifics = new List<ProgressLevelModifier>();

        public List<ProgressEXPTagDef> expTags = new List<ProgressEXPTagDef>();

        public List<ProgressLevelLabel> levelLabels = new List<ProgressLevelLabel>();

        [MustTranslate]
        public string progressLevelUpKey = "ISF_LevelUp";

        [MustTranslate]
        public string progressLevelUpDescKey = "ISF_LevelUpMessage";

        [MustTranslate]
        public string skillPointLabelKey = "ISF_SkillPointLabel";

        private HashSet<ProgressEXPWorker> cachedEXPWorkers = new HashSet<ProgressEXPWorker>();

        public float maxLevel 
        {
            get 
            {
                return progressHediff?.maxSeverity ?? 0f;
            }
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            bool error = false;

            int hediffCheckCount =  SorcerySchemaUtility.AllProgressTrackerDefs.Where(x => x.progressHediff == progressHediff).Count();

            if (hediffCheckCount > 1)
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} should not have the same hediff as other ProgressTrackerDefs.");
            }

            if (error) Log.Error("The ProgressTrackerDef " + defName + " has errors.");
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
                            capMod.capacity.GetLabelFor().CapitalizeFirst(),
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

        public HashSet<ProgressEXPWorker> Workers
        {
            get
            {
                if (cachedEXPWorkers.EnumerableNullOrEmpty())
                {
                    foreach(ProgressEXPTagDef tag in expTags)
                    {
                        ProgressEXPWorker EXPWorker = (ProgressEXPWorker)Activator.CreateInstance(tag.workerClass);
                        EXPWorker.def = tag;
                        cachedEXPWorkers.Add(EXPWorker);
                    }
                }
                return cachedEXPWorkers;
            }
        }

        public Texture2D BGIcon
        {
            get
            {
                if (trackerBG == null)
                {
                    if (!bgPath.NullOrEmpty())
                    {
                        trackerBG = ContentFinder<Texture2D>.Get(bgPath, true);
                    }
                    else
                    {
                        trackerBG = BaseContent.BadTex;
                    }
                }
                return trackerBG;
            }
        }

        
    }

    public class ProgressLevelLabel
    {
        public int level = 1;

        public string label = "level";
    }

    



}
