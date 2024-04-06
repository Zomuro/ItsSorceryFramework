using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_OnDamage : EnergyTrackerComp_DamageBase
    {
        public override void CompPostDamageDealt(DamageInfo damageInfo) 
        {
            if (Props.damageDefs.NullOrEmpty() || Props.damageDefs.Contains(damageInfo.Def))
            {
                //StatDef refStatDef = Props.scalingStatDef is null ? StatDefOf_ItsSorcery.Scaling_ItsSorcery: Props.scalingStatDef;
                float energyMaxChange = parent.InvMult * damageInfo.Amount * parent.pawn.GetStatValue(ScalingStatDef);
                parent.currentEnergy = Mathf.Clamp(parent.currentEnergy + energyMaxChange, parent.AbsMinEnergy, parent.AbsMaxEnergy);
                // in the future, add effect activation here.
            }
        }

        public override float CompDrawGUI(Rect rect)
        {
            float yMin = rect.yMin;

            // retrieve string values
            string energyLabel = parent.EnergyLabel;
            string damageDefs = Props.damageDefs.NullOrEmpty() ? "" : DamageDefsLabels(Props.damageDefs).ToStringSafeEnumerable();
            float energyFactor = parent.InvMult * parent.pawn.GetStatValue(ScalingStatDef);
            string energyFactorString = energyFactor.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Factor);

            // draw normal components (label and normal energy regen)
            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnDamageLabel".Translate(damageDefs).Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnDamageDesc".Translate(energyLabel.Named("ENERGY"), ScalingStatDef.label.Named("STAT"), energyFactorString.Named("FACTOR")));
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }

    }

}
