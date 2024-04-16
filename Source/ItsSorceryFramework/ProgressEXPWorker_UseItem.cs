using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPWorker_UseItem: ProgressEXPWorker
    {
        public override bool TryExecute(ProgressTracker progressTracker, float exp = 0)
        {
            if (progressTracker.Maxed) return false;
            progressTracker.AddExperience(def.fixedEXP);
            return true;
        }

        public override float DrawWorker(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            if (def.expItems.NullOrEmpty()) return 0;

            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPUseItem".Translate().Colorize(ColoredText.TipSectionTitleColor), 
                true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, 
                "ISF_LearningProgressEXPUseItemDesc".Translate(), 
                true, false);
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
