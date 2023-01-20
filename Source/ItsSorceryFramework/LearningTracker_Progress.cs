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

        public SorcerySchema Schema
        {
            get
            {
                if (cachedSchema == null) cachedSchema = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef);

                return cachedSchema;
            }
        }

        public ProgressTracker ProgressTracker
        {
            get
            {
                return Schema.progressTracker;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
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
            Widgets.LabelCacheHeight(ref labelRect, Schema.def.LabelCap, true, false);
            coordY += labelRect.height;

            Rect lvlRect = new Rect(0f, coordY, viewRect.width, 50f);

            if (ProgressTracker.CurLevelLabel.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref lvlRect,
                    "ISF_LearningLevelLabel".Translate(ProgressTracker.currLevel), true, false);
            }
            else
            {
                Widgets.LabelCacheHeight(ref lvlRect,
                    "ISF_LearningLevelLabelCustom".Translate(ProgressTracker.CurLevelLabel, ProgressTracker.currLevel), true, false);
            }
            coordY += lvlRect.height;
            //coordY += labelRect.height;

            Rect xpBar = new Rect(0f, coordY + 10, rect.width, 35f);
            Widgets.FillableBar(xpBar, ProgressTracker.currProgress);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(xpBar, (ProgressTracker.exp).ToString("F0") + " / " + ProgressTracker.currentLevelEXPReq.ToString("F0"));
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

            // current and upcoming modifiers for hediff
            Rect modRect = new Rect(0, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            modRect.height += 20f;
            Rect modRectView = new Rect(modRect.x, modRect.y, modRect.width - 20f, modScrollViewHeight);
            Widgets.BeginScrollView(modRect, ref this.modScrollPosition, modRectView, true);

            coordY = 0f;
            Rect allModRect = new Rect(modRectView.x, modRectView.y + coordY, modRectView.width, 500f);
            coordY += drawModifiers(allModRect);
            modScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // all experience gain methods
            Rect expRect = new Rect(rect.width / 4f, 0, rect.width / 4f, rect.height * 2f / 3f).ContractedBy(20f);
            expRect.height += 20f;
            Rect expRectView = new Rect(expRect.x, expRect.y, expRect.width - 20f, expScrollViewHeight);
            Widgets.BeginScrollView(expRect, ref this.expScrollPosition, expRectView, true);

            coordY = 0f;
            Rect allEXPRect = new Rect(expRectView.x, expRectView.y + coordY, expRectView.width, 500f);
            coordY += drawEXPMethods(allEXPRect);
            expScrollViewHeight = coordY;

            Widgets.EndScrollView();

            // all sorceries linked to this schema (and if you have them)
            Rect sorceryRect = new Rect(0, rect.height * 2f / 3f, rect.width / 2f, rect.height * 3f).ContractedBy(20f);
            sorceryRect.height += 20f;
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
            //Text.Font = GameFont.Small;
            rect.x += 22f;

            String tipString = TipStringExtra(ProgressTracker.hediff.CurStage);
            String tipString2;
            if (!tipString.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Current:", true, false);
                rect.yMin += rect.height;
                Text.Font = GameFont.Small;

                Widgets.LabelCacheHeight(ref rect, tipString, true, false);
                rect.yMin += rect.height;
            }

            ProgressTrackerDef pDef = ProgressTracker.def;
            float projLevel = ProgressTracker.currLevel + 1;

            if (projLevel > ProgressTracker.hediff.def.maxSeverity) return rect.yMin - yMin;
            Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "Upcoming:", true, false);
            rect.yMin += rect.height;
            Text.Font = GameFont.Small;

            for (int i = (int) projLevel; i < (int) projLevel + 5; i++)
            {
                if (i > ProgressTracker.hediff.def.maxSeverity) break;

                ProgressLevelModifier factor = pDef.getLevelFactor(i);
                tipString = TipStringExtra(factor);
                ProgressLevelModifier special = pDef.getLevelSpecific(i);
                tipString2 = TipStringExtra(special);

                if (tipString.NullOrEmpty() && !hyperlinkCheck(factor) && 
                    tipString2.NullOrEmpty() && !hyperlinkCheck(special)) continue;

                Text.Font = GameFont.Small;
                //Text.Font = GameFont.Medium;
                if(ProgressTracker.CurLevelLabel.NullOrEmpty())
                    Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevel".Translate(i).Colorize(ColoredText.TipSectionTitleColor), true, false);
                else
                    Widgets.LabelCacheHeight(ref rect, "ISF_LearningProgressLevelCustom".Translate(i, ProgressTracker.GetProgressLevelLabel(i)).Colorize(ColoredText.TipSectionTitleColor), true, false);
                rect.yMin += rect.height;
                //Text.Font = GameFont.Small;

                Rect hyperlinkRect;

                if (!tipString.NullOrEmpty() || hyperlinkCheck(factor))
                {
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref rect, "Normal".Colorize(ColoredText.SubtleGrayColor), true, false);
                    rect.yMin += rect.height;

                    Widgets.LabelCacheHeight(ref rect, tipString, true, false);
                    rect.yMin += rect.height;

                    rect.xMin += 6f;
                    hyperlinkRect = new Rect(rect.x, rect.yMin, rect.width, 500f);
                    rect.yMin += this.drawHyperlinks(hyperlinkRect, factor);
                    rect.xMin -= 6f;
                }
                if (!tipString2.NullOrEmpty() || hyperlinkCheck(special))
                {
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref rect, "Special".Colorize(ColoredText.SubtleGrayColor), true, false);
                    rect.yMin += rect.height;

                    Widgets.LabelCacheHeight(ref rect, tipString2, true, false);
                    rect.yMin += rect.height;

                    rect.xMin += 6f;
                    hyperlinkRect = new Rect(rect.x, rect.yMin, rect.width, 500f);
                    rect.yMin += this.drawHyperlinks(hyperlinkRect, special);
                    rect.xMin -= 6f;
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

        public string TipStringExtra(ProgressLevelModifier mods)
        {
            IEnumerable<StatDrawEntry> entries = ProgressTracker.def.specialDisplayMods(mods);
            if (entries.EnumerableNullOrEmpty()) return null;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StatDrawEntry statDrawEntry in entries)
            {
                if (statDrawEntry.ShouldDisplay)
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            if(mods.pointGain > 0) stringBuilder.AppendInNewLine("  - " + ProgressTracker.def.skillPointLabelKey.Translate().CapitalizeFirst() + ": " + 
                mods.pointGain);
            return stringBuilder.ToString();
        }

        private float drawEXPMethods(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "Experience:", true, false);
            rect.yMin += rect.height;
            //Text.Font = GameFont.Small;
            rect.x += 22f;

            /*if(schema.progressTracker.def.expTags.EnumerableNullOrEmpty()) return rect.yMin - yMin;
            foreach (ProgressEXPDef expDef in schema.progressTracker.def.expTags)
            {
                rect.yMin += expDef.Worker.drawWorker(rect);
            }*/

            if(Schema.progressTracker.def.Workers.NullOrEmpty()) return rect.yMin - yMin;
            foreach (ProgressEXPWorker worker in Schema.progressTracker.def.Workers)
            {
                rect.yMin += worker.drawWorker(rect);
            }

            return rect.yMin - yMin;
        }

        private bool hyperlinkCheck(ProgressLevelModifier mod)
        {
            if (mod == null) return false;

            if (mod.abilityGain.NullOrEmpty() && mod.abilityRemove.NullOrEmpty() && mod.hediffAdd.NullOrEmpty() &&
                mod.hediffAdjust.NullOrEmpty() && mod.hediffRemove.NullOrEmpty()) return false;

            return true;
        }

        private float drawHyperlinks(Rect rect, ProgressLevelModifier mod)
        {
            List<AbilityDef> abilityGain = mod.abilityGain;
            List<AbilityDef> abilityRemove = mod.abilityRemove;
            List<NodeHediffProps> hediffAdd = mod.hediffAdd;
            List<NodeHediffProps> hediffAdjust = mod.hediffAdjust;
            List<HediffDef> hediffRemove = mod.hediffRemove;

            if (abilityGain.NullOrEmpty() && abilityRemove.NullOrEmpty() && hediffAdd.NullOrEmpty() && hediffAdjust.NullOrEmpty() &&
                hediffRemove.NullOrEmpty())
            {
                return 0f;
            }

            float yMin = rect.yMin;
            float x = rect.x;
            Dialog_InfoCard.Hyperlink hyperlink;

            if (!abilityGain.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Abilities gained:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (AbilityDef abilityDef in abilityGain)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(abilityDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!abilityRemove.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Abilities removed:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (AbilityDef abilityDef in abilityRemove)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(abilityDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!hediffAdd.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Hediffs added:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (NodeHediffProps prop in hediffAdd)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    HediffDef hediffDef = prop.hediffDef;
                    string sev;

                    sev = hediffDef.stages.NullOrEmpty() ? prop.severity.ToStringWithSign("F0") :
                        hediffDef.stages[hediffDef.StageAtSeverity(prop.severity)].label;
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, hediffDef.LabelCap + " ({0})".Translate(sev),
                        2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;

                }
                rect.x = x;
            }

            if (!hediffAdjust.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Hediff adjustments:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (NodeHediffProps prop in hediffAdjust)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    HediffDef hediffDef = prop.hediffDef;
                    string sev;

                    sev = prop.severity.ToStringWithSign("F0");
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, hediffDef.LabelCap + " ({0})".Translate(sev),
                        2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!hediffRemove.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Hediffs removed:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (HediffDef hediffDef in hediffRemove)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            return rect.yMin - yMin;
        }

        private float drawSorceries(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            List<Sorcery> sorceries = (from ability in pawn.abilities.abilities
                                       where (ability as Sorcery) != null && (ability as Sorcery).sorceryDef.sorcerySchema == this.schemaDef
                                       select (ability as Sorcery)).ToList();

            List<SorceryDef> sorceryDefs = (from def in DefDatabase<SorceryDef>.AllDefs
                                       where def.sorcerySchema == this.schemaDef
                                       select def).ToList();

            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(rect);
            Widgets.LabelCacheHeight(ref titleRect, "Sorceries:", true, false);
            rect.yMin += titleRect.height;
            Text.Font = GameFont.Small;

            float scale = 50f;

            Color col = Color.white;
            Rect bounds = new Rect(rect.x, rect.y, rect.width - 5f, rect.height);
            Rect sorceriesRect = GenUI.DrawElementStack<SorceryDef>(bounds, scale, sorceryDefs,
                delegate (Rect r, SorceryDef sorceryDef)
                {
                    if (pawn.abilities.GetAbility(sorceryDef) != null) col = Color.white;
                    else col = new Color(0.25f, 0.25f, 0.25f);

                    GUI.DrawTexture(r, BaseContent.ClearTex);
                    if (Mouse.IsOver(r))
                    {
                        Widgets.DrawHighlight(r);
                    }
                    if (Widgets.ButtonImage(r, sorceryDef.uiIcon, col, true))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(sorceryDef, null));
                    }
                    if (Mouse.IsOver(r))
                    {
                        Sorcery sorcery = AbilityUtility.MakeAbility(sorceryDef, pawn) as Sorcery;
                        TipSignal tip = new TipSignal(() => sorcery.SorceryTooltip + "\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor),
                            (int)bounds.y * 37);
                        TooltipHandler.TipRegion(r, tip);
                    }

                }, (SorceryDef sorceryDef) => scale, 4f, 5f, true);

            rect.yMin += sorceriesRect.height;

            return rect.yMin - yMin;
        }


        public SorcerySchema cachedSchema;

        private Vector2 leftScrollPosition = Vector2.zero;

        private Vector2 modScrollPosition = Vector2.zero;

        private Vector2 expScrollPosition = Vector2.zero;

        private Vector2 sorceryScrollPosition = Vector2.zero;


        private float leftScrollViewHeight;

        private float modScrollViewHeight;

        private float expScrollViewHeight;

        private float sorceryScrollViewHeight;


    }
}
