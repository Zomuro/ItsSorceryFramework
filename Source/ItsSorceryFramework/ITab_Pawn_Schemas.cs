using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class ITab_Pawn_Schemas : ITab
    {
        // only humanlike pawns with the sorcery comp can even see the schema tab
        public override bool IsVisible
        {
            get
            {
                return SorceryComp != null && !Schemas.NullOrEmpty();
            }
        }

        public ITab_Pawn_Schemas()
        {
            this.size = new Vector2(480f, 450f);
            this.labelKey = "ISF_TabSchemas";
            this.tutorTag = "ISF_TabSchemas";
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Medium;
            Rect window = new Rect(0, 5, TabRect.width, TabRect.height - 5);
            Widgets.BeginGroup(window);

            Rect view = window.ContractedBy(15f);
            view.yMin += 10;
            view.xMax = window.xMax;

            Color color = GUI.color;
            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(window.x, view.y, window.width);
            GUI.color = color;

            Rect viewSchema = view.ContractedBy(5f);
            Rect schemaRect = view.ContractedBy(8f);
            schemaRect.width -= 20f;
            schemaRect.height = 75f;

            Rect viewScroll = new Rect(viewSchema.x, viewSchema.y, viewSchema.width - 20, schemaScrollViewHeight + 10f);
            // calculate the number of "pages" and schemas we can fit into the itab
            possibleSlots = 5;
            possiblePages = (int) Math.Ceiling((1f*Schemas.CountAllowNull()) / possibleSlots);

            // sets current page
            currentPage = energyTrackerIndex / possibleSlots + 1;
            Text.Font = GameFont.Small;

            // draw page count and page change buttons
            DrawPageUI();

            float totalSchemaHeight = 0;
            
            Widgets.BeginScrollView(viewSchema, ref this.schemaScrollPosition, viewScroll, true);
            // for every sorcery schema
            foreach (SorcerySchema schema in Schemas.GetRange(energyTrackerIndex,
                Math.Min(Schemas.Count() - energyTrackerIndex, 5)))
            {
                // take the energy tracker and display it
                //schema.energyTracker.DrawOnGUI(schemaRect);
                float schemaHeight = schema.energyTracker.DrawOnGUI(ref schemaRect);
                totalSchemaHeight += schemaHeight + 1;
                schemaRect.y += schemaHeight + 1;
            }
            Text.Font = GameFont.Small;

            schemaScrollViewHeight = totalSchemaHeight;
            Widgets.EndScrollView();
            Widgets.EndGroup();
        }

        public void DrawPageUI()
        {
            Rect button1 = new Rect(size.x / 2 - 25 - 50, 0, 50, 25);
            Rect button2 = new Rect(size.x / 2 + 25, 0, 50, 25);
            Rect pageLabel = new Rect(size.x / 2 - 25, 0, 50, 25);

            // as long as it isn't the first page, go back
            if (currentPage > 1 && Widgets.ButtonText(button1, "<")) energyTrackerIndex -= possibleSlots;

            // as long as it isn't the last page, can move forwards
            if (currentPage < possiblePages && Widgets.ButtonText(button2, ">")) energyTrackerIndex += possibleSlots;

            Text.Anchor = TextAnchor.MiddleCenter;
            // if there is only one page, don't bother with the page number
            if (possiblePages < 2)
            {
                Widgets.Label(pageLabel, "- / -");
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                Widgets.Label(pageLabel, currentPage.ToString() + " / " + possiblePages.ToString());
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        public Comp_ItsSorcery SorceryComp
        {
            get
            {
                if(sorceryComp == null || sorceryComp.pawn != SelPawn)
                {
                    sorceryComp = SorcerySchemaUtility.GetSorceryComp(SelPawn);
                }

                return sorceryComp;
            }
        }

        public List<SorcerySchema> Schemas
        {
            get
            {
                if (SorceryComp == null) return null;
                return SorceryComp.schemaTracker.sorcerySchemas;
            }
        }

        private Comp_ItsSorcery sorceryComp = null;

        public int energyTrackerIndex = 0;

        public int currentPage;

        public int possiblePages;

        public int possibleSlots;

        private Vector2 schemaScrollPosition = Vector2.zero;

        private float schemaScrollViewHeight;
    }
}
