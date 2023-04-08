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
                return SelPawn.IsColonist && SorceryComp != null && !HashSchemas.EnumerableNullOrEmpty();
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

            HashSet<SorcerySchema> viewedSchemas = favView ? HashFilteredFavSchemas : HashFilteredSchemas;
            Rect viewScroll = new Rect(viewSchema.x, viewSchema.y, viewSchema.width - 20, schemaScrollViewHeight + 10f);

            // calculate the number of "pages" and schemas we can fit into the itab + sets current page
            possibleSlots = 5;
            possiblePages = (int) Math.Ceiling((1f* viewedSchemas.Count()) / possibleSlots);
            currentPage = schemaIndex / possibleSlots + 1;
            Text.Font = GameFont.Small;

            // draw page count + searchbar and favorites button, page change buttons, favorites, and schemas
            DrawPageUI();
            FavViewButton(DrawSearchBox(schemaRect.x) + 5, 0);
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
            if (currentPage > 1 && Widgets.ButtonText(button1, "<")) schemaIndex -= possibleSlots;
            // as long as it isn't the last page, can move forwards
            if (currentPage < possiblePages && Widgets.ButtonText(button2, ">")) schemaIndex += possibleSlots;

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

        public float DrawSearchBox(float xPos)
        {
            filter = Widgets.TextField(new Rect(xPos, 0, TabRect.width / 4 - (xPos - TabRect.x), 26), filter);
            if (Event.current.type == EventType.MouseDown || Event.current.keyCode == KeyCode.Escape)
            {
                GUI.FocusControl(null);
            }

            return TabRect.width / 4 + TabRect.x;
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

        public void DrawSchemas(Rect view, Rect viewScroll, Rect schemaRect, HashSet<SorcerySchema> viewedSchemas)
        {
            float totalSchemaHeight = 0;
            Widgets.BeginScrollView(view, ref schemaScrollPosition, viewScroll, true);

            // for every sorcery schema
            SorcerySchema[] schemas = viewedSchemas.ToArray();

            for(int i = schemaIndex; i <= schemaIndex + Math.Min(schemas.Count() - schemaIndex, 5) - 1; i++)
            {
                //float schemaHeight = schemas[i].energyTracker.DrawOnGUI(ref schemaRect);

                float schemaHeight = schemas[i].DrawOnGUI(ref schemaRect);
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

        public HashSet<SorcerySchema> HashSchemas
        {
            get
            {
                if (SorceryComp is null) return null;
                if (cachedHashSchema is null || cacheCount != SorceryComp.schemaTracker.sorcerySchemas.Count())
                {
                    cachedHashSchema = new HashSet<SorcerySchema>(SorceryComp.schemaTracker.sorcerySchemas);
                    cacheCount = cachedHashSchema.Count();
                }
                return cachedHashSchema;
            }
        }

        public HashSet<SorcerySchema> HashFavSchemas
        {
            get
            {
                return HashSchemas.Where(x => x.favorited).ToHashSet() ?? new HashSet<SorcerySchema>();
            }
        }

        public HashSet<SorcerySchema> HashFilteredSchemas
        {
            get
            {
                if (filter.NullOrEmpty()) return HashSchemas;
                if (cachedHashFilterSchema is null || filter != cachedFilter)
                {
                    cachedHashFilterSchema = HashSchemas.Where(x => x.def.label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToHashSet();
                    cachedFilter = filter;
                }
                return cachedHashFilterSchema;
            }
        }

        public HashSet<SorcerySchema> HashFilteredFavSchemas
        {
            get
            {
                if (filter.NullOrEmpty()) return HashFavSchemas;
                return HashFilteredSchemas.Where(x => x.favorited).ToHashSet();
            }
        }

        private Comp_ItsSorcery sorceryComp = null;


        // Hashset versions
        private HashSet<SorcerySchema> cachedHashSchema;

        private int cacheCount = 0;

        private HashSet<SorcerySchema> cachedHashFilterSchema;

        //public int energyTrackerIndex = 0;

        public int schemaIndex = 0;

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
