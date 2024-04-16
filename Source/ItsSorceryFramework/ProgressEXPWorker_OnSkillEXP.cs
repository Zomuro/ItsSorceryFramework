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

        public override float DrawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            String skills = !def.skillDefs.NullOrEmpty() ? LabelsFromDef(def.skillDefs).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPSkill".Translate(skills).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPSkillDesc".Translate(def.expFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor)),
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
