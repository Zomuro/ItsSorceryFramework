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
        private Vector2 leftScrollPosition = Vector2.zero;

        private Vector2 modScrollPosition = Vector2.zero;

        private Vector2 expScrollPosition = Vector2.zero;

        private Vector2 sorceryScrollPosition = Vector2.zero;

        private float leftScrollViewHeight;

        private float modScrollViewHeight;

        private float expScrollViewHeight;

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
            Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, leftScrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.leftScrollPosition, viewRect, true);

            float coordY = 0f;

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            // title of sorcery schema
            Rect labelRect = new Rect(0f, coordY, viewRect.width, 50f);
            Widgets.LabelCacheHeight(ref labelRect, schema.def.LabelCap, true, false);
            coordY += labelRect.height;

            // level label
            Rect lvlRect = new Rect(0f, coordY, viewRect.width, 50f);
            Text.Font = GameFont.Small;
            if (CurLevelLabel.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref lvlRect,
                    "ISF_LearningLevelLabel".Translate(CurrLevel), true, false);
            }
            else
            {
                if (Maxed)
                {
                    Widgets.LabelCacheHeight(ref lvlRect,
                        "ISF_LearningLevelLabelCustom".Translate(CurLevelLabel, "ISF_LearningLevelLabelMax".Translate()), true, false);
                }
                else
                {
                    Widgets.LabelCacheHeight(ref lvlRect,
                        "ISF_LearningLevelLabelCustom".Translate(CurLevelLabel, CurrLevel), true, false);
                }

            }
            coordY += lvlRect.height;

            // xp bar
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

            // description
            GenUI.ResetLabelAlign();
            Text.Font = GameFont.Small;
            Rect descRect = new Rect(0f, coordY, viewRect.width, 0f);
            Widgets.LabelCacheHeight(ref descRect, schema.def.description, true, false);
            coordY += descRect.height;

            this.leftScrollViewHeight = coordY;
            Widgets.EndScrollView();
            Widgets.EndGroup();

        }

        public override void DrawRightGUI(Rect rect)
        {
            Widgets.BeginGroup(rect);
            Rect leftHalf = new Rect(0, 0, rect.width / 2f, rect.height);

            // current and upcoming modifiers for hediff
            Rect modRect = new Rect(0, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            modRect.height += 20f;
            Rect modRectView = new Rect(modRect.x, modRect.y, modRect.width - 20f, modScrollViewHeight);
            Widgets.BeginScrollView(modRect, ref this.modScrollPosition, modRectView, true);

            float coordY = 0f;
            Rect allModRect = new Rect(modRectView.x, modRectView.y + coordY, modRectView.width, 500f);
            coordY += DrawModifiers(allModRect);
            modScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // all experience gain methods
            Rect expRect = new Rect(rect.width / 4f, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            expRect.height += 20f;
            Rect expRectView = new Rect(expRect.x, expRect.y, expRect.width - 20f, expScrollViewHeight);
            Widgets.BeginScrollView(expRect, ref this.expScrollPosition, expRectView, true);

            coordY = 0f;
            Rect allEXPRect = new Rect(expRectView.x, expRectView.y + coordY, expRectView.width, 500f);
            coordY += DrawEXPMethods(allEXPRect);
            expScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // all sorceries linked to this schema (and if you have them)
            Rect sorceryRect = new Rect(0, rect.height * 2f / 3f, rect.width / 2f, rect.height * 3f).ContractedBy(20f);
            sorceryRect.height += 20f;
            Rect sorceryRectView = new Rect(sorceryRect.x, sorceryRect.y, sorceryRect.width - 20f, sorceryScrollViewHeight);
            Widgets.BeginScrollView(sorceryRect, ref this.sorceryScrollPosition, sorceryRectView, true);

            coordY = 0f;
            Rect allSorceryRect = new Rect(sorceryRectView.x, sorceryRectView.y + coordY, sorceryRectView.width, 500f);
            coordY += DrawSorceries(allSorceryRect);
            sorceryScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // background image
            Rect rightHalf = new Rect(rect.width / 2f, 0, rect.width / 2f, rect.height);
            Color col = GUI.color;
            GUI.color = Color.grey;
            Widgets.DrawLineVertical(rightHalf.x, rightHalf.y, rightHalf.height);
            GUI.color = col;

            Widgets.DrawTextureFitted(rightHalf, def.BGIcon, 0.95f);
            Widgets.EndGroup();
        }

        public override float DrawModifiers(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "Modifiers", true, false);
            rect.yMin += rect.height;
            //Text.Font = GameFont.Small;
            rect.x += 22f;

            String tipString = TipStringExtra(hediff.CurStage);
            String tipString2;
            if (!tipString.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Current:", true, false);
                rect.yMin += rect.height;
                Text.Font = GameFont.Small;

                Widgets.LabelCacheHeight(ref rect, tipString, true, false);
                rect.yMin += rect.height;
            }

            ProgressTrackerDef pDef = def;
            float projLevel = CurrLevel + 1;

            if (projLevel > hediff.def.maxSeverity) return rect.yMin - yMin;
            Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "Upcoming:", true, false);
            rect.yMin += rect.height;
            Text.Font = GameFont.Small;

            for (int i = (int)projLevel; i < (int)projLevel + 5; i++)
            {
                if (i > hediff.def.maxSeverity) break;

                ProgressLevelModifier factor = pDef.getLevelFactor(i);
                tipString = TipStringExtra(factor);
                ProgressLevelModifier special = pDef.getLevelSpecific(i);
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
    }
}
