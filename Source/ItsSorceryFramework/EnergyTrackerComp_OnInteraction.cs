using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_OnInteraction : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_OnInteraction Props => (EnergyTrackerCompProperties_OnInteraction)props;

        public float cachedScalingStatVal = float.MinValue;

        public int nextRecacheTick = -1;

        public IEnumerable<String> InteractionDefLabels(IEnumerable<Def> defs)
        {
            foreach (var def in defs) yield return def.label;
            yield break;
        }

        public StatDef ScalingStatDef => Props.scalingStatDef is null ? StatDefOf_ItsSorcery.ISF_ScalingStat : Props.scalingStatDef;

        public float ScalingStatVal
        {
            get
            {
                if (cachedScalingStatVal == float.MinValue) cachedScalingStatVal = parent.pawn.GetStatValue(ScalingStatDef);
                return cachedScalingStatVal;
            }
        }

        public override void CompPostInteraction(InteractionDef interactionDef) 
        {
            if (interactionDef is null) return;

            if (Props.interactionDefs.NullOrEmpty() || Props.interactionDefs.Contains(interactionDef))
            {
                float energyMaxChange = Props.baseEnergy * parent.pawn.GetStatValue(ScalingStatDef);
                parent.AddEnergy(energyMaxChange);
            }
        } 

        public override float CompDrawGUI(Rect rect)
        {
            float yMin = rect.yMin;

            // retrieve string values
            string energyLabel = parent.EnergyLabel;
            string interactionDefsStr = Props.interactionDefs.NullOrEmpty() ? "" : InteractionDefLabels(Props.interactionDefs).ToStringSafeEnumerable();
            string energyMaxChange = (parent.InvMult * Props.baseEnergy * parent.pawn.GetStatValue(ScalingStatDef)).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset);

            // draw normal components (label and normal energy regen)
            Text.Font = GameFont.Small;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnInteractionLabel".Translate(ScalingStatDef.label.Named("STAT"), interactionDefsStr.Named("INTERACTDEFS")).Colorize(ColoredText.TipSectionTitleColor), true, false);
            rect.yMin += rect.height;
            Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompOnInteractionDesc".Translate(energyLabel.Named("ENERGY"), energyMaxChange.Named("CHANGE")));
            rect.yMin += rect.height;

            return rect.yMin - yMin;
        }
    }

}
