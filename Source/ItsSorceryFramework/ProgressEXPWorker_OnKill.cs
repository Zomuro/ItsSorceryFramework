using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_OnKill : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            if (progressTracker.Maxed) return false;
            progressTracker.AddExperience(def.fixedEXP);
            if (ItsSorceryUtility.settings.ProgressShowXPMotes)
                FireEXPMote(progressTracker.pawn, def.fixedEXP);
            return true;
        }

        public override float DrawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;
          
            String allDamage = !def.damageDefs.NullOrEmpty() ? LabelsFromDef(def.damageDefs).ToStringSafeEnumerable() : "";      

            Text.Font = GameFont.Small;
            if(allDamage != "") Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPOnKillDamage".Translate(allDamage).Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            else Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnKill".Translate().Colorize(ColoredText.TipSectionTitleColor), 
                true, false);

            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnKillDesc".Translate(def.fixedEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Absolute)), 
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
