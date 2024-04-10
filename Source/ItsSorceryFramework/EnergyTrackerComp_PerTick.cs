using RimWorld;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_PerTick : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_PerTick Props => (EnergyTrackerCompProperties_PerTick)props;

        public override void CompExposeData() { } // saving values to comp, if needed

        public float ToTick => 1.TicksToSeconds(); // set to conversion value from tick to second.

        public float InvFactor => parent.InvMult; // if inverse energy tracker, this is -1f; otherwise, it is 1f.

        public StatDef RecoveryRateStatDef => Props.energyRecoveryStatDef ?? StatDefOf_ItsSorcery.ISF_EnergyRecovery;

        public float RecoveryRate => parent.pawn.GetStatValue(RecoveryRateStatDef);

        public override void CompPostTick() 
        {
            float energyChange = parent.InvMult * ToTick * RecoveryRate;

            if (parent.InDeficit)
            {
                if (!parent.def.inverse) parent.currentEnergy = Mathf.Max(parent.currentEnergy + energyChange * Props.deficitRecoveryFactor, parent.AbsMinEnergy);
                else parent.currentEnergy = Mathf.Min(parent.currentEnergy + energyChange * Props.deficitRecoveryFactor, parent.AbsMaxEnergy);
            }
            else if (parent.InOvercharge)
            {
                if (!parent.def.inverse) parent.currentEnergy = Mathf.Min(parent.currentEnergy + energyChange * Props.overchargeRecoveryFactor, parent.AbsMaxEnergy);
                else parent.currentEnergy = Mathf.Max(parent.currentEnergy + energyChange * Props.overchargeRecoveryFactor, parent.AbsMinEnergy);
            }
            else parent.currentEnergy = Mathf.Clamp(parent.currentEnergy + energyChange, parent.MinEnergy, parent.MaxEnergy);
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
