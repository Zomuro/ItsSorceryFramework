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
            fireEXPMote(progressTracker.pawn, Math.Abs(exp) * def.expFactor);
            return true;
        }

        public override float drawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            String allDamage = !def.damageDefs.NullOrEmpty() ? labelsFromDef(def.damageDefs).ToStringSafeEnumerable() : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "On receiving damage: ".Colorize(ColoredText.TipSectionTitleColor) + allDamage, true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "Grant experience equal to " + 
                def.expFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor).Colorize(ColoredText.TipSectionTitleColor) +
                " the base damage received by this pawn.", true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
