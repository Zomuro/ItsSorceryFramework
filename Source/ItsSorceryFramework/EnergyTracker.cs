﻿using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTracker : IExposable
    {
        public Pawn pawn;

        public EnergyTrackerDef def;

        public SorcerySchema schema;

        public float currentEnergy;

        public List<EnergyTrackerComp> comps;

        public StatCategoryDef tempStatCategory;

        public float cachedEnergyMax = float.MinValue;

        public float cachedEnergyMin = float.MinValue;

        public float cachedEnergyAbsMax = float.MinValue;

        public float cachedEnergyAbsMin = float.MinValue;

        public float cachedEnergyCostFactor = float.MinValue;

        public int nextRecacheTick = -1;

        public Texture2D cachedEmptyBarTex;

        public Texture2D cachedUnderBarTex;

        public Texture2D cachedNormalBarTex;

        public Texture2D cachedOverBarTex;

        //public int loadID = -1;

        public Texture2D EmptyBarTex
        {
            get
            {
                if (cachedEmptyBarTex is null) cachedEmptyBarTex = SolidColorMaterials.NewSolidColorTexture(def.emptyBarColor);
                return cachedEmptyBarTex;
            }
        }

        public Texture2D UnderBarTex
        {
            get
            {
                if (cachedUnderBarTex is null) cachedUnderBarTex = SolidColorMaterials.NewSolidColorTexture(def.underBarColor);
                return cachedUnderBarTex;
            }
        }

        public Texture2D NormalBarTex
        {
            get
            {
                if (cachedNormalBarTex is null) cachedNormalBarTex = SolidColorMaterials.NewSolidColorTexture(def.normalBarColor);
                return cachedNormalBarTex;
            }
        }

        public Texture2D OverBarTex
        {
            get
            {
                if (cachedOverBarTex is null) cachedOverBarTex = SolidColorMaterials.NewSolidColorTexture(def.overBarColor);
                return cachedOverBarTex;
            }
        }

        // initalizer- created via activator via SorcerySchema
        public EnergyTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public EnergyTracker(Pawn pawn, EnergyTrackerDef def, SorcerySchema schema)
        {
            this.pawn = pawn;
            this.def = def;
            this.schema = schema;
            InitializeEnergy();
        }

        public virtual void InitializeComps()
        {
            if (def.comps.NullOrEmpty()) return; // null or empty compproperties = don't run.

            comps = new List<EnergyTrackerComp>();
            foreach (var c in def.comps)
            {
                EnergyTrackerComp energyTrackerComp = null;
                try
                {
                    energyTrackerComp = (EnergyTrackerComp)Activator.CreateInstance(c.compClass);
                    energyTrackerComp.props = c;
                    energyTrackerComp.parent = this;
                    comps.Add(energyTrackerComp);
                }
                catch (Exception arg)
                {
                    Log.Error("[It's Sorcery!] Could not instantiate a EnergyTrackerComp: " + arg);
                    comps.Remove(energyTrackerComp);
                }
            }
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref schema, "schema");
            Scribe_Values.Look(ref currentEnergy, "currentEnergy", 0f, false);

            if (Scribe.mode == LoadSaveMode.LoadingVars) InitializeComps();
            if(!comps.NullOrEmpty()) foreach (var c in comps) c.CompExposeData();
        }

        public List<LearningTracker> LearningTrackers => schema.learningTrackers;

        public float InvMult => def.inverse ? -1f : 1f;

        public StatDef MinEnergyStatDef => def.energyMinStatDef ?? StatDefOf_ItsSorcery.ISF_MinEnergy;

        public StatDef MaxEnergyStatDef => def.energyMaxStatDef ?? StatDefOf_ItsSorcery.ISF_MaxEnergy;

        public StatDef EnergyCostFactorStatDef => def.energyCostFactorStatDef ?? StatDefOf_ItsSorcery.ISF_EnergyCostFactor;

        public virtual float MinEnergy
        {
            get
            {
                if (cachedEnergyMin == float.MinValue) cachedEnergyMin = Math.Min(pawn.GetStatValue(def.energyMinStatDef ?? StatDefOf_ItsSorcery.ISF_MinEnergy, true, -1), MaxEnergy);
                return cachedEnergyMin;
            }
        }

        public virtual float MaxEnergy
        {
            get
            {
                if (cachedEnergyMax == float.MinValue) cachedEnergyMax = pawn.GetStatValue(def.energyMaxStatDef ?? StatDefOf_ItsSorcery.ISF_MaxEnergy, true, -1);
                return cachedEnergyMax;
            }
        }

        public virtual float AbsMinEnergy
        {
            get
            {
                if (cachedEnergyAbsMin == float.MinValue) cachedEnergyAbsMin = def.energyAbsMinStatDef is null ? MinEnergy : Math.Min(pawn.GetStatValue(def.energyAbsMinStatDef, true, -1), MinEnergy);
                return cachedEnergyAbsMin;
            }
        }

        public virtual float AbsMaxEnergy
        {
            get
            {
                if (cachedEnergyAbsMax == float.MinValue) cachedEnergyAbsMax = def.energyAbsMaxStatDef is null ? MaxEnergy : Math.Max(pawn.GetStatValue(def.energyAbsMaxStatDef, true, -1), MaxEnergy);
                return cachedEnergyAbsMax;
            }
        }

        public virtual float EnergyCostFactor
        {
            get
            {
                if (cachedEnergyCostFactor == float.MinValue) cachedEnergyCostFactor = pawn.GetStatValue(def.energyCostFactorStatDef ?? StatDefOf_ItsSorcery.ISF_EnergyCostFactor, true, -1);
                return cachedEnergyCostFactor;
            }
        }

        public virtual bool HasOverchargeZone => !def.inverse ? AbsMaxEnergy > MaxEnergy : MinEnergy > AbsMinEnergy;

        public virtual bool HasDeficitZone => !def.inverse ? MinEnergy > AbsMinEnergy : AbsMaxEnergy > MaxEnergy;

        public virtual bool InOvercharge => HasOverchargeZone && !def.inverse ? currentEnergy > MaxEnergy : currentEnergy < MinEnergy;

        public virtual bool InDeficit => HasDeficitZone && !def.inverse ? currentEnergy < MinEnergy : currentEnergy > MaxEnergy;

        public virtual string EnergyLabel => def.energyUnitStatDef.label;

        public virtual string EnergyDesc => def.energyUnitStatDef.description;

        public virtual void ForceClearEnergyStatCaches()
        {
            ClearStatCache();
            foreach(var comp in comps) comp.CompClearStatCache();
        }

        public virtual void ClearStatCache()
        {
            nextRecacheTick = Find.TickManager.TicksGame + PawnCacheUtility.GetEnergyTickOffset();//UnityEngine.Random.Range(baseTicks - 3, baseTicks + 3);

            if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
            {
                Log.Message($"[It's Sorcery!] {pawn.ThingID}.{schema.def.defName}.{def.defName} clearing cache ; refresh in {nextRecacheTick - Find.TickManager.TicksGame} ticks; prior cached values:" +
                        $"\n{def.energyMinStatDef.defName}: {cachedEnergyMin}" +
                        $"\n{def.energyMaxStatDef.defName}: {cachedEnergyMax}" +
                        $"\n{def.energyAbsMinStatDef?.defName ?? def.energyMinStatDef.defName}: {cachedEnergyAbsMin}" +
                        $"\n{def.energyAbsMaxStatDef?.defName ?? def.energyMaxStatDef.defName}: {cachedEnergyAbsMax}" +
                        $"\n{def.energyCostFactorStatDef.defName}: {cachedEnergyCostFactor}");
            }

            cachedEnergyMin = float.MinValue;
            cachedEnergyMax = float.MinValue;
            cachedEnergyAbsMin = float.MinValue;
            cachedEnergyAbsMax = float.MinValue;
            cachedEnergyCostFactor = float.MinValue;
        }

        public virtual void EnergyTrackerTick()
        {
            if (Find.TickManager.TicksGame >= nextRecacheTick) ClearStatCache();
            if (!comps.NullOrEmpty()) foreach (var c in comps) c.CompPostTick();
        }

        public virtual float EnergyRelativeValue => EnergyToRelativeValue();

        public virtual float RelativeMin => (MinEnergy - AbsMinEnergy) / (AbsMaxEnergy - AbsMinEnergy); // get relative value of min point; say min relval 0.3, curr val is at 0.1

        public virtual float RelativeMax => (MaxEnergy - AbsMinEnergy) / (AbsMaxEnergy - AbsMinEnergy); // get relative value of max point; say max relval = 0.7, curr val at 0.9

        public virtual float EnergyToRelativeValue(float energyCost = 0)
        {
            float tempAdjCurrEnergy = currentEnergy - InvMult * energyCost - AbsMinEnergy; // current energy minus energy cost, downshifted by absolute min energy

            float AdjRange = AbsMaxEnergy - AbsMinEnergy; // absolute max energy downshifted by absolute min energy

            return tempAdjCurrEnergy / AdjRange;
        }

        public virtual bool WouldReachLimitEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            float postEnergy = currentEnergy - InvMult * energyCost;

            if (HasDeficitZone) return !def.inverse ? (postEnergy < MinEnergy && schema.limitLocked) : (postEnergy > MaxEnergy && schema.limitLocked);          
            else return !def.inverse ? (postEnergy < MinEnergy) : (postEnergy > MaxEnergy);
        }

        public virtual bool TryAlterEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (!WouldReachLimitEnergy(energyCost))
            {
                currentEnergy = Mathf.Clamp(currentEnergy - InvMult * energyCost, AbsMinEnergy, AbsMaxEnergy);
                ApplyHediffSeverity(EnergyToRelativeValue());
                return true;
            }

            return false;
        }

        public virtual void AddEnergy(float energyChange, bool normalBounds = false)
        {
            if (!normalBounds) currentEnergy = Mathf.Clamp(currentEnergy + InvMult * energyChange, AbsMinEnergy, AbsMaxEnergy);
            else currentEnergy = Mathf.Clamp(currentEnergy + InvMult * energyChange, MinEnergy, MaxEnergy);
        }

        public virtual void EmptyEnergy()
        {
            if (!def.inverse) currentEnergy = MinEnergy;
            else currentEnergy = MaxEnergy;
        }

        public virtual void InitializeEnergy()
        {
            if (!def.inverse) currentEnergy = MaxEnergy;
            else currentEnergy = MinEnergy;
        }

        public virtual void ApplyHediffSeverity(float relVal) 
        {
            // if there isn't room beyond limits (i.e over max or under min), don't bother
            // no side effect defined = no side effects
            HediffDef hediffDef = def.sideEffect;
            if (hediffDef == null || !HasDeficitZone) return; 

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);

            if (!def.inverse) // if not inverse system
            {
                if (relVal >= RelativeMin || RelativeMin == 0f) return; // side effect has no effect if not lower than min relval
                if (hediff == null) HealthUtility.AdjustSeverity(pawn, hediffDef, 1f - relVal / RelativeMin); // i.e. (1f - 0.1/0.3) = 0.66
                else if (1f - relVal / RelativeMin > hediff.Severity / hediff.def.maxSeverity) hediff.Severity = 1f - relVal / RelativeMin;
            }
            else // else, if inverse system
            {
                if (relVal <= RelativeMax || RelativeMax == 1f) return;
                if (hediff == null) HealthUtility.AdjustSeverity(pawn, hediffDef, (relVal - RelativeMax) / (1f - RelativeMax)); // i.e. (0.9f - 0.7f)/ (1f - 0.7f) = 0.66
                else if ((relVal - RelativeMax) / (1f - RelativeMax) > hediff.Severity / hediff.def.maxSeverity) hediff.Severity = (relVal - RelativeMax) / (1f - RelativeMax);
            }

            // leave warning when pawn reaches the max severity of their side effect hediff
            if (hediff != null && hediff.Severity >= hediff.def.maxSeverity) 
            {
                Messages.Message(def.hitLimitKey.Translate(pawn.Named("PAWN")),
                    pawn, MessageTypeDefOf.NegativeEvent, true);
            }
        }

        public virtual float DrawOnGUI(ref Rect rect, bool addToGizmo = true)
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
            Widgets.LabelCacheHeight(ref labelBox, EnergyLabel.CapitalizeFirst());

            // draws power bar & highlight energy costs
            barBox.height = labelBox.height; // set barbox to labelbox height for consistency
            DrawEnergyBarTip(barBox, addToGizmo); // draw the tipbox when hovering over it

            // include invisible buttons to add/remove to the quick energy gizmo
            if (addToGizmo) AddQuickEnergyEntry(barBox);
            else RemoveQuickEnergyEntry(barBox);

            // adds/removes the energy bar / energy highlight graphic part
            if (ItsSorceryUtility.settings.SchemaShowEnergyBar)
            {
                DrawEnergyBar(barBox);
                HightlightEnergyCost(barBox);
            }

            // draw amount of energy
            string energyLabel = currentEnergy.ToString("F0") + " / " + MaxEnergy.ToString("F0");
            Widgets.Label(barBox, energyLabel);
            Text.Anchor = TextAnchor.UpperLeft;
 
            coordY += labelBox.height; // add label/barbox height
            rect = orgRect; // reset rectangle
            return coordY; // return accumulated height
        }

        public virtual void DrawEnergyBar(Rect rect)
        {
            Widgets.FillableBar(rect, Mathf.Clamp(EnergyRelativeValue, 0f, 1f), CurrBarTex(), EmptyBarTex, true);
            DrawEnergyBarThresholds(rect);
        }

        public virtual void DrawEnergyBarTip(Rect rect, bool addToGizmo = true)
        {
            if (Mouse.IsOver(rect))
            {
                string energy = EnergyLabel.CapitalizeFirst();
                string tipString = "ISF_BarBaseTip".Translate(energy, currentEnergy.ToString("F0"), MinEnergy.ToString("F0"), MaxEnergy.ToString("F0"));
                if (!def.inverse)
                {
                    if (AbsMaxEnergy > MaxEnergy) tipString += "\n" + "ISF_BarOverTip".Translate(MaxEnergy.ToString("F0"), AbsMaxEnergy.ToString("F0"));
                    if (AbsMinEnergy < MinEnergy) tipString += "\n" + "ISF_BarUnderTip".Translate(AbsMinEnergy.ToString("F0"), MinEnergy.ToString("F0"));
                }
                else
                {
                    if (AbsMaxEnergy > MaxEnergy) tipString += "\n" + "ISF_BarUnderTip".Translate(MaxEnergy.ToString("F0"), AbsMaxEnergy.ToString("F0"));
                    if (AbsMinEnergy < MinEnergy) tipString += "\n" + "ISF_BarOverTip".Translate(AbsMinEnergy.ToString("F0"), MinEnergy.ToString("F0"));
                }

                tipString += "\n\n";
                if (addToGizmo) tipString += "ISF_BarAddQuickEnergy".Translate().Colorize(ColoredText.TipSectionTitleColor);
                else tipString += "ISF_BarRemoveQuickEnergy".Translate().Colorize(ColoredText.TipSectionTitleColor);
                TooltipHandler.TipRegion(rect, tipString);
            }
        }

        public virtual void AddQuickEnergyEntry(Rect rect)
        {
            if (Widgets.ButtonInvisible(rect)) SorcerySchemaUtility.AddQuickEnergyEntry(pawn, schema, this);
        }

        public virtual void RemoveQuickEnergyEntry(Rect rect)
        {
            if (Widgets.ButtonInvisible(rect)) SorcerySchemaUtility.RemoveQuickEnergyEntry(pawn, schema, this);
        }

        public virtual void DrawEnergyBarThresholds(Rect rect)
        {
            Color tempColor = GUI.color;
            float thresWidth = 2f; //rect.width > 60f ? 2f : 1f;
            Texture2D image;

            if(RelativeMin > 0f)
            {
                Rect positionMin = new Rect(rect.x + rect.width * RelativeMin - (thresWidth - 1f), rect.y + rect.height / 2f, thresWidth, rect.height / 2f);
                if (RelativeMin < EnergyRelativeValue)
                {
                    image = BaseContent.BlackTex;
                    GUI.color = new Color(1f, 1f, 1f, 0.9f);
                }
                else
                {
                    image = BaseContent.GreyTex;
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                }
                GUI.DrawTexture(positionMin, image);
            }

            if (RelativeMax < 1f)
            {
                Rect positionMax = new Rect(rect.x + rect.width * RelativeMax - (thresWidth - 1f), rect.y + rect.height / 2f, thresWidth, rect.height / 2f);
                if (RelativeMax < EnergyRelativeValue)
                {
                    image = BaseContent.BlackTex;
                    GUI.color = new Color(1f, 1f, 1f, 0.9f);
                }
                else
                {
                    image = BaseContent.GreyTex;
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                }
                GUI.DrawTexture(positionMax, image);
            }
           
            GUI.color = tempColor;
        }

        public Texture2D CurrBarTex()
        {
            if (!def.inverse) // if energy typically goes from 100 -> 75
            {
                if (EnergyRelativeValue > RelativeMax) return OverBarTex;
                else if (EnergyRelativeValue < RelativeMin) return UnderBarTex;
                else return NormalBarTex;
            }
            else // if energy typically goes from 0 -> 25
            {
                if (EnergyRelativeValue > RelativeMax) return UnderBarTex;
                else if (EnergyRelativeValue < RelativeMin) return OverBarTex;
                else return NormalBarTex;
            }
        }

        public virtual void HightlightEnergyCost(Rect rect)
        {
            // return if hovered gizmo is null
            Command_Sorcery command_Sorcery = MapGizmoUtility.LastMouseOverGizmo as Command_Sorcery;
            if (command_Sorcery == null) return;

            // return if it isn't a sorceryDef or isn't the same energytracker
            SorceryDef sorceryDef = (command_Sorcery?.Ability as Sorcery)?.sorceryDef;
            if (sorceryDef == null || sorceryDef.sorcerySchema != schema.def || !sorceryDef.sorcerySchema.energyTrackerDefs.Contains(def)) return;

            float energyCost = sorceryDef.statBases.GetStatValueFromList(def.energyUnitStatDef, 0) * EnergyCostFactor;
            if (energyCost == 0f) return; // no energy cost? don't bother with showing it!

            float lowerRelVal = Mathf.Min(EnergyRelativeValue, EnergyToRelativeValue(energyCost));
            float higherRelVal = Mathf.Max(EnergyRelativeValue, EnergyToRelativeValue(energyCost));

            // used to make random blinking effect on highlight
            float loopingNum = Mathf.Repeat(Time.time, 0.85f);
            float alphaVal = 1f;
            if (loopingNum < 0.1f) alphaVal = loopingNum / 0.1f;
            else if (loopingNum >= 0.25f) alphaVal = 1f - (loopingNum - 0.25f) / 0.6f;

            Rect highlight = rect.ContractedBy(3f);
            //float max = highlight.xMax;
            float min = highlight.xMin;
            float width = highlight.width;

            highlight.xMin = UIScaling.AdjustCoordToUIScalingFloor(min + Mathf.Clamp(lowerRelVal, 0f, 1f) * width);
            highlight.xMax = UIScaling.AdjustCoordToUIScalingFloor(min + Mathf.Clamp(higherRelVal, 0f, 1f) * width);

            // if not inverse, check if energy after cast is less than current energy
            // else if inverse, check if energy after cast is more than current energy
            GUI.color = new Color(1f, 1f, 1f, alphaVal * 0.7f);
            GenUI.DrawTextureWithMaterial(highlight, !def.inverse == (EnergyRelativeValue > EnergyToRelativeValue(energyCost)) ? UnderBarTex : OverBarTex, null, default(Rect));
            GUI.color = Color.white;
        }

        public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            StatDef statDef;
            StatRequest pawnReq = StatRequest.For(pawn);
            StatCategoryDef finalCat = tempStatCategory ?? StatCategoryDefOf_ItsSorcery.ISF_EnergyTracker;

            int displayPriority = 100;

            // shows what the "energy" is
            yield return new StatDrawEntry(finalCat,
                        "ISF_EnergyTrackerUnit".Translate(), EnergyLabel.CapitalizeFirst(),
                        EnergyDesc, displayPriority, null, null, false);
            displayPriority--;

            // shows whether or not energy is loaded/gained inversely
            yield return new StatDrawEntry(finalCat,
                    "ISF_EnergyTrackerInverse".Translate(), def.inverse ? "Inverted" : "Normal",
                    "ISF_EnergyTrackerInverseDesc".Translate(), displayPriority, null, null, false);
            displayPriority--;

            // shows the maximum energy of the whole sorcery schema
            if (AbsMaxEnergy > MaxEnergy) // only show if there's a difference between overmax and max energy.
            {
                statDef = def.energyAbsMaxStatDef ?? StatDefOf_ItsSorcery.ISF_AbsMaxEnergy;
                yield return new StatDrawEntry(finalCat,
                        statDef, pawn.GetStatValue(statDef, true, -1), pawnReq, statDef.toStringNumberSense, displayPriority, false);
                displayPriority--;
            }

            statDef = def.energyMaxStatDef ?? StatDefOf_ItsSorcery.ISF_MaxEnergy;
            yield return new StatDrawEntry(finalCat,
                    statDef, pawn.GetStatValue(statDef, true, -1), pawnReq, statDef.toStringNumberSense, displayPriority, false);
            displayPriority--;

            statDef = def.energyMinStatDef ?? StatDefOf_ItsSorcery.ISF_MinEnergy;
            yield return new StatDrawEntry(finalCat,
                    statDef, pawn.GetStatValue(statDef, true, -1), pawnReq, statDef.toStringNumberSense, displayPriority, false);
            displayPriority--;

            if (AbsMinEnergy < MinEnergy) // only show if there's a difference between min energy and 0.
            {
                statDef = def.energyAbsMinStatDef ?? StatDefOf_ItsSorcery.ISF_AbsMinEnergy;
                yield return new StatDrawEntry(finalCat,
                        statDef, pawn.GetStatValue(statDef, true, -1), pawnReq, statDef.toStringNumberSense, displayPriority, false);
                displayPriority--;
            }

            // shows a pawn's multiplier on relevant sorcery cost
            statDef = def.energyCostFactorStatDef ?? StatDefOf_ItsSorcery.ISF_EnergyCostFactor;
            yield return new StatDrawEntry(finalCat,
                    statDef, pawn.GetStatValue(statDef, true, -1), pawnReq, statDef.toStringNumberSense, displayPriority, false);
            displayPriority--;
        }

        public virtual string TopRightLabel(SorceryDef sorceryDef)
        {
            return "{0}: {1}".Translate(EnergyLabel.CapitalizeFirst()[0], 
                Math.Round(sorceryDef.statBases.GetStatValueFromList(def.energyUnitStatDef, 0) * EnergyCostFactor, 2).ToString());
        }

        public virtual string DisableCommandReason() => def.disableReasonKey ?? (def.inverse ? "ISF_CommandDisableReasonInvert" : "ISF_CommandDisableReasonBase");

        public override string ToString() => "Energy class: " + GetType().Name.ToString();
    }
}
