using RimWorld;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_PerTick : EnergyTrackerComp
    {
        public float cachedRecoveryRate = float.MinValue;

        public int nextRecacheTick = -1;

        public EnergyTrackerCompProperties_PerTick Props => (EnergyTrackerCompProperties_PerTick)props;

        public override void CompExposeData() { } // saving values to comp, if needed

        public StatDef RecoveryRateStatDef => Props.energyRecoveryStatDef ?? StatDefOf_ItsSorcery.ISF_EnergyRecovery;

        public float RecoveryRate
        {
            get
            {
                if (cachedRecoveryRate == float.MinValue) cachedRecoveryRate = parent.pawn.GetStatValue(RecoveryRateStatDef);
                return cachedRecoveryRate;
            }
        }

        public override void CompClearStatCache()
        {
            //int baseTicks = ItsSorceryUtility.settings.EnergyStatCacheTicks;
            nextRecacheTick = Find.TickManager.TicksGame + PawnCacheUtility.GetEnergyTickOffset(); // UnityEngine.Random.Range(baseTicks - 3, baseTicks + 3);
            cachedRecoveryRate = float.MinValue;
        }

        public override void CompPostTick() 
        {
            if (Find.TickManager.TicksGame >= nextRecacheTick) CompClearStatCache();

            float energyChange = 1.TicksToSeconds() * RecoveryRate; // inverse system => -1; recovery rate 5

            if (parent.InDeficit) parent.AddEnergy(energyChange * Props.deficitRecoveryFactor);
            else if (parent.InOvercharge) parent.AddEnergy(energyChange * Props.overchargeRecoveryFactor);
            else parent.AddEnergy(energyChange, true);
        } 

        public override float CompDrawGUI(Rect rect)
        {
            float yMin = rect.yMin;

            // retrieve string values
            string energyLabel = parent.EnergyLabel;
            float energyRate = parent.InvMult * parent.pawn.GetStatValue(Props.energyRecoveryStatDef);
            string energyRateString = energyRate.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset);

            // draw normal components (label and normal energy regen)
            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnTickLabel".Translate(RecoveryRateStatDef.label.Named("STAT")).Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnTickNormal".Translate(energyLabel.Named("ENERGY"), energyRateString.Named("CHANGE")));
            rect.yMin += rect.height;

            // draw specialized components (overcharge and deficit regen)
            if (parent.HasOverchargeZone)
            {
                string overchargeEnergyRate = (energyRate * Props.overchargeRecoveryFactor).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset);
                Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnTickOvercharge".Translate(energyLabel.Named("ENERGY"), overchargeEnergyRate.Named("CHANGE")));
                rect.yMin += rect.height;
            }
            if (parent.HasDeficitZone)
            {
                string deficitEnergyRate = (energyRate * Props.deficitRecoveryFactor).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset);
                Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnTickDeficit".Translate(energyLabel.Named("ENERGY"), deficitEnergyRate.Named("CHANGE")));
                rect.yMin += rect.height;
            }

            return rect.yMin - yMin;
        }

    }

}
