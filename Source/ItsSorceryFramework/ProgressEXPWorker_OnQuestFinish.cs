using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_OnQuestFinish : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float inputAmt = 0)
        {
            if (progressTracker.Maxed) return false;

            float finalEXP = def.fixedEXP * ScalingStatValue(progressTracker.pawn);
            progressTracker.AddExperience(finalEXP);
            if (ItsSorceryUtility.settings.ProgressShowXPMotes)
                FireEXPMote(progressTracker.pawn, finalEXP);
            return true;
        }

        public override float DrawWorker(Pawn pawn, Rect rect)
        {
            float yMin = rect.yMin;
          
            String allQuests = !def.questDefs.NullOrEmpty() ? LabelsFromDef(def.questDefs).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnQuestFinish".Translate(def.questOutcome.ToString().ToLower().Named("OUTCOME"), 
                ScalingStatDef.label.Named("STAT"), allQuests.Named("QUESTDEFS")).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;

            float finalEXP = def.fixedEXP * ScalingStatValue(pawn);
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnQuestFinishDesc".Translate(finalEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Absolute)), 
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
