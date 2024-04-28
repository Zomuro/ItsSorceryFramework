using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_DuringJob : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            if (progressTracker.Maxed) return false;
            if (progressTracker.pawn.CurJobDef == null) return false;

            if (def.jobSets.NullOrEmpty()) return false;
            foreach(EXPJobSet sets in def.jobSets)
            {
                if (sets.jobDefs.Contains(progressTracker.pawn.CurJobDef))
                {
                    progressTracker.AddExperience(def.fixedEXP);
                    if (ItsSorceryUtility.settings.ProgressShowXPMotes)
                        FireEXPMote(progressTracker.pawn, def.fixedEXP);
                    return true;
                }
            }
            return false;
        }

        public override float DrawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            if (def.jobSets.NullOrEmpty()) return 0;
            String allJobSets = !def.jobSets.NullOrEmpty() ? labelsFromEXPJobSets(def.jobSets).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPJob".Translate(allJobSets).Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPJobDesc".Translate(def.fixedEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Absolute))
                , true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }

        public IEnumerable<String> labelsFromEXPJobSets(IEnumerable<EXPJobSet> jobSets)
        {
            foreach (var set in jobSets)
            {
                yield return set.label;
            }
            yield break;
        }

        public List<JobDef> allJobs
        {
            get
            {
                if (jobDefs.NullOrEmpty())
                {
                    if (def.jobSets.NullOrEmpty()) return jobDefs;

                    List<JobDef> tempJobs = new List<JobDef>();
                    foreach (EXPJobSet sets in def.jobSets)
                    {
                        tempJobs.AddRange(sets.jobDefs);
                    }

                    jobDefs = tempJobs.Distinct().ToList();
                }

                return jobDefs;
            }
        }

        public List<JobDef> jobDefs = new List<JobDef>();
    }
}
