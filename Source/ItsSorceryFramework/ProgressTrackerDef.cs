using System;
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

        public float baseEXP = 100f;

        public float scaling = 1.1f;

        public ProgressTrackerClassDef baseClass; 

        [MustTranslate]
        public string progressLevelUpKey = "ISF_LevelUp";

        [MustTranslate]
        public string progressLevelUpDescKey = "ISF_LevelUpMessage";

        [MustTranslate]
        public string skillPointLabelKey = "ISF_SkillPointLabel";

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
            if (baseClass != null && baseClass.progressTrackerDef != this && baseClass != ISF_DefOf.ISF_Generic_Class)
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName}'s base class {baseClass.defName} should be linked to {defName}, unless it is the generic class {ISF_DefOf.ISF_Generic_Class.defName}.");
            }

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
}
