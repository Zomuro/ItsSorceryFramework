using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_OnQuestFinish : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            if (progressTracker.Maxed) return false;
            
            progressTracker.AddExperience(Math.Abs(def.fixedEXP));
            if (ItsSorceryUtility.settings.ProgressShowXPMotes)
                FireEXPMote(progressTracker.pawn, Math.Abs(def.fixedEXP));
            return true;
        }

        public override float DrawWorker(Rect rect)
        {
            float yMin = rect.yMin;
          
            String allQuests = !def.questDefs.NullOrEmpty() ? LabelsFromDef(def.questDefs).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnQuestFinish".Translate(def.questOutcome.ToString().ToLower(), allQuests).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnQuestFinishDesc".Translate((def.fixedEXP * def.expFactor).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor)), 
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
