using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_OnInteraction : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            if (progressTracker.Maxed) return false;
            if (exp <= 0) return false;
            
            progressTracker.AddExperience(Math.Abs(def.fixedEXP));
            if (ItsSorceryUtility.settings.ProgressShowXPMotes)
                FireEXPMote(progressTracker.pawn, Math.Abs(def.fixedEXP));
            return true;
        }

        public override float DrawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;
          
            String allInteractions = !def.interactionDefs.NullOrEmpty() ? LabelsFromDef(def.interactionDefs).ToStringSafeEnumerable() : "";      

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnInteraction".Translate(allInteractions).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnInteractionDesc".Translate((def.fixedEXP * def.expFactor).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor)), 
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
