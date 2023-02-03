using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_UseItem: ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {           
            progressTracker.addExperience(def.fixedEXP);
            return true;
        }

        public override float drawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            if (def.expItems.NullOrEmpty()) return 0;

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "On using items: ".Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "Gain experience based on the following", true, false);
            rect.yMin += rect.height;

            rect.x += 6;

            string factor = "";
            foreach(var item in def.expItems)
            {
                if (item.expFactorStat == null) factor = item.exp.ToString("F2");
                else factor = item.exp.ToString("F2") + " x " + item.expFactorStat.label;

                Widgets.LabelCacheHeight(ref rect, "  - " + item.thingDef.label + ": " + factor.Colorize(ColoredText.TipSectionTitleColor), true, false);
                rect.yMin += rect.height;
            }

            return rect.yMin - yMin;
        }

        public IEnumerable<String> labelsFromEXPItems(IEnumerable<EXPConsumableItems> items)
        {
            foreach (var item in items)
            {
                yield return item.thingDef.label;
            }
            yield break;
        }
    }
}
