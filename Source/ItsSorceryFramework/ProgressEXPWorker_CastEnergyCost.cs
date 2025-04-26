using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_CastEnergyCost : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float inputAmt = 0)
        {
            if (progressTracker.Maxed) return false;
            if (inputAmt == 0) return false;

            float finalEXP = Math.Abs(inputAmt * ScalingStatValue(progressTracker.pawn));
            progressTracker.AddExperience(finalEXP);

            if(ItsSorceryUtility.settings.ProgressShowXPMotes) 
                FireEXPMote(progressTracker.pawn, finalEXP);
            return true;
        }

        public override float DrawWorker(Pawn pawn, Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPCast".Translate(ScalingStatDef.label.Named("STAT")).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPCastDesc".Translate(ScalingStatValue(pawn).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor)),
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
