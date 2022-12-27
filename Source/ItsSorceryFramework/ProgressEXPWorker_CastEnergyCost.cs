using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_CastEnergyCost : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp)
        {
            if (exp == 0) return false;
            
            progressTracker.addExperience(Math.Abs(exp));
            return true;
        }

        public override float drawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "On cast:".Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "Grant experience equal to x" + 
                def.expFactor.ToString("F2").Colorize(ColoredText.TipSectionTitleColor) +
                " the base energy cost.", true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
