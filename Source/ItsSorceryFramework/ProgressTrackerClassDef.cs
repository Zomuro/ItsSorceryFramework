﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTrackerClassDef : Def
    {
        public IntRange levelRange = new IntRange(Int32.MinValue.ChangeType<int>(), Int32.MaxValue.ChangeType<int>());

        public List<ProgressTrackerClassDef> prereqClasses = new List<ProgressTrackerClassDef>();

        public List<ProgressAltClassMap> altClasses = new List<ProgressAltClassMap>();

        //public List<ProgressTrackerClassDef> exclusiveClasses = new List<ProgressTrackerClassDef>();

        public List<ProgressLevelModifier> levelFactors = new List<ProgressLevelModifier>();

        public List<ProgressLevelModifier> levelSpecifics = new List<ProgressLevelModifier>();

        public List<ProgressEXPTagDef> expTags = new List<ProgressEXPTagDef>();

        public List<ProgressLevelLabel> levelLabels = new List<ProgressLevelLabel>();

        private HashSet<ProgressEXPWorker> cachedEXPWorkers = new HashSet<ProgressEXPWorker>();

        private List<ProgressTrackerClassDef> cachedAltClasses = new List<ProgressTrackerClassDef>();

        private List<ProgressTrackerClassDef> cachedExclusiveClasses = new List<ProgressTrackerClassDef>();

        /*public override void ResolveReferences()
        {
            base.ResolveReferences();

            bool error = false;

            *//*int hediffCheckCount =  SorcerySchemaUtility.AllProgressTrackerDefs.Where(x => x.progressHediff == progressHediff).Count();

            if (hediffCheckCount > 1)
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} should not have the same hediff as other ProgressTrackerDefs.");
            }*//*
            

            if (error) Log.Error("The ProgressTrackerClassDef " + defName + " has errors.");
        }*/

        public IEnumerable<StatDrawEntry> SpecialDisplayMods(ProgressLevelModifier levelMod)
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

        public ProgressLevelModifier GetLevelFactor(float severity) 
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

        public ProgressLevelModifier GetLevelSpecific(float severity)
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

        public List<ProgressTrackerClassDef> AltClassDefs
        {
            get
            {
                if (cachedAltClasses.EnumerableNullOrEmpty()) cachedAltClasses = altClasses.Select(x => x.classDef).ToList();
                return cachedAltClasses;
            }
        }

        /*public List<ProgressTrackerClassDef> ExclusiveClassDefs
        {
            get
            {
                if (cachedExclusiveClasses.EnumerableNullOrEmpty()) cachedExclusiveClasses = altClasses.Where(x => x.exclusive == false).Select(x => x.classDef).ToList();
                return cachedExclusiveClasses;
            }
        }*/
    }

    public class ProgressAltClassMap
    {
        public ProgressTrackerClassDef classDef;

        public bool levelReset = false;

        public bool benefitReset = false;
    }
    
    public class ProgressLevelLabel
    {
        public int level = 1;

        public string label = "level";
    }




}
