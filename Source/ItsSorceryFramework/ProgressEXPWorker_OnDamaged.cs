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
    public class ProgressEXPWorker_OnDamaged : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp)
        {
            if (exp == 0) return false;
            
            progressTracker.addExperience(Math.Abs(exp) * def.expFactor);
            return true;
        }

        public override float drawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            String damage = def.damageDef != null ? def.damageDef.label : "";

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "On receiving damage: ".Colorize(ColoredText.TipSectionTitleColor) + damage, true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "Grant experience equal to x" + 
                def.expFactor.ToString("F2").Colorize(ColoredText.TipSectionTitleColor) +
                " the base damage received by this pawn.", true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
