using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_Passive : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            if (progressTracker.Maxed) return false;
            progressTracker.AddExperience(Math.Abs(def.fixedEXP));
            return true;
        }

        public override float DrawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPPassive".Translate().Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPPassiveDesc".Translate(def.fixedEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset)),
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
