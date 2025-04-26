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

            float finalEXP = def.fixedEXP * ScalingStatValue(progressTracker.pawn);
            progressTracker.AddExperience(finalEXP);
            return true;
        }

        public override float DrawWorker(Pawn pawn, Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            String skills = !def.skillDefs.NullOrEmpty() ? LabelsFromDef(def.skillDefs).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPSkill".Translate(ScalingStatDef.label.Named("STAT"), skills.Named("SKILLDEFS")).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;

            float finalEXP = def.fixedEXP * ScalingStatValue(pawn);
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPSkillDesc".Translate(finalEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor)),
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
