using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_Passive : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float inputAmt = 0)
        {
            if (progressTracker.Maxed) return false;

            float finalEXP = Math.Abs(def.fixedEXP * ScalingStatValue(progressTracker.pawn));
            progressTracker.AddExperience(finalEXP);
            return true;
        }

        public override float DrawWorker(Pawn pawn, Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPPassive".Translate(ScalingStatDef.label.Named("STAT")).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;

            float finalEXP = Math.Abs(def.fixedEXP * ScalingStatValue(pawn));
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPPassiveDesc".Translate(finalEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset)),
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
