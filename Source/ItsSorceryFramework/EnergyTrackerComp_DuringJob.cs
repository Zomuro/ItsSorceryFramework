using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_DuringJob : EnergyTrackerComp_JobBase
    {
        public HashSet<JobDef> cachedJobDefs = new HashSet<JobDef>();

        public float cachedScalingStatVal = float.MinValue;

        public int nextRecacheTick = -1;

        public float ScalingStatVal
        {
            get
            {
                if(cachedScalingStatVal == float.MinValue) cachedScalingStatVal = parent.pawn.GetStatValue(ScalingStatDef);
                return cachedScalingStatVal;
            }
        }

        public HashSet<JobDef> JobDefs
        {
            get
            {
                if (cachedJobDefs.NullOrEmpty()) cachedJobDefs = Props.jobDefs.ToHashSet() ?? new HashSet<JobDef>();
                return cachedJobDefs;
            }
        }

        public override void CompClearStatCache()
        {
            nextRecacheTick = Find.TickManager.TicksGame + PawnCacheUtility.GetEnergyTickOffset();
            cachedScalingStatVal = float.MinValue;
        }

        public override void CompPostTick()
        {
            // recache scaling stat
            if (Find.TickManager.TicksGame >= nextRecacheTick) CompClearStatCache();

            // nullcheck list of jobs to check - if false, ignore
            if (JobDefs.NullOrEmpty()) return;
            if (parent.pawn.CurJobDef is null || !JobDefs.Contains(parent.pawn.CurJobDef)) return;

            // add energy to energytracker
            float energyChange = 1.TicksToSeconds() * Props.baseEnergy * ScalingStatVal;
            parent.AddEnergy(energyChange);
        }

        public override float CompDrawGUI(Rect rect)
        {
            float yMin = rect.yMin;

            // retrieve string values
            string energyLabel = parent.EnergyLabel;
            float energyRate = parent.InvMult * Props.baseEnergy * ScalingStatVal;
            string energyRateString = energyRate.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset);

            // draw job related values
            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompDuringJobLabel".Translate(Props.jobSetLabel, ScalingStatDef.label.Named("STAT")).Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompDuringJobDesc".Translate(energyLabel.Named("ENERGY"), energyRateString.Named("CHANGE")));
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }

}
