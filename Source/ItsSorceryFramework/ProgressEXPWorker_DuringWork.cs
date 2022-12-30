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
    public class ProgressEXPWorker_DuringWork : ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            Log.Message(def.workGiverDef.ToString());
            if (progressTracker.pawn.GetWorkgiver() != def.workGiverDef) return false;
           
            progressTracker.addExperience(def.fixedEXP);
            return true;
        }

        public override float drawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            //String skills = !def.skillDefs.NullOrEmpty() ? labelsFromDef(def.skillDefs).ToStringSafeEnumerable() : "";

            if (def.workGiverDef == null) return 0;

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "On performing work: ".Colorize(ColoredText.TipSectionTitleColor) + def.workGiverDef.label, true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "Grant " + 
                def.fixedEXP.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Absolute).Colorize(ColoredText.TipSectionTitleColor) +
                " experience every second while performing work.", true, false);
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }
}
