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
    public class EnergyTrackerComp_OnKill : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_OnKill Props => (EnergyTrackerCompProperties_OnKill)props;

        public StatDef ScalingStatDef => Props.scalingStatDef is null ? StatDefOf_ItsSorcery.ISF_ScalingStat : Props.scalingStatDef;

        public IEnumerable<String> DamageDefsLabels(IEnumerable<Def> defs)
        {
            foreach (var def in defs) yield return def.label;
            yield break;
        }

        public override void CompPostKill(DamageInfo? damageInfo) 
        {
            if (damageInfo is null || damageInfo.Value.Instigator is null || damageInfo.Value.Instigator as Pawn != parent.pawn) return;

            if (Props.damageDefs.NullOrEmpty() || Props.damageDefs.Contains(damageInfo.Value.Def))
            {
                //StatDef refStatDef = Props.scalingStatDef is null ? StatDefOf_ItsSorcery.Scaling_ItsSorcery : Props.scalingStatDef;
                float energyMaxChange = parent.InvMult * Props.baseEnergy * parent.pawn.GetStatValue(ScalingStatDef);
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
            string energyFactorString = energyFactor.ToStringByStyle(ScalingStatDef.toStringStyle);

            // draw normal components (label and normal energy regen)
            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnKillLabel".Translate(ScalingStatDef.label.Named("STAT"), damageDefs.Named("DAMAGEDEFS")).Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnKillDesc".Translate(energyLabel.Named("ENERGY"), 
                energyFactorString.Named("FACTOR"), Props.baseEnergy.Named("CHANGE")));
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }

}
