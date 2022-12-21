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
            //Widgets.BeginScrollView(outRect, ref this.leftScrollPosition, viewRect, true);

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
            Rect outScrollRect = new Rect(outRect);
            outScrollRect.yMin = coordY;
            Rect viewScrollRect = new Rect(viewRect);
            viewScrollRect.yMin = coordY;
            Widgets.BeginScrollView(outScrollRect, ref this.leftScrollPosition, viewScrollRect, true);

            GenUI.ResetLabelAlign();
            Text.Font = GameFont.Small;
            Rect descRect = new Rect(0f, coordY, viewRect.width, 0f);
            Widgets.LabelCacheHeight(ref descRect, schemaDef.description, true, false);
            coordY += descRect.height;

            
            //coordY += labelRect.height;

            this.leftScrollViewHeight = descRect.height + xpBar.height * 4f;
            Widgets.EndScrollView();
            Widgets.EndGroup();

        }

        public override void DrawRightGUI(Rect rect)
        {

        }



        public SorcerySchema cachedSchema;

        private Vector2 leftScrollPosition = Vector2.zero;

        private float leftScrollViewHeight;

    }
}
