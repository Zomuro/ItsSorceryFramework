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
        
        //public SorcerySchemaDef sorcerySchemaDef;

        //private SorcerySchema cachedSchema;

        //private List<LearningTracker> cachedLearningTrackers = new List<LearningTracker>();

        public float currentEnergy;

        public List<EnergyTrackerComp> comps;

        public StatCategoryDef tempStatCategory;

        public Texture2D cachedEmptyBarTex;

        public Texture2D cachedUnderBarTex;

        public Texture2D cachedNormalBarTex;

        public Texture2D cachedOverBarTex;

        public int loadID = -1;

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

            // maybe put initalize gizmo here idunno
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
                    Log.Error("Could not instantiate or initialize a EnergyTrackerComp: " + arg);
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

        public SorcerySchema Schema => schema;

        public List<LearningTracker> LearningTrackers => schema.learningTrackers;

        public float InvMult => def.inverse ? -1f : 1f;

        public virtual bool HasLimit => HasDeficitZone;

        public virtual float MinEnergy => Math.Min(pawn.GetStatValue(def.energyMinStatDef ?? StatDefOf_ItsSorcery.MinEnergy_ItsSorcery, true), MaxEnergy);

        public virtual float MaxEnergy => pawn.GetStatValue(def.energyMaxStatDef ?? StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery, true);

        public virtual float AbsMinEnergy => def.energyAbsMinStatDef is null ? MinEnergy : Math.Min(pawn.GetStatValue(def.energyAbsMinStatDef, true), MinEnergy);

        public virtual float AbsMaxEnergy => def.energyAbsMaxStatDef is null ? MaxEnergy : Math.Max(pawn.GetStatValue(def.energyAbsMaxStatDef, true), MaxEnergy);

        public virtual float EnergyRecoveryRate => 5f;

        public virtual float EnergyCostFactor => pawn.GetStatValue(def.energyCostFactorStatDef ?? StatDefOf_ItsSorcery.EnergyCostFactor_ItsSorcery, true);

        public virtual float CastFactor => pawn.GetStatValue(def.castFactorStatDef ?? StatDefOf_ItsSorcery.CastFactor_ItsSorcery, true);

        public virtual bool HasOverchargeZone => !def.inverse ? AbsMaxEnergy > MaxEnergy : MinEnergy > AbsMinEnergy;

        public virtual bool HasDeficitZone => !def.inverse ? MinEnergy > AbsMinEnergy : AbsMaxEnergy > MaxEnergy;

        public virtual bool InDeficit => HasDeficitZone && !def.inverse ? currentEnergy < MinEnergy : currentEnergy > MaxEnergy;

        public virtual bool InOvercharge => HasOverchargeZone && !def.inverse ? currentEnergy > MaxEnergy : currentEnergy < MinEnergy;

        public virtual string EnergyLabel => def.energyLabelKey.Translate();

        public virtual void EnergyTrackerTick()
        {
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
            return !def.inverse ? (currentEnergy - InvMult * energyCost < MinEnergy && Schema.limitLocked) : (currentEnergy - InvMult * energyCost > MaxEnergy && Schema.limitLocked);
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
            if (hediffDef == null || !HasLimit) return; 

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
                Messages.Message("ISF_MessagePastLimit".Translate(pawn.Named("PAWN")),
                    pawn, MessageTypeDefOf.NegativeEvent, true);
            }
        }

        public virtual void DrawOnGUI(Rect rect) {}

        public virtual float DrawOnGUI(ref Rect rect)
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
            string energyLabel = currentEnergy.ToString("F0") + " / " + MaxEnergy.ToString("F0");
            Widgets.Label(barBox, energyLabel);
            Text.Anchor = TextAnchor.UpperLeft;

            // highlight energy costs
            HightlightEnergyCost(barBox);

            // add label/barbox height
            coordY += labelBox.height;
            // reset rectangle
            rect = orgRect;
            // return accumulated height
            return coordY;
        }

        public virtual void DrawEnergyBar(Rect rect)
        {
            Widgets.FillableBar(rect, Mathf.Clamp(EnergyRelativeValue, 0f, 1f), CurrBarTex(), EmptyBarTex, true);
            DrawEnergyBarThresholds(rect);

            if (Mouse.IsOver(rect))
            {
                string energy = def.energyLabelKey.Translate().CapitalizeFirst();
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
                TooltipHandler.TipRegion(rect, tipString);
            }
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
            // return if tabwindow is null
            MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
            if (mainTabWindow_Inspect == null) return;

            // return if hovered gizmo is null
            Command_Sorcery command_Sorcery = ((mainTabWindow_Inspect != null) ? mainTabWindow_Inspect.LastMouseoverGizmo : null) as Command_Sorcery;
            if (command_Sorcery == null) return;

            // return if it isn't a sorceryDef or isn't the same energytracker
            SorceryDef sorceryDef = (command_Sorcery?.Ability as Sorcery)?.sorceryDef;
            if (sorceryDef == null || !sorceryDef.sorcerySchema.energyTrackerDefs.Contains(def)) return;

            float energyCost = sorceryDef.statBases.GetStatValueFromList(def.energyUnitStatDef, 0) * EnergyCostFactor;
            if (energyCost == 0f) return; // no energy cost? don't bother with showing it!

            /* float relativeEnergyDiff = EnergyToRelativeValue(energyCost);
            float relativeEnergy = EnergyRelativeValue;*/

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

            highlight.xMin = Widgets.AdjustCoordToUIScalingFloor(min + Mathf.Clamp(lowerRelVal, 0f, 1f) * width);
            highlight.xMax = Widgets.AdjustCoordToUIScalingFloor(min + Mathf.Clamp(higherRelVal, 0f, 1f) * width);

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
            StatCategoryDef finalCat = tempStatCategory ?? StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF;

            int displayPriority = 100;

            // shows what the "energy" is
            yield return new StatDrawEntry(finalCat,
                        "ISF_EnergyTrackerUnit".Translate(), def.energyLabelKey.Translate().CapitalizeFirst(),
                        def.energyDescKey.Translate(), displayPriority, null, null, false);
            displayPriority--;

            // shows whether or not energy is loaded/gained inversely
            yield return new StatDrawEntry(finalCat,
                    def.inverseLabelKey.Translate(), def.inverse ? "Inverted" : "Normal",
                    def.inverseDescKey.Translate(), displayPriority, null, null, false);
            displayPriority--;

            // shows the maximum energy of the whole sorcery schema
            if (AbsMaxEnergy > MaxEnergy) // only show if there's a difference between overmax and max energy.
            {
                statDef = def.energyAbsMaxStatDef ?? StatDefOf_ItsSorcery.AbsMaxEnergy_ItsSorcery;
                yield return new StatDrawEntry(finalCat,
                        statDef, AbsMaxEnergy, pawnReq, ToStringNumberSense.Undefined, displayPriority, false);
                displayPriority--;
            }

            statDef = def.energyMaxStatDef ?? StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery;
            yield return new StatDrawEntry(finalCat,
                    statDef, MaxEnergy, pawnReq, ToStringNumberSense.Undefined, displayPriority, false);
            displayPriority--;

            statDef = def.energyMinStatDef ?? StatDefOf_ItsSorcery.MinEnergy_ItsSorcery;
            yield return new StatDrawEntry(finalCat,
                    statDef, MinEnergy, pawnReq, ToStringNumberSense.Undefined, displayPriority, false);
            displayPriority--;

            if (AbsMinEnergy < MinEnergy) // only show if there's a difference between min energy and 0.
            {
                statDef = def.energyAbsMinStatDef ?? StatDefOf_ItsSorcery.AbsMinEnergy_ItsSorcery;
                yield return new StatDrawEntry(finalCat,
                        statDef, AbsMinEnergy, pawnReq, ToStringNumberSense.Undefined, displayPriority, false);
                displayPriority--;
            }

            // shows a pawn's multiplier on relevant sorcery cost
            statDef = def.energyCostFactorStatDef ?? StatDefOf_ItsSorcery.EnergyCostFactor_ItsSorcery;
            yield return new StatDrawEntry(finalCat,
                    statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Factor, displayPriority, false);
            displayPriority--;
        }

        public virtual string TopRightLabel(SorceryDef sorceryDef)
        {
            return "{0}: {1}".Translate(def.energyLabelKey.Translate().CapitalizeFirst()[0], 
                Math.Round(sorceryDef.statBases.GetStatValueFromList(def.energyUnitStatDef, 0) * EnergyCostFactor, 2).ToString());
        }

        public virtual string DisableCommandReason() => def.disableReasonKey ?? "ISF_CommandDisableReasonBase";

        public override string ToString() => "Energy class: " + GetType().Name.ToString();
    }
}
