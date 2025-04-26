using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_OnInteraction : ProgressEXPWorker
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
            float x = rect.x;
          
            String allInteractions = !def.interactionDefs.NullOrEmpty() ? LabelsFromDef(def.interactionDefs).ToStringSafeEnumerable() : "";      

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnInteraction".Translate(ScalingStatDef.label.Named("STAT"), allInteractions.Named("INTERACTIONDEFS")).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;

            float finalEXP = def.fixedEXP * ScalingStatValue(pawn);
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnInteractionDesc".Translate(finalEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Absolute)), 
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
