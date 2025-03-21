using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTrackerClassDef : Def
    {
        public ProgressTrackerDef progressTrackerDef;
        
        public IntRange levelRange = new IntRange(Int32.MinValue.ChangeType<int>(), Int32.MaxValue.ChangeType<int>());

        public List<ProgressLinkedClassMap> linkedClasses = new List<ProgressLinkedClassMap>();

        public List<ProgressLevelModifier> levelFactors = new List<ProgressLevelModifier>();

        public List<ProgressLevelModifier> levelSpecifics = new List<ProgressLevelModifier>();

        public List<ProgressEXPTagDef> expTags = new List<ProgressEXPTagDef>();

        public List<ProgressLevelLabel> levelLabels = new List<ProgressLevelLabel>();


        // class change validation fields
        public LearningNodePrereqMode prereqClassMode = LearningNodePrereqMode.All;

        public int prereqClassModeMin = 1;

        public LearningNodePrereqMode prereqNodeMode = LearningNodePrereqMode.All;

        public int prereqNodeModeMin = 1;

        public LearningNodePrereqMode prereqResearchMode = LearningNodePrereqMode.All;

        public int prereqResearchModeMin = 1;

        public int prereqLevel = 0;

        public List<ProgressTrackerClassDef> prereqsClassDefs = new List<ProgressTrackerClassDef>();

        public List<LearningTreeNodeDef> prereqsNodes = new List<LearningTreeNodeDef>();

        public List<ResearchProjectDef> prereqsResearchs = new List<ResearchProjectDef>();

        public Dictionary<HediffDef, float> prereqsHediff = new Dictionary<HediffDef, float>();

        public List<NodeStatReqs> prereqsStats = new List<NodeStatReqs>();

        public List<NodeSkillReqs> prereqsSkills = new List<NodeSkillReqs>();

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
                        statMod.stat.LabelCap, statMod.stat.Worker.ValueToString(statMod.value + 1, false, ToStringNumberSense.Factor),
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
