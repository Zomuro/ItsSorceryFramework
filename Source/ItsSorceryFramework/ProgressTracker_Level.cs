using RimWorld;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTracker_Level : ProgressTracker
    {
        // UI fields
         private Vector2 leftDescScrollPosition = Vector2.zero;

        private Vector2 leftStatusScrollPosition = Vector2.zero;

        private Vector2 modScrollPosition = Vector2.zero;

        private Vector2 expScrollPosition = Vector2.zero;

        private Vector2 energyScrollPosition = Vector2.zero;

        private Vector2 sorceryScrollPosition = Vector2.zero;

        private float leftDescScrollViewHeight;

        private float leftStatusScrollViewHeight;

        private float modScrollViewHeight;

        private float expScrollViewHeight;

        private float energyScrollViewHeight;

        private float sorceryScrollViewHeight;

        // initalizer- created via activator via SorcerySchema
        public ProgressTracker_Level(Pawn pawn) : base(pawn) { }

        public ProgressTracker_Level(Pawn pawn, ProgressTrackerDef def, SorcerySchema schema) : base(pawn, def, schema)
        {
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            if(pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff) == null)
                HealthUtility.AdjustSeverity(pawn, def.progressHediff, def.progressHediff.initialSeverity);
            hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff) as Hediff_ProgressLevel;
            hediff.progressTracker = this;
            SetupHediffStage(hediff as Hediff_ProgressLevel);
        }

        public override void ExposeData() => base.ExposeData();

        public override void ProgressTrackerTick()
        {
            if(Find.TickManager.TicksGame % 60 == 0)
            {               
                if (def.Workers.EnumerableNullOrEmpty()) return;
                foreach (var worker in def.Workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_Passive) || 
                    x.GetType() == typeof(ProgressEXPWorker_DuringJob)))
                {
                    worker.TryExecute(this);
                }
            }
        }

        public override void AddExperience(float experience)
        {
            float orgSev = CurrLevel;
            bool done = false;
            exp += experience;

            while (!done)
            {
                if (Maxed) break;
                if (exp > CurrentLevelEXPReq)
                {
                    exp -= CurrentLevelEXPReq;
                    hediff.Severity += 1;
                    NotifyLevelUp(hediff.Severity);
                }
                else done = true;
            }

            if(CurrLevel > orgSev) NotifyTotalLevelUp(orgSev);
        }

        public override void ForceLevelUp()
        {
            if (hediff == null || Maxed) return;
            hediff.Severity += 1;
            NotifyLevelUp(hediff.Severity);
        }

        public override void NotifyLevelUp(float sev)
        {
            ProgressLevelModifier factor = def.getLevelFactor(sev);
            if (factor != null)
            {
                AdjustModifiers(factor);
                AdjustAbilities(factor);
                AdjustHediffs(factor);
                points += factor.pointGain;
                ApplyOptions(factor);
            }

            ProgressLevelModifier special = def.getLevelSpecific(sev);
            if (special != null)
            {
                AdjustModifiers(special);
                AdjustAbilities(special);
                AdjustHediffs(special);
                points += special.pointGain;
                ApplyOptions(special);
            }

            hediff.curStage = RefreshCurStage();
        }

        public override HediffStage RefreshCurStage()
        {

            HediffStage stage = new HediffStage()
            {
                statOffsets = CreateStatModifiers(statOffsetsTotal).ToList(),
                statFactors = CreateStatModifiers(statFactorsTotal).ToList(),
                capMods = CreateCapModifiers(capModsTotal).ToList()
            };

            return stage;
        }

        public override void NotifyTotalLevelUp(float orgSev)
        {
            Find.LetterStack.ReceiveLetter(def.progressLevelUpKey.Translate(pawn.Name.ToStringShort),
                def.progressLevelUpDescKey.Translate(orgSev.ToString(), CurrLevel.ToString()), LetterDefOf.NeutralEvent, null);
        }

        public override float CurrProgress => exp / CurrentLevelEXPReq;

        public override float CurrentLevelEXPReq => def.baseEXP * Mathf.Pow(def.scaling, CurrLevel - 1f);

        public override void DrawLeftGUI(Rect rect)
        {
            Widgets.BeginGroup(rect);

            Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, rect.height);

            float coordY = 0f;

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            // SCHEMA //
            Rect labelRect = new Rect(0f, coordY, viewRect.width, 50f);
            Widgets.LabelCacheHeight(ref labelRect, schema.def.LabelCap, true, false);
            coordY += labelRect.height;

            // LEVEL //
            Rect lvlRect = new Rect(0f, coordY, viewRect.width, 50f);
            Text.Font = GameFont.Small;
            if (CurLevelLabel.NullOrEmpty()) Widgets.LabelCacheHeight(ref lvlRect, "ISF_LearningLevelLabel".Translate(CurrLevel), true, false);
            else
            {
                if (Maxed) Widgets.LabelCacheHeight(ref lvlRect,
                        "ISF_LearningLevelLabelCustom".Translate(CurLevelLabel, "ISF_LearningLevelLabelMax".Translate()), true, false);
                else Widgets.LabelCacheHeight(ref lvlRect,
                        "ISF_LearningLevelLabelCustom".Translate(CurLevelLabel, CurrLevel), true, false);
            }
            coordY += lvlRect.height;

            // XP BAR //
            Rect xpBar = new Rect(0f, coordY + 10, rect.width, 35f);
            if (Maxed) // if at max level, full xp bar + Maxed
            {
                Widgets.FillableBar(xpBar, 1);
                Text.Font = GameFont.Medium;
                Widgets.Label(xpBar, "ISF_LearningLevelLabelMax".Translate());
            }
            else // normal function
            {
                Widgets.FillableBar(xpBar, CurrProgress);
                Text.Font = GameFont.Medium;
                Widgets.Label(xpBar, exp.ToString("F0") + " / " + CurrentLevelEXPReq.ToString("F0"));
            }

            coordY += xpBar.height * 1.5f;

            // DESCRIPTION //
            GenUI.ResetLabelAlign();
            
            Text.Font = GameFont.Medium; // Description LABEL
            Rect descTitleRect = new Rect(0f, coordY, viewRect.width, 0f);
            Widgets.LabelCacheHeight(ref descTitleRect, "ISF_LearningProgressLevelDescription".Translate(), true, false);
            coordY += descTitleRect.height;

            Text.Font = GameFont.Small; // Description DESCRIPTION
            Rect descOuterRect = new Rect(0f, coordY, outRect.width, outRect.height / 2f - coordY);
            Rect descViewRect = new Rect(0f, coordY, viewRect.width, leftDescScrollViewHeight); //leftDescScrollViewHeight
            Widgets.BeginScrollView(descOuterRect, ref leftDescScrollPosition, descViewRect, true);
            Widgets.LabelCacheHeight(ref descViewRect, schema.def.description, true, false);
            leftDescScrollViewHeight = descViewRect.height;
            Widgets.EndScrollView();
            coordY += descOuterRect.height;

            coordY += xpBar.height * 0.5f;

            // STATUS //
            Text.Font = GameFont.Medium; // STATUS LABEL
            Rect modTitleRect = new Rect(0f, coordY, viewRect.width, 0f);
            Widgets.LabelCacheHeight(ref modTitleRect, "ISF_LearningProgressLevelStatus".Translate(), true, false);
            coordY += modTitleRect.height;

            String tipString = TipStringExtra(hediff.CurStage);
            if (tipString.NullOrEmpty()) tipString = "N/A";

            Text.Font = GameFont.Small; // Status info
            Rect statusOuterRect = new Rect(0f, coordY, outRect.width, outRect.height - coordY);
            Rect statusViewRect = new Rect(0f, coordY, viewRect.width, leftStatusScrollViewHeight); //leftDescScrollViewHeight
            Widgets.BeginScrollView(statusOuterRect, ref leftStatusScrollPosition, statusViewRect, true);
            Widgets.LabelCacheHeight(ref statusViewRect, tipString, true, false);
            leftStatusScrollViewHeight = statusViewRect.height;
            Widgets.EndScrollView();

            //coordY += statusOuterRect.height;
            Widgets.EndGroup();
        }

        public override void DrawRightGUI(Rect rect)
        {
            Widgets.BeginGroup(rect);
            Rect leftHalf = new Rect(0, 0, rect.width / 2f, rect.height);

            // PROSPECTS - future modifiers w/ level up //
            Rect modRect = new Rect(0, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            modRect.height += 20f;
            Rect modRectView = new Rect(modRect.x, modRect.y, modRect.width - 20f, modScrollViewHeight);
            Widgets.BeginScrollView(modRect, ref this.modScrollPosition, modRectView, true);

            float coordY = 0f;
            Rect allModRect = new Rect(modRectView.x, modRectView.y + coordY, modRectView.width, 500f);
            coordY += DrawModifiers(allModRect);
            modScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // EXPERIENCE - show expgain //
            Rect expRect = new Rect(rect.width / 4f, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            expRect.height += 20f;
            Rect expRectView = new Rect(expRect.x, expRect.y, expRect.width - 20f, expScrollViewHeight);
            Widgets.BeginScrollView(expRect, ref this.expScrollPosition, expRectView, true);

            coordY = 0f;
            Rect allEXPRect = new Rect(expRectView.x, expRectView.y + coordY, expRectView.width, 500f);
            coordY += DrawEXPMethods(allEXPRect);
            expScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // ENERGY - show energytrackers //
            Rect energyRect = new Rect(rect.width / 2f, 0, rect.width / 4f, rect.height).ContractedBy(20f);
            //energyRect.height += 20f;
            Rect energyRectView = new Rect(energyRect.x, energyRect.y, energyRect.width - 20f, energyScrollViewHeight);
            Widgets.BeginScrollView(energyRect, ref this.energyScrollPosition, energyRectView, true);

            coordY = 0f;
            Rect allenergyRect = new Rect(energyRectView.x, energyRectView.y + coordY, energyRectView.width, 500f);
            coordY += DrawEnergyComps(allenergyRect);
            energyScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // SORCERIES - see sorceries and change out what you want to use //
            Rect sorceryRect = new Rect(0, rect.height * 2f / 3f, rect.width / 2f, rect.height * 3f).ContractedBy(20f);
            sorceryRect.height += 20f;
            Rect sorceryRectView = new Rect(sorceryRect.x, sorceryRect.y, sorceryRect.width - 20f, sorceryScrollViewHeight);
            Widgets.BeginScrollView(sorceryRect, ref this.sorceryScrollPosition, sorceryRectView, true);

            coordY = 0f;
            Rect allSorceryRect = new Rect(sorceryRectView.x, sorceryRectView.y + coordY, sorceryRectView.width, 500f);
            coordY += DrawSorceries(allSorceryRect);
            sorceryScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // IMAGE - decorative banner //
            Rect bannerRect = new Rect(rect.width * 3f/4f, 0, rect.width / 4f, rect.height);          
            Widgets.DrawTextureFitted(bannerRect.ContractedBy(10f), def.BGIcon, 1f);
            Widgets.EndGroup();
        }

        public override float DrawModifiers(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevelProspects".Translate(), true, false);
            rect.yMin += rect.height;
            rect.x += 22f;

            String tipString;
            String tipString2;
            float projLevel = CurrLevel + 1;

            if (projLevel > hediff.def.maxSeverity) return rect.yMin - yMin;
            Text.Font = GameFont.Small;
            for (int i = (int)projLevel; i < (int)projLevel + 5; i++)
            {
                if (i > hediff.def.maxSeverity) break;

                ProgressLevelModifier factor = def.getLevelFactor(i);
                tipString = TipStringExtra(factor);
                ProgressLevelModifier special = def.getLevelSpecific(i);
                tipString2 = TipStringExtra(special);

                if (tipString.NullOrEmpty() && !HyperlinkCheck(factor) &&
                    tipString2.NullOrEmpty() && !HyperlinkCheck(special)) continue;

                Text.Font = GameFont.Small;
                //Text.Font = GameFont.Medium;
                if (CurLevelLabel.NullOrEmpty())
                    Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevel".Translate(i).Colorize(ColoredText.TipSectionTitleColor), true, false);
                else
                    Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevelCustom".Translate(i, GetProgressLevelLabel(i)).Colorize(ColoredText.TipSectionTitleColor), true, false);
                rect.yMin += rect.height;
                //Text.Font = GameFont.Small;

                Rect hyperlinkRect;

                if (!tipString.NullOrEmpty() || HyperlinkCheck(factor))
                {
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref rect, "Normal".Colorize(ColoredText.SubtleGrayColor), true, false);
                    rect.yMin += rect.height;

                    Widgets.LabelCacheHeight(ref rect, tipString, true, false);
                    rect.yMin += rect.height;

                    rect.xMin += 6f;
                    hyperlinkRect = new Rect(rect.x, rect.yMin, rect.width, 500f);
                    rect.yMin += this.DrawHyperlinks(hyperlinkRect, factor);
                    rect.xMin -= 6f;
                }
                if (!tipString2.NullOrEmpty() || HyperlinkCheck(special))
                {
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref rect, "Special".Colorize(ColoredText.SubtleGrayColor), true, false);
                    rect.yMin += rect.height;

                    Widgets.LabelCacheHeight(ref rect, tipString2, true, false);
                    rect.yMin += rect.height;

                    rect.xMin += 6f;
                    hyperlinkRect = new Rect(rect.x, rect.yMin, rect.width, 500f);
                    rect.yMin += this.DrawHyperlinks(hyperlinkRect, special);
                    rect.xMin -= 6f;
                }

            }

            return rect.yMin - yMin;
        }

        public override float DrawEnergyComps(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            /*Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "Energy", true, false);
            rect.yMin += rect.height;*/
            //Text.Font = GameFont.Small;
            //rect.xMin += 22f;

            foreach(var et in schema.energyTrackers) // for each energy tracker
            {
                // write energy label
                rect.xMin = x;
                Text.Font = GameFont.Medium;
                Widgets.LabelCacheHeight(ref rect, et.EnergyLabel.CapitalizeFirst(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 22f;

                // write out the detailed info within energycomps
                Text.Font = GameFont.Small;
                if (et.comps.NullOrEmpty()) // no comps?
                {
                    Widgets.LabelCacheHeight(ref rect, "N/A", true, false); // leave N/A
                    rect.yMin += rect.height;
                    continue; //skip this energy tracker
                }

                // else put all the comp information onto the UI.
                foreach(var comp in et.comps) rect.yMin += comp.CompDrawGUI(rect);
            }

            return rect.yMin - yMin;
        }
    }
}
