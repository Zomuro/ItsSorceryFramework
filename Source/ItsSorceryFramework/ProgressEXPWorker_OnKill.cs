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
    public class ProgressEXPWorker_OnKill : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {           
            progressTracker.addExperience(def.fixedEXP);
            fireEXPMote(progressTracker.pawn, def.fixedEXP);
            return true;
        }

        public override float drawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;
          
            String allDamage = !def.damageDefs.NullOrEmpty() ? labelsFromDef(def.damageDefs).ToStringSafeEnumerable() : "";      

            Text.Font = GameFont.Small;
            if(allDamage != "") Widgets.LabelCacheHeight(ref rect, "On kill with damage: ".Colorize(ColoredText.TipSectionTitleColor) + allDamage, true, false);
            else Widgets.LabelCacheHeight(ref rect, "On kill: ".Colorize(ColoredText.TipSectionTitleColor), true, false);

            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "Grant " + 
                def.fixedEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor).Colorize(ColoredText.TipSectionTitleColor) +
                " experience.", true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
