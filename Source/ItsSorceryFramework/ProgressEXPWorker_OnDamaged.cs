using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_OnDamaged : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float inputAmt = 0)
        {
            if (progressTracker.Maxed) return false;
            if (inputAmt <= 0) return false;

            float finalEXP = inputAmt * ScalingStatValue(progressTracker.pawn);
            progressTracker.AddExperience(finalEXP);
            if (ItsSorceryUtility.settings.ProgressShowXPMotes)
                FireEXPMote(progressTracker.pawn, finalEXP);
            return true;
        }

        public override float DrawWorker(Pawn pawn, Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            String allDamage = !def.damageDefs.NullOrEmpty() ? LabelsFromDef(def.damageDefs).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnDamaged".Translate(ScalingStatDef.label.Named("STAT"), allDamage.Named("DAMAGEDEFS")).Colorize(ColoredText.TipSectionTitleColor),
                true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect,
                "ISF_LearningProgressEXPOnDamagedDesc".Translate(ScalingStatValue(pawn).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor)),
                true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
