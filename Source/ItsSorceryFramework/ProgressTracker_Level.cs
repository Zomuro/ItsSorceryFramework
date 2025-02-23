using RimWorld;
using System;
using System.Collections.Generic;
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

        public ProgressTracker_Level(Pawn pawn, ProgressTrackerDef def, SorcerySchema schema) : base(pawn, def, schema) { }

        public ProgressTracker_Level(Pawn pawn, ProgressTrackerDef def, SorcerySchema schema, ProgressTrackerClassDef classDef) : base(pawn, def, schema, classDef) { }

        public override void Initialize()
        {
            base.Initialize();
            ResetHediff(); // used to create progress tracker's hediff during initialization... or when hediff is missing
        }

        public override void ExposeData() => base.ExposeData();

        // set as a wrapper and null check method?
        public override Hediff_Progress Hediff
        {
            get
            {
                if (hediff is null) ResetHediff(); // missing hediff => recreate hediff w/ proper curstage; existing hediff; relink
                return hediff;
            }
            set { hediff = value; }
        }

        public override void ResetHediff()
        {
            // if the progress hediff already exists:
            if (pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff) != null)
            {
                Hediff_ProgressLevel existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff) as Hediff_ProgressLevel;
                existingHediff.Schema = schema; // ensure the hediff is linked to this progresstracker's schema
                Hediff = existingHediff; // link the progresstracker to the hediff on the progress tracker's side
            }

            // else, we are going to create it
            else
            {
                Hediff_ProgressLevel tempHediff = HediffMaker.MakeHediff(def.progressHediff, pawn, null) as Hediff_ProgressLevel; // define hediff w/ proper class
                tempHediff.Severity = def.progressHediff.initialSeverity; // set initial severity
                tempHediff.Schema = schema; // ensure the hediff is linked to this progresstracker's schema
                pawn.health.AddHediff(tempHediff, null, null, null); // add to pawn
                Hediff = tempHediff; // link the progresstracker to the hediff on the progress tracker's side
            }

            // finally, (re)set the hediffstage of the hediff
            Hediff.cachedCurStage = RefreshCurStage();
        }

        public override void ProgressTrackerTick()
        {
            if(Find.TickManager.TicksGame % 60 == 0)
            {
                // def.Workers
                if (currClassDef.Workers.EnumerableNullOrEmpty()) return;
                foreach (var worker in currClassDef.Workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_Passive) || 
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

            List<Window> optionWindows = new List<Window>();

            while (!done)
            {
                if (Maxed) break;
                if (exp > CurrentLevelEXPReq)
                {
                    exp -= CurrentLevelEXPReq;
                    CurrLevel += 1;
                    NotifyLevelUp(CurrLevel, ref optionWindows); // get benefits of level up + add windows
                }
                else done = true;
            }

            if(CurrLevel > orgSev) NotifyTotalLevelUp(orgSev, optionWindows); // notify total level up and add windows
        }

        public override void ForceLevelUp(int levels, bool silent_msg = false)
        {
            if (Hediff == null || Maxed) return;
            float orgSev = CurrLevel;
            int level_inc = 0;

            List<Window> windows = new List<Window>();
            while (levels > level_inc)
            {
                if (Maxed) break;
                CurrLevel += 1;
                NotifyLevelUp(CurrLevel, ref windows); // level up + get 
                level_inc++;
            }

            /*List<Window> windows = new List<Window>();
            CurrLevel += 1;
            NotifyLevelUp(CurrLevel, ref windows); */
            NotifyTotalLevelUp(orgSev, windows, silent_msg); // notify level up + get windows
        }

        public override void NotifyLevelUp(float sev, ref List<Window> windows)
        {
            // begin the new log here
            ProgressDiffLedger progressDiffLedger = progressDiffLog.PrepNewLedger(this);
            ProgressDiffClassLedger progressDiffClassLedger = new ProgressDiffClassLedger();

            // for factors of the level
            ProgressLevelModifier factor = currClassDef.GetLevelFactor(sev);  // def.GetLevelFactor(sev);
            if (factor != null)
            {
                // debug statement about levelmodifier's options
                if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug && !factor.options.NullOrEmpty())
                    Log.Message($"[It's Sorcery!] Level {CurrLevel} has {factor.options.Count} factor options to choose from; picking {factor.optionChoices}");

                // adjust modifiers
                AdjustModifiers(factor, ref progressDiffClassLedger);
                AdjustAbilities(factor, ref progressDiffClassLedger);
                AdjustHediffs(factor, ref progressDiffClassLedger);
                points += factor.pointGain;
                ApplyOptions(factor, ref windows, ref progressDiffClassLedger);
            }

            // for that specific level
            ProgressLevelModifier special = currClassDef.GetLevelSpecific(sev);  // def.GetLevelSpecific(sev);
            if (special != null)
            {
                // debug statement about levelmodifier's options
                if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug && !special.options.NullOrEmpty())
                    Log.Message($"[It's Sorcery!] Level {CurrLevel} has {special.options.Count} special options to choose from; picking {special.optionChoices}");

                // adjust modifiers
                AdjustModifiers(special, ref progressDiffClassLedger);
                AdjustAbilities(special, ref progressDiffClassLedger);
                AdjustHediffs(special, ref progressDiffClassLedger);
                ApplyUnlocks(special); // only for modifiers within special
                ApplyClasses(special);
                points += special.pointGain;
                ApplyOptions(special, ref windows, ref progressDiffClassLedger);
            }

            // debug validating that hediff and progresstracker modifiers are the same
            if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
            {
                Log.Message($"[It's Sorcery!] {schema.def.label} Modifier Validation:" +
                        $"\nProgressTracker offets: {statOffsetsTotal.ToStringSafeEnumerable()}" +
                        $"\nProgressTracker factors: {statFactorsTotal.ToStringSafeEnumerable()}" +
                        $"\nProgressTracker cap mods: {capModsTotal.ToStringSafeEnumerable()}" +
                        $"\nHediff ProgressTracker offets: {Hediff.Schema.progressTracker.statOffsetsTotal.ToStringSafeEnumerable()}" +
                        $"\nHediff ProgressTracker factors: {Hediff.Schema.progressTracker.statFactorsTotal.ToStringSafeEnumerable()}" +
                        $"\nHediff ProgressTracker cap mods: {Hediff.Schema.progressTracker.capModsTotal.ToStringSafeEnumerable()}");
            }

            progressDiffLedger.classDiffLedgers[currClassDef] = progressDiffClassLedger;
            progressDiffLog.AddLedger(progressDiffLedger);
            Hediff.cachedCurStage = RefreshCurStage();
        }

        public override HediffStage RefreshCurStage()
        {
            HediffStage stage = new HediffStage()
            {
                statOffsets = CreateStatModifiers(statOffsetsTotal).ToList(),
                //statFactors = CreateStatModifiers(statFactorsTotal).ToList(), // assumes multiplier is baked in
                statFactors = CreateStatModifiers(statFactorsTotal, true).ToList(), // will bake in the multiplier effect for statfactors
                capMods = CreateCapModifiers(capModsTotal).ToList()
            };
            
            return stage;
        }

        public override void NotifyTotalLevelUp(float orgSev, List<Window> windows = null, bool silent_msg = false)
        {
            // if there are any option windows that show up, display them in reverse (lowest to highest lvl)
            if (!windows.NullOrEmpty()) 
            {
                foreach(var window in windows.Reverse<Window>()) Find.WindowStack.Add(window);
            }

            // debug info
            if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
                Log.Message($"[It's Sorcery!] {schema.def.label} Diff Log Total:\n{progressDiffLog.TotalDiff(null)}");

            // not player faction? don't show level ups!
            // silent msg? don't bother running the rest of the method then!
            if (pawn.Faction is null || !pawn.Faction.IsPlayer || silent_msg) return; 
            Find.LetterStack.ReceiveLetter(def.progressLevelUpKey.Translate(pawn.Name.ToStringShort),
                def.progressLevelUpDescKey.Translate(orgSev.ToString(), CurrLevel.ToString()), LetterDefOf.NeutralEvent);
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

            // CLASS CHANGE BUTTON
            Text.Font = GameFont.Small;
            Rect classChangeRect = new Rect(0f + viewRect.width * 2f / 5f, coordY, viewRect.width / 5, 35f);
            if (pawn.Faction != null && pawn.Faction.IsPlayer && Widgets.ButtonText(classChangeRect, "Classes")) Find.WindowStack.Add(new Dialog_ClassChange(this, classChangeOpps));
            coordY += classChangeRect.height * 1.5f;

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

            String tipString = TipStringExtra(Hediff.CurStage);
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
            //Rect leftHalf = new Rect(0, 0, rect.width / 2f, rect.height);

            // PROSPECTS - future modifiers w/ level up //
            Rect modRect = new Rect(0, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            modRect.height += 20f;
            Rect modRectView = new Rect(modRect.x, modRect.y, modRect.width - 20f * Prefs.UIScale, modScrollViewHeight);
            Widgets.BeginScrollView(modRect, ref this.modScrollPosition, modRectView, true);

            float coordY = 0f;
            Rect allModRect = new Rect(modRectView.x, modRectView.y + coordY, modRectView.width, 500f);
            coordY += DrawProspects(allModRect);
            modScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // EXPERIENCE - show expgain //
            Rect expRect = new Rect(rect.width / 4f, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            expRect.height += 20f;
            Rect expRectView = new Rect(expRect.x, expRect.y, expRect.width - 20f * Prefs.UIScale, expScrollViewHeight);
            Widgets.BeginScrollView(expRect, ref this.expScrollPosition, expRectView, true);

            coordY = 0f;
            Rect allEXPRect = new Rect(expRectView.x, expRectView.y + coordY, expRectView.width, 500f);
            coordY += DrawEXPMethods(allEXPRect);
            expScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // ENERGY - show energytrackers //
            Rect energyRect = new Rect(rect.width / 2f, 0, rect.width / 4f, rect.height).ContractedBy(20f);
            //energyRect.height += 20f;
            Rect energyRectView = new Rect(energyRect.x, energyRect.y, energyRect.width - 20f * Prefs.UIScale, energyScrollViewHeight);
            Widgets.BeginScrollView(energyRect, ref this.energyScrollPosition, energyRectView, true);

            coordY = 0f;
            Rect allenergyRect = new Rect(energyRectView.x, energyRectView.y + coordY, energyRectView.width, 500f);
            coordY += DrawEnergyComps(allenergyRect);
            energyScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // SORCERIES - see sorceries and change out what you want to use //
            Rect sorceryRect = new Rect(0, rect.height * 2f / 3f, rect.width / 2f, rect.height /3f).ContractedBy(20f);
            sorceryRect.height += 20f;
            Rect sorceryRectView = new Rect(sorceryRect.x, sorceryRect.y, sorceryRect.width - 20f * Prefs.UIScale, sorceryScrollViewHeight);
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

        public override float DrawProspects(Rect rect)
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

            if (projLevel > Hediff.def.maxSeverity) return rect.yMin - yMin;
            Text.Font = GameFont.Small;
            for (int i = (int)projLevel; i <= Math.Min((int)projLevel + ItsSorceryUtility.settings.ProgressViewProspectsNum, currClassDef.levelRange.TrueMax); i++)
            {
                if (i > Hediff.def.maxSeverity) break;

                ProgressLevelModifier factor = currClassDef.GetLevelFactor(i); // def.GetLevelFactor(i);
                tipString = TipStringExtra(factor);
                ProgressLevelModifier special = currClassDef.GetLevelSpecific(i); // def.GetLevelSpecific(i);
                tipString2 = TipStringExtra(special);

                if (tipString.NullOrEmpty() && !OptionsCheck(factor) && !HyperlinkCheck(factor)  &&
                    tipString2.NullOrEmpty() && !OptionsCheck(special) && !HyperlinkCheck(special) && !SpecialUnlocksCheck(special)) continue;

                Text.Font = GameFont.Small;
                if (CurLevelLabel.NullOrEmpty())
                    Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevel".Translate(i).Colorize(ColoredText.TipSectionTitleColor), true, false);
                else
                    Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevelCustom".Translate(i, GetProgressLevelLabel(i)).Colorize(ColoredText.TipSectionTitleColor), true, false);
                rect.yMin += rect.height;

                if (!tipString.NullOrEmpty() || OptionsCheck(factor) || HyperlinkCheck(factor))
                {
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevelProspectsNormal".Translate().Colorize(ColoredText.SubtleGrayColor), true, false);
                    rect.yMin += rect.height;

                    // draw modifiers
                    rect.yMin += this.DrawModifiers(rect, factor, tipString);

                    // draw options
                    rect.yMin += this.DrawOptions(rect, factor);

                    // draw hyperlinks
                    rect.yMin += this.DrawHyperlinks(rect, factor);
                }
                if (!tipString2.NullOrEmpty() || OptionsCheck(special) || HyperlinkCheck(special) || SpecialUnlocksCheck(special))
                {
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevelProspectsSpecial".Translate().Colorize(ColoredText.SubtleGrayColor), true, false);
                    rect.yMin += rect.height;

                    // draw modifiers
                    rect.yMin += this.DrawModifiers(rect, special, tipString2);

                    // draw options
                    rect.yMin += this.DrawOptions(rect, special);

                    // draw hyperlinks
                    rect.yMin += this.DrawHyperlinks(rect, special);

                    // draw special unlocks
                    rect.yMin += this.DrawSpecialUnlocks(rect, special);
                }

            }

            return rect.yMin - yMin;
        }

        public override float DrawEnergyComps(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            if (schema.energyTrackers.NullOrEmpty())
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.LabelCacheHeight(ref rect, "ISF_EnergyTrackerCompNone".Translate(), true, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                rect.yMin += rect.height;
                return rect.yMin - yMin;
            }

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
