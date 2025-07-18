﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTrackerClassDef : ISF_PrereqDef //Def
    {
        public ProgressTrackerDef progressTrackerDef;
        
        public IntRange levelRange = new IntRange(Int32.MinValue.ChangeType<int>(), Int32.MaxValue.ChangeType<int>());

        public List<ProgressLinkedClassMap> linkedClasses = new List<ProgressLinkedClassMap>();

        public List<ProgressLevelModifier> levelFactors = new List<ProgressLevelModifier>();

        public List<ProgressLevelModifier> levelSpecifics = new List<ProgressLevelModifier>();

        public List<ProgressEXPTagDef> expTags = new List<ProgressEXPTagDef>();

        public List<ProgressLevelLabel> levelLabels = new List<ProgressLevelLabel>();

        // class change validation fields - see ISF_PrereqDef for common prereq fields
        public LearningNodePrereqMode prereqClassMode = LearningNodePrereqMode.All;

        public int prereqClassModeMin = 1;

        public List<ProgressTrackerClassDef> prereqClasses = new List<ProgressTrackerClassDef>();

        public LearningNodePrereqMode prereqClassModeProhibit = LearningNodePrereqMode.All;

        public int prereqClassModeMinProhibit = 1;

        public List<ProgressTrackerClassDef> prereqClassesProhibit = new List<ProgressTrackerClassDef>();

        public List<LearningTrackerDef> unlocks;

        // cacheing fields
        private HashSet<ProgressEXPWorker> cachedEXPWorkers = new HashSet<ProgressEXPWorker>();

        private List<ProgressTrackerClassDef> cachedLinkedClasses = new List<ProgressTrackerClassDef>();

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
                    yield return new StatDrawEntry(statMod.stat.category,
                        statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value, false, ToStringNumberSense.Offset),
                        statMod.stat.description, 4070, null, null, false);
                }
            }

            if (!levelMod.statFactorOffsets.NullOrEmpty())
            {
                foreach (StatModifier statMod in levelMod.statFactorOffsets)
                {
                    yield return new StatDrawEntry(statMod.stat.category,
                        statMod.stat.LabelCap, (statMod.value * 100f).ToString("+#;-#") + "%", //statMod.stat.Worker.ValueToString(statMod.value + 1, false, ToStringNumberSense.Offset), 
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
                        ProgressEXPWorker expWorker = (ProgressEXPWorker)Activator.CreateInstance(tag.workerClass);
                        expWorker.def = tag;
                        cachedEXPWorkers.Add(expWorker);
                    }
                }
                return cachedEXPWorkers;
            }
        }

        public List<ProgressTrackerClassDef> LinkedClassDefs
        {
            get
            {
                if (cachedLinkedClasses.EnumerableNullOrEmpty()) cachedLinkedClasses = linkedClasses.Select(x => x.classDef).ToList();
                return cachedLinkedClasses;
            }
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            bool error = false;

            // null progressTrackerDef that isn't this null class?
            if (progressTrackerDef is null && this != ISF_DefOf.ISF_Generic_Class)
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} should have a base class specified.");
            }

            if (error) Log.Error("The ProgressTrackerClassDef " + defName + " has errors.");
        }
    }

    public class ProgressLinkedClassMap : IExposable
    {
        public ProgressTrackerClassDef classDef;

        public bool levelReset = false;

        public bool benefitReset = false;

        public bool removePostClassChange = true;

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref classDef, "classDef");
            Scribe_Values.Look(ref levelReset, "levelReset", false);
            Scribe_Values.Look(ref benefitReset, "benefitReset", false);
            Scribe_Values.Look(ref removePostClassChange, "removePostClassChange", true);
        }

        public override bool Equals(object obj)
        {
            // equality for hash comparisons based on class def
            ProgressLinkedClassMap compareMapping = obj as ProgressLinkedClassMap;
            return classDef == compareMapping.classDef;
        }

        // hashcode = classdef - we only want unique mappings based on class def; avoid duplicate mappings to the same class
        public override int GetHashCode() => classDef.GetHashCode(); 
    }
    
    public class ProgressLevelLabel
    {
        public int level = 1;

        public string label = "level";
    }




}
