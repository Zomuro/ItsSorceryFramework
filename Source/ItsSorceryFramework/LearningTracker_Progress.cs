using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class LearningTracker_Progress : LearningTracker
    {
        public LearningTracker_Progress(Pawn pawn) : base(pawn)
        {

        }

        public LearningTracker_Progress(Pawn pawn, LearningTrackerDef def) : base(pawn, def)
        {

        }

        public LearningTracker_Progress(Pawn pawn, LearningTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {

        }

        /*public Vector2 ViewSize
        {
            get
            {
                if (cachedViewSize == null || cachedViewSize == Vector2.zero) cachedViewSize = findViewSize();

                return cachedViewSize;
            }
        }*/

        public SorcerySchema schema
        {
            get
            {
                if (cachedSchema == null) cachedSchema = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef);

                return cachedSchema;
            }
        }

        public ProgressTracker progressTracker
        {
            get
            {
                return schema.progressTracker;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Collections.Look(ref nodes, "nodes", LookMode.Def);
            //Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value);

        }

        public override void DrawLeftGUI(Rect rect)
        {
            //float outRectHeight = rect.height - (10f + leftStartAreaHeight) - 45f;

            Widgets.BeginGroup(rect);

            // add textures- no dice, dependent on size
            /*Rect uvRect = rect.ContractedBy(25f);
            Widgets.DrawTextureFitted(rect, def.BGIcon, 0.8f);*/

            Rect outRect = new Rect(0f, 0f, rect.width, rect.height);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, leftScrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.leftScrollPosition, viewRect, true);

            float coordY = 0f;

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            //GenUI.SetLabelAlign(TextAnchor.MiddleLeft);

            Rect labelRect = new Rect(0f, coordY, viewRect.width, 50f);
            Widgets.LabelCacheHeight(ref labelRect, schema.def.LabelCap, true, false);
            coordY += labelRect.height;

            Rect lvlRect = new Rect(0f, coordY, viewRect.width, 50f);
            Widgets.LabelCacheHeight(ref lvlRect, 
                "(lvl. "+ "{0}/{1})".Translate(progressTracker.currLevel, progressTracker.hediff.def.maxSeverity), true, false);
            coordY += labelRect.height;

            Rect xpBar = new Rect(0f, coordY + 10, rect.width, 35f);
            Widgets.FillableBar(xpBar, progressTracker.currProgress);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(xpBar, (progressTracker.exp).ToString("F0") + " / " + progressTracker.currentLevelEXPReq.ToString("F0"));
            coordY += xpBar.height * 1.5f;

            //viewRect
            /*Rect outScrollRect = new Rect(outRect);
            outScrollRect.yMin = coordY;
            Rect viewScrollRect = new Rect(viewRect);
            viewScrollRect.yMin = coordY;*/

            //Widgets.BeginScrollView(outScrollRect, ref this.leftScrollPosition, viewScrollRect, true);
            GenUI.ResetLabelAlign();
            Text.Font = GameFont.Small;
            Rect descRect = new Rect(0f, coordY, viewRect.width, 0f);
            //Rect descRect = new Rect(viewScrollRect);
            Widgets.LabelCacheHeight(ref descRect, schemaDef.description, true, false);
            coordY += descRect.height;

            
            //coordY += labelRect.height;

            this.leftScrollViewHeight = coordY;
            Widgets.EndScrollView();
            Widgets.EndGroup();

        }

        public override void DrawRightGUI(Rect rect)
        {
            Widgets.BeginGroup(rect);
            Rect leftHalf = new Rect(0, 0, rect.width/2f, rect.height);
            Rect leftHalfView = leftHalf.ContractedBy(10);


            float coordY = 0f;

            /*
            Rect leftQuad1 = new Rect(0, 0, rect.width / 4f, rect.height * 2f/3f).ContractedBy(10f);
            Rect leftQuad1View = new Rect(leftQuad1.x, leftQuad1.y, leftQuad1.width - 20f, scrollViewHeightQuad1);
            Widgets.BeginScrollView(leftQuad1, ref this.scrollPositionQuad1, leftQuad1View, true);*/

            // current and upcoming modifiers for hediff
            Rect modRect = new Rect(0, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            Rect modRectView = new Rect(modRect.x, modRect.y, modRect.width - 20f, modScrollViewHeight);
            Widgets.BeginScrollView(modRect, ref this.modScrollPosition, modRectView, true);

            coordY = 0f;
            Rect allModRect = new Rect(modRectView.x, modRectView.y + coordY, modRectView.width, 500f);
            coordY += drawModifiers(allModRect);
            modScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // all sorceries linked to this schema (and if you have them)
            Rect sorceryRect = new Rect(0, rect.height * 2f / 3f, rect.width / 2f, rect.height * 3f).ContractedBy(20f);
            Rect sorceryRectView = new Rect(sorceryRect.x, sorceryRect.y, sorceryRect.width - 20f, sorceryScrollViewHeight);
            Widgets.BeginScrollView(sorceryRect, ref this.sorceryScrollPosition, sorceryRectView, true);

            coordY = 0f;
            Rect allSorceryRect = new Rect(sorceryRectView.x, sorceryRectView.y + coordY, sorceryRectView.width, 500f);
            coordY += drawSorceries(allSorceryRect);
            sorceryScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // background image
            Rect rightHalf = new Rect(rect.width / 2f, 0, rect.width / 2f, rect.height);
            Widgets.DrawLineVertical(rightHalf.x, rightHalf.y, rightHalf.height);

            Widgets.DrawTextureFitted(rightHalf, def.BGIcon, 0.95f);
            Widgets.EndGroup();
        }

        private float drawModifiers(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "Modifiers", true, false);
            rect.yMin += rect.height;
            Text.Font = GameFont.Small;
            rect.x += 6f;

            String tipString = TipStringExtra(progressTracker.hediff.CurStage);
            String tipString2;
            if (!tipString.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Current:", true, false);
                rect.yMin += rect.height;
                Widgets.LabelCacheHeight(ref rect, tipString, true, false);
                rect.yMin += rect.height;
            }

            ProgressTrackerDef pDef = progressTracker.def;
            IEnumerable<StatDrawEntry> mods;
            float projLevel = progressTracker.currLevel + 1;

            Widgets.LabelCacheHeight(ref rect, "Upcoming:", true, false);
            rect.yMin += rect.height;

            for (int i = (int) projLevel; i < (int) projLevel + 5; i++)
            {
                if (i > progressTracker.hediff.def.maxSeverity) break;

                mods = pDef.specialDisplayMods(pDef.getLevelFactor(i));
                tipString = TipStringExtra(mods);
                mods = pDef.specialDisplayMods(pDef.getLevelSpecific(i));
                tipString2 = TipStringExtra(mods);

                if (tipString.NullOrEmpty() && tipString2.NullOrEmpty()) continue;

                Widgets.LabelCacheHeight(ref rect, "Level "+ i, true, false);
                rect.yMin += rect.height;

                if (!tipString.NullOrEmpty())
                {
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref rect, tipString, true, false);
                    rect.yMin += rect.height;
                }
                if (!tipString2.NullOrEmpty())
                {
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref rect, tipString2, true, false);
                    rect.yMin += rect.height;
                }

            }

            return rect.yMin - yMin;
        }

        public string TipStringExtra(HediffStage stage)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StatDrawEntry statDrawEntry in stage.SpecialDisplayStats())
            {
                if (statDrawEntry.ShouldDisplay)
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            return stringBuilder.ToString();
        }

        public string TipStringExtra(IEnumerable<StatDrawEntry> entries)
        {
            if (entries.EnumerableNullOrEmpty()) return null;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StatDrawEntry statDrawEntry in entries)
            {
                if (statDrawEntry.ShouldDisplay)
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            return stringBuilder.ToString();
        }

        private float drawSorceries(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            List<Sorcery> sorceries = (from ability in pawn.abilities.abilities
                                       where (ability as Sorcery) != null && (ability as Sorcery).sorceryDef.sorcerySchema == this.schemaDef
                                       select (ability as Sorcery)).ToList();

            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(rect);
            Widgets.LabelCacheHeight(ref titleRect, "Sorceries:", true, false);
            rect.yMin += titleRect.height;
            Text.Font = GameFont.Small;

            float scale = 50f;

            Rect bounds = new Rect(rect.x, rect.y, rect.width - 5f, rect.height);
            Rect sorceriesRect = GenUI.DrawElementStack<Sorcery>(bounds, scale, sorceries, 
                delegate (Rect r, Sorcery sorcery)
                {
                    GUI.DrawTexture(r, BaseContent.ClearTex);
                    if (Mouse.IsOver(r))
                    {
                        Widgets.DrawHighlight(r);
                    }
                    if (Widgets.ButtonImage(r, sorcery.def.uiIcon, false))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(sorcery.def, null));
                    }
                    if (Mouse.IsOver(r))
                    {
                        Sorcery capture = sorcery;
                        TipSignal tip = new TipSignal(() => capture.SorceryTooltip + "\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor), 
                            (int)bounds.y * 37);
                        TooltipHandler.TipRegion(r, tip);
                    }

                }, (Sorcery sorcery) => scale, 4f, 5f, true);

            rect.yMin += sorceriesRect.height;

            return rect.yMin - yMin;
        }


        public SorcerySchema cachedSchema;

        private Vector2 leftScrollPosition = Vector2.zero;

        private Vector2 modScrollPosition = Vector2.zero;

        private Vector2 sorceryScrollPosition = Vector2.zero;


        private float leftScrollViewHeight;

        private float modScrollViewHeight;

        private float sorceryScrollViewHeight;


    }
}
