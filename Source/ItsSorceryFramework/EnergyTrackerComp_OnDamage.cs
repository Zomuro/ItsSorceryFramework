using RimWorld;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_OnDamage : EnergyTrackerComp_DamageBase
    {
        public override void CompPostDamageDealt(DamageInfo damageInfo) 
        {
            if (Props.damageDefs.NullOrEmpty() || Props.damageDefs.Contains(damageInfo.Def))
            {
                float energyMaxChange = damageInfo.Amount * ScalingStatValue; 
                parent.AddEnergy(energyMaxChange);
            }
        }

        public override float CompDrawGUI(Rect rect)
        {
            float yMin = rect.yMin;

            // retrieve string values
            string energyLabel = parent.EnergyLabel;
            string damageDefs = Props.damageDefs.NullOrEmpty() ? "" : DamageDefsLabels(Props.damageDefs).ToStringSafeEnumerable();
            float energyFactor = parent.InvMult * ScalingStatValue;
            string energyFactorString = energyFactor.ToStringByStyle(ScalingStatDef.toStringStyle);

            // draw normal components (label and normal energy regen)
            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnDamageLabel".Translate(ScalingStatDef.label.Named("STAT"), damageDefs.Named("DAMAGEDEFS")).Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnDamageDesc".Translate(energyLabel.Named("ENERGY"),  energyFactorString.Named("FACTOR")));
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }

    }

}
