using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
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

        public override void OnOpen()
        {
            base.OnOpen();
            this.filter = "";
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Medium;
            Rect window = new Rect(0, 5, TabRect.width, TabRect.height - 5);
            Widgets.BeginGroup(window);

            Rect view = window.ContractedBy(15f);
            view.yMin += 10;
            view.xMax = window.xMax;

            DrawDivider(window.x, view.y, window.width);

            Rect viewSchema = view.ContractedBy(5f);
            Rect schemaRect = view.ContractedBy(8f);
            schemaRect.width -= 20f;
            schemaRect.height = 75f;

            List<SorcerySchema> viewedSchemas = favView ? FilteredFavSchemas : FilteredSchemas;
            Rect viewScroll = new Rect(viewSchema.x, viewSchema.y, viewSchema.width - 20, schemaScrollViewHeight + 10f);

            // calculate the number of "pages" and schemas we can fit into the itab + sets current page
            possibleSlots = 5;
            possiblePages = (int) Math.Ceiling((1f* viewedSchemas.CountAllowNull()) / possibleSlots);
            currentPage = energyTrackerIndex / possibleSlots + 1;
            Text.Font = GameFont.Small;

            // draw page count + searchbar and favorites button, page change buttons, favorites, and schemas
            DrawPageUI();
            Rect search = DrawSearchBar(schemaRect.x);
            FavViewButton(search.xMax + 5, 0);
            DrawSchemas(viewSchema, viewScroll, schemaRect, viewedSchemas);
            Widgets.EndGroup();
        }

        public void DrawDivider(float x, float y, float width)
        {
            Color color = GUI.color;
            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(x, y, width);
            GUI.color = color;
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

        public Rect DrawSearchBar(float xPos)
        {
            //Rect bar = new Rect(TabRect.x + 20, 0, TabRect.width / 3 - 20, 26);
            Rect bar = new Rect(xPos, 0, TabRect.width / 4 - (xPos - TabRect.x), 26);

            this.filter = Widgets.TextField(bar, this.filter);
            if(Event.current.type == EventType.MouseDown || Event.current.keyCode == KeyCode.Escape)
            {
                GUI.FocusControl(null);
            }

            return bar;
        }

        public bool FavViewButton(float x, float y)
        {
            Rect rect = new Rect(x, y, 26f, 26f);
            MouseoverSounds.DoRegion(rect);
            TooltipHandler.TipRegionByKey(rect, "ISF_ButtonFavView");

            if (Widgets.ButtonImage(rect, favView ? GizmoTextureUtility.StarFull : GizmoTextureUtility.StarEmpty, GUI.color, true))
            {
                favView = !favView;
                return true;
            }
            return false;
        }

        public void DrawSchemas(Rect view, Rect viewScroll, Rect schemaRect, List<SorcerySchema> viewedSchemas)
        {
            float totalSchemaHeight = 0;
            Widgets.BeginScrollView(view, ref schemaScrollPosition, viewScroll, true);
            // for every sorcery schema
            foreach (SorcerySchema schema in viewedSchemas.GetRange(energyTrackerIndex,
                Math.Min(viewedSchemas.Count() - energyTrackerIndex, 5)))
            {
                // take the energy tracker and display it
                float schemaHeight = schema.energyTracker.DrawOnGUI(ref schemaRect);
                totalSchemaHeight += schemaHeight + 1;
                schemaRect.y += schemaHeight + 1;
            }
            Text.Font = GameFont.Small;

            schemaScrollViewHeight = totalSchemaHeight;
            Widgets.EndScrollView();
        }

        public Comp_ItsSorcery SorceryComp
        {
            get
            {
                if(sorceryComp is null || sorceryComp.pawn != SelPawn)
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
                if (SorceryComp is null) return null;
                return SorceryComp.schemaTracker.sorcerySchemas;
            }
        }

        public List<SorcerySchema> FavSchemas
        {
            get
            {
                return (from schema in Schemas where schema.favorited == true select schema).ToList() ?? new List<SorcerySchema>();
            }
        }

        // used in conjunction with the search bar component
        public List<SorcerySchema> FilteredSchemas
        {
            get
            {
                if (filter.NullOrEmpty()) return Schemas;
                if (cachedFilterSchema is null || filter != cachedFilter)
                {
                    cachedFilterSchema = (from schema in Schemas 
                                          where schema.def.label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 
                                          select schema).ToList();
                    cachedFilter = filter;
                }
                return cachedFilterSchema;
            }
        }

        public List<SorcerySchema> FilteredFavSchemas
        {
            get
            {
                if (filter.NullOrEmpty()) return FavSchemas;
                if (cachedFilterFavSchema is null || filter != cachedFilter)
                {
                    cachedFilterFavSchema = (from schema in FilteredSchemas 
                                             where schema.favorited 
                                             select schema).ToList();
                    cachedFilter = filter;
                }
                return cachedFilterFavSchema;
            }
        }

        private Comp_ItsSorcery sorceryComp = null;

        private List<SorcerySchema> cachedFilterSchema;

        private List<SorcerySchema> cachedFilterFavSchema;

        public int energyTrackerIndex = 0;

        public int currentPage;

        public int possiblePages;

        public int possibleSlots;

        private Vector2 schemaScrollPosition = Vector2.zero;

        private float schemaScrollViewHeight;

        private string filter = "";

        private string cachedFilter = "";

        private bool favView = false;
    }
}
