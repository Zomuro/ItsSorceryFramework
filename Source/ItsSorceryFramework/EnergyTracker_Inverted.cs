﻿using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTracker_Inverted : EnergyTracker
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_Inverted(Pawn pawn) : base(pawn)
        {
        }

        public EnergyTracker_Inverted(Pawn pawn, EnergyTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool HasLimit
        {
            get
            {
                if (OverMaxEnergy > MaxEnergy) return true;
                return false;
            }
        }

        public override float MaxEnergy
        {
            get
            {
                return this.pawn.GetStatValue(def.energyMaxStatDef ?? StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery, true);
            }
        }

        public override float MinEnergy
        {
            get
            {
                return Math.Max(pawn.GetStatValue(def.energyMinStatDef ?? StatDefOf_ItsSorcery.MinEnergy_ItsSorcery, true), 0f);
            }
        }

        public override float OverMaxEnergy // limit to 2x the maximum energy - pragmatic UI choice
        {
            get
            {
                return Math.Min(Math.Max(MaxEnergy, pawn.GetStatValue(def.energyOverMaxStatDef ?? (def.energyMaxStatDef ?? StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery), true)), 
                    MaxEnergy * 2f);
            }
        }

        public override float EnergyRecoveryRate
        {
            get
            {
                return this.pawn.GetStatValue(def.energyRecoveryStatDef ?? StatDefOf_ItsSorcery.EnergyRecovery_ItsSorcery, true);
            }
        }

        public override float EnergyCostFactor
        {
            get
            {
                return this.pawn.GetStatValue(def.energyCostFactorStatDef ?? StatDefOf_ItsSorcery.EnergyCostFactor_ItsSorcery, true);
            }
        }

        public override float OverBarRecoveryFactor
        {
            get
            {
                return Math.Max(def.overBarRecoveryFactor, 0.1f);
            }
        }

        public override float UnderBarRecoveryFactor
        {
            get
            {
                return Math.Max(def.underBarRecoveryFactor, 0.1f);
            }
        }

        public override float EnergyToRelativeValue(float energyCost = 0)
        {
            float tempCurrentEnergy = currentEnergy + energyCost;

            if (tempCurrentEnergy <= MinEnergy)
            {
                return MinEnergy / MaxEnergy;
            }
            if (tempCurrentEnergy >= OverMaxEnergy)
            {
                return OverMaxEnergy / MaxEnergy;
            }
            return tempCurrentEnergy / MaxEnergy;
        }

        public override void EnergyTrackerTick()
        {
            float tempEnergy;
            if (currentEnergy < 0)
            {
                tempEnergy = Math.Min(currentEnergy + 1.TicksToSeconds() * EnergyRecoveryRate * UnderBarRecoveryFactor,
                    MaxEnergy);
            }
            else if (currentEnergy <= MaxEnergy) // when energy is under or equal the normal max
            {
                tempEnergy = Math.Min(currentEnergy - 1.TicksToSeconds() * EnergyRecoveryRate, MaxEnergy);
            }
            else // when energy is over the normal max
            {
                tempEnergy = Math.Min(currentEnergy - 1.TicksToSeconds() * EnergyRecoveryRate * OverBarRecoveryFactor,
                    OverMaxEnergy);
            }

            currentEnergy = Math.Max(tempEnergy, MinEnergy);
        }

        public override bool WouldReachLimitEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if ((currentEnergy + energyCost) > MaxEnergy && Schema.limitLocked) return true;
            return false;
        }

        public override bool TryAlterEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (!WouldReachLimitEnergy(energyCost))
            {
                currentEnergy = Math.Min(Math.Max(MinEnergy, currentEnergy + energyCost), OverMaxEnergy);
                this.ApplyHediffSeverity(this.EnergyToRelativeValue());
                return true;
            }
            
            return false;
        }

        public override void ApplyHediffSeverity(float relVal)
        {
            if (OverMaxEnergy == MaxEnergy) return;
            float relValNorm = relVal - 1f;
            float maxRelVal = OverMaxEnergy / MaxEnergy - 1f;

            HediffDef hediffDef = this.def.sideEffect;
            if (hediffDef == null) return;
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (relVal > 1f)
            {
                if (hediff == null) HealthUtility.AdjustSeverity(this.pawn, hediffDef, relValNorm / maxRelVal);
                else if (relValNorm / maxRelVal > hediff.Severity) hediff.Severity = relValNorm / maxRelVal;
            }

            if (hediff != null && hediff.Severity >= hediff.def.maxSeverity)
            {
                Messages.Message("ISF_MessagePastLimit".Translate(this.pawn.Named("PAWN")),
                    this.pawn, MessageTypeDefOf.NegativeEvent, true);
            }

        }

        public override float DrawOnGUI(ref Rect rect)
        {
            // get original rect
            Rect orgRect = new Rect(rect);
            float coordY = 0;

            // add space
            coordY += 10;
            rect.y += coordY;

            // set up label and bar rects
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect labelBox = new Rect(rect);
            labelBox.width = rect.width / 2;
            labelBox.ContractedBy(5);
            Rect barBox = new Rect(labelBox);
            barBox.x = rect.width * 2 / 5 + rect.x;

            // energy label
            Widgets.LabelCacheHeight(ref labelBox, def.energyLabelKey.Translate().CapitalizeFirst());

            // draws power bar
            barBox.height = labelBox.height; // set barbox to labelbox height for consistency
            DrawEnergyBar(barBox);

            // draw amount of energy
            string energyLabel = this.currentEnergy.ToString("F0") + " / " + this.MaxEnergy.ToString("F0");
            Widgets.Label(barBox, energyLabel);
            Text.Anchor = TextAnchor.UpperLeft;

            // highlight energy costs
            HightlightEnergyCost(barBox);

            // add label/barbox height + add a small boundary space for appearance
            coordY += labelBox.height + 10;
            // reset rectangle
            rect = orgRect;
            // return accumulated height
            return coordY;
        }

        public override void DrawEnergyBar(Rect rect)
        {
            if (this.EnergyRelativeValue < 0)
            {
                Widgets.FillableBar(rect, Mathf.Min(this.EnergyRelativeValue + 1, 1f),
                    GizmoTextureUtility.EmptyBarTex, GizmoTextureUtility.OverBarTex, true);
            }
            else if (this.EnergyRelativeValue <= 1)
            {
                Widgets.FillableBar(rect, Mathf.Min(this.EnergyRelativeValue, 1f), GizmoTextureUtility.BarTex,
                    GizmoTextureUtility.EmptyBarTex, true);
            }
            else
            {
                /*Widgets.FillableBar(barBox, Mathf.Min((this.EnergyRelativeValue - 1f) / (MaxEnergyOverload / MaxEnergy - 1), 1f), 
                    GizmoTextureUtility.UnderBarTex,
                    GizmoTextureUtility.BarTex, true);*/
                Widgets.FillableBar(rect, Mathf.Min((this.EnergyRelativeValue - 1), 1f),
                    GizmoTextureUtility.UnderBarTex,
                    GizmoTextureUtility.BarTex, true);
            }
        }

        public override void HightlightEnergyCost(Rect rect)
        {
            // return if tabwindow is null
            MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
            if (mainTabWindow_Inspect == null) return;

            // return if hovered gizmo is null
            Command_Sorcery command_Sorcery = ((mainTabWindow_Inspect != null) ? mainTabWindow_Inspect.LastMouseoverGizmo : null) as Command_Sorcery;
            if (command_Sorcery == null) return;

            // return if it isn't a sorceryDef or isn't the same energytracker
            SorceryDef sorceryDef = (command_Sorcery?.Ability as Sorcery)?.sorceryDef;
            if (sorceryDef == null || !sorceryDef.sorcerySchema.energyTrackerDefs.Contains(def)) return;

            Rect highlight = rect.ContractedBy(3f);
            float max = highlight.xMax;
            float min = highlight.xMin;
            float width = highlight.width;

            float relativeEnergyDiff = this.EnergyToRelativeValue(sorceryDef.statBases.GetStatValueFromList(def.energyUnitStatDef, 0) * EnergyCostFactor);
            float relativeEnergy = this.EnergyRelativeValue;

            // used to make random blinking effect on highlight
            float num = Mathf.Repeat(Time.time, 0.85f);
            float num2 = 1f;
            if (num < 0.1f)
            {
                num2 = num / 0.1f;
            }
            else if (num >= 0.25f)
            {
                num2 = 1f - (num - 0.25f) / 0.6f;
            }

            if (sorceryDef.statBases.GetStatValueFromList(def.energyUnitStatDef, 0) * EnergyCostFactor < 0f) // if current relative qi > after using ability
            {
                if (findFloor(relativeEnergy) != findFloor(relativeEnergyDiff))
                {
                    highlight.xMin = Widgets.AdjustCoordToUIScalingFloor(min);
                    highlight.xMax = Widgets.AdjustCoordToUIScalingFloor(min + normVal(relativeEnergy) * width);
                }
                else // if current relative qi < qi cost of ability
                {
                    highlight.xMin = Widgets.AdjustCoordToUIScalingFloor(min + normVal(relativeEnergyDiff) * width);
                    highlight.xMax = Widgets.AdjustCoordToUIScalingFloor(min + normVal(relativeEnergy) * width);
                }
                //rect6.xMin = Widgets.AdjustCoordToUIScalingFloor(rect6.xMin + num3 * width);
                //rect6.width = Widgets.AdjustCoordToUIScalingFloor(Mathf.Max(Mathf.Min(num4, 1f) - num3, 0f) * width);
                GUI.color = new Color(1f, 1f, 1f, num2 * 0.7f);
                GenUI.DrawTextureWithMaterial(highlight, GizmoTextureUtility.OverBarTex, null, default(Rect));
                GUI.color = Color.white;
            }

            else // if current relative qi < after using ability
            {
                if (findFloor(relativeEnergy, false) != findFloor(relativeEnergyDiff, false))
                {
                    highlight.xMin = Widgets.AdjustCoordToUIScalingFloor(min + normVal(relativeEnergy, false) * width);
                    highlight.xMax = Widgets.AdjustCoordToUIScalingFloor(max);
                }
                else // if current relative qi < qi cost of ability
                {
                    highlight.xMin = Widgets.AdjustCoordToUIScalingFloor(min + normVal(relativeEnergy, false) * width);
                    highlight.xMax = Widgets.AdjustCoordToUIScalingFloor(Math.Min(min + normVal(relativeEnergyDiff, false) * width, max));
                }
                //rect6.xMin = Widgets.AdjustCoordToUIScalingFloor(rect6.xMin + num3 * width);
                //rect6.width = Widgets.AdjustCoordToUIScalingFloor(Mathf.Max(Mathf.Min(num4, 1f) - num3, 0f) * width);
                GUI.color = new Color(1f, 1f, 1f, num2 * 0.7f);
                GenUI.DrawTextureWithMaterial(highlight, GizmoTextureUtility.UnderBarTex, null, default(Rect));
                GUI.color = Color.white;
            }

        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            StatDef statDef;

            StatRequest pawnReq = StatRequest.For(pawn);

            StatCategoryDef finalCat = tempStatCategory is null ? StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF : tempStatCategory;

            // shows the maximum energy of the whole sorcery schema
            statDef = def.energyMaxStatDef != null ? def.energyMaxStatDef : StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery;
            yield return new StatDrawEntry(finalCat,
                    statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            if (OverMaxEnergy > MaxEnergy) // only show if there's a difference between overmax and max energy.
            {
                statDef = def.energyOverMaxStatDef != null ? def.energyOverMaxStatDef : StatDefOf_ItsSorcery.OverMaxEnergy_ItsSorcery;
                yield return new StatDrawEntry(finalCat,
                        statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);
            }

            if (MinEnergy < 0) // only show if there's a difference between min energy and 0.
            {
                statDef = def.energyMinStatDef != null ? def.energyMinStatDef : StatDefOf_ItsSorcery.MinEnergy_ItsSorcery;
                yield return new StatDrawEntry(finalCat,
                        statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);
            }

            // show recovery amount per second
            statDef = def.energyRecoveryStatDef != null ? def.energyRecoveryStatDef : StatDefOf_ItsSorcery.EnergyRecovery_ItsSorcery;
            yield return new StatDrawEntry(finalCat,
                    statDef, pawn.GetStatValue(statDef) * -1f, pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            // shows a pawn's multiplier on relevant sorcery cost
            statDef = def.energyCostFactorStatDef != null ? def.energyCostFactorStatDef : StatDefOf_ItsSorcery.EnergyCostFactor_ItsSorcery;
            yield return new StatDrawEntry(finalCat,
                    statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            //over and under bar drain multiplier
            yield return new StatDrawEntry(finalCat,
                    "ISF_EnergyTrackerUnderBarFactor".Translate(), (-1f * def.underBarRecoveryFactor).ToStringPercent(),
                    "ISF_EnergyTrackerUnderBarFactorDesc".Translate(), 40, null, null, false);

            yield return new StatDrawEntry(finalCat,
                    "ISF_EnergyTrackerOverBarFactor".Translate(), def.overBarRecoveryFactor.ToStringPercent(),
                    "ISF_EnergyTrackerOverBarFactorDesc".Translate(),
                    30, null, null, false);

        }

        public override string TopRightLabel(SorceryDef sorceryDef)
        {
            return (def.energyLabelKey.Translate().CapitalizeFirst()[0]) + ": " +
                    Math.Round(sorceryDef.statBases.GetStatValueFromList(def.energyUnitStatDef, 0) * this.EnergyCostFactor, 2).ToString();
        }

        public override string DisableCommandReason()
        {
            return def.disableReasonKey ?? "ISF_CommandDisableReasonInvert";
        }


    }
}
