using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_OnDamaged : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            Log.Message("test: " + exp);
            if (progressTracker.Maxed) return false;
            if (exp <= 0) return false;
            
            progressTracker.AddExperience(Math.Abs(exp) * def.expFactor);
            FireEXPMote(progressTracker.pawn, Math.Abs(exp) * def.expFactor);
            return true;
        }

        public override float DrawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            String allDamage = !def.damageDefs.NullOrEmpty() ? LabelsFromDef(def.damageDefs).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnDamaged".Translate(allDamage).Colorize(ColoredText.TipSectionTitleColor),
                true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnDamagedDesc".Translate(def.expFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor)),
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
