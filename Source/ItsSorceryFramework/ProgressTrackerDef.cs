using RimWorld;
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

        //public IntRange levelRange = new IntRange(Int32.MinValue.ChangeType<int>(), Int32.MaxValue.ChangeType<int>());

        public float baseEXP = 100f;

        public float scaling = 1.1f;

        //public float maxEXP = 1000f;

        public List<ProgressTrackerClassDef> classes = new List<ProgressTrackerClassDef>();

        public ProgressTrackerClassDef baseClass; 

        /*public List<ProgressLevelModifier> levelFactors = new List<ProgressLevelModifier>();

        public List<ProgressLevelModifier> levelSpecifics = new List<ProgressLevelModifier>();

        public List<ProgressEXPTagDef> expTags = new List<ProgressEXPTagDef>();

        public List<ProgressLevelLabel> levelLabels = new List<ProgressLevelLabel>();*/

        [MustTranslate]
        public string progressLevelUpKey = "ISF_LevelUp";

        [MustTranslate]
        public string progressLevelUpDescKey = "ISF_LevelUpMessage";

        [MustTranslate]
        public string skillPointLabelKey = "ISF_SkillPointLabel";

        //private HashSet<ProgressEXPWorker> cachedEXPWorkers = new HashSet<ProgressEXPWorker>();

        /*public float maxLevel 
        {
            get 
            {
                return progressHediff?.maxSeverity ?? 0f;
            }
        }*/

        public List<ProgressTrackerClassDef> AllClasses
        {
            get
            {
                List<ProgressTrackerClassDef> classList = classes;
                classList.Add(baseClass);
                return classList.Distinct().ToList();
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
            if (baseClass is null)
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} should have a base class specified.");
            }
            /*if (classes.Contains(baseClass))
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName}'s base class {baseClass} should not be contained in the class list.");
            }*/

            if (error) Log.Error("The ProgressTrackerDef " + defName + " has errors.");
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

    /*public class ProgressLevelLabel
    {
        public int level = 1;

        public string label = "level";
    }*/

    



}
