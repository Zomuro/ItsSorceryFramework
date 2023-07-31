using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_OnSkillEXP : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            if (progressTracker.Maxed) return false;
            progressTracker.AddExperience(exp * def.expFactor);
            return true;
        }

        public override float drawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            String skills = !def.skillDefs.NullOrEmpty() ? labelsFromDef(def.skillDefs).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "On gaining skill experience: ".Colorize(ColoredText.TipSectionTitleColor) + skills, true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "Grant experience equal to " + 
                def.expFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor).Colorize(ColoredText.TipSectionTitleColor) +
                " the experience gained while learning skills.", true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
