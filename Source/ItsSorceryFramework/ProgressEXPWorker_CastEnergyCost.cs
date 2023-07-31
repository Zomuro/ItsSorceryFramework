using System;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_CastEnergyCost : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            if (progressTracker.Maxed) return false;
            if (exp == 0) return false;
            
            progressTracker.AddExperience(Math.Abs(exp) * def.expFactor);
            fireEXPMote(progressTracker.pawn, Math.Abs(exp) * def.expFactor);
            return true;
        }

        public override float drawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "On cast:".Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "Grant experience equal to " + 
                def.expFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor).Colorize(ColoredText.TipSectionTitleColor) +
                " the base energy cost.", true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
