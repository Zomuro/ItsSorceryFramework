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
                return base.SelPawn.RaceProps.Humanlike &&
                    SorceryComp != null /*&& !SorceryComp.schemaTracker.sorcerySchemas.NullOrEmpty()*/;
            }
        }

        public ITab_Pawn_Schemas()
        {
            this.size = new Vector2(460f, 450f);
            this.labelKey = "ISF_TabSchemas";
            this.tutorTag = "ISF_TabSchemas";
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Medium;

            Rect window = new Rect(0, 30, size.x, size.y);
            Rect view = window.ContractedBy(10f);
            Rect schemaRect = view.ContractedBy(8f);
            schemaRect.height = 75f;

            // no magic systems => "no schemas"
            if (SorceryComp.schemaTracker.sorcerySchemas.NullOrEmpty())
            {
                Widgets.Label(view, "No schemas.");
                return;
            }

            List<EnergyTracker> energyTrackers = (from schema in SorceryComp.schemaTracker.sorcerySchemas
                                                  select schema.energyTracker).ToList();

            int possibleSlots = (int) Math.Floor((size.y - 48) / 75f);
            int possiblePages = (int) Math.Ceiling((1f*energyTrackers.CountAllowNull()) / possibleSlots);

            int currentPage = energyTrackerIndex / possibleSlots + 1;
            Text.Font = GameFont.Small;
            Rect button1 = new Rect(size.x / 2 - 25 - 50, 10, 50, 25);
            Rect button2 = new Rect(size.x / 2 + 25, 10, 50, 25);
            Rect pageLabel = new Rect(size.x / 2 - 25, 10, 50, 25);
            if (currentPage > 1 && Widgets.ButtonText(button1, "<"))
            {
                energyTrackerIndex -= possibleSlots;
            }
            if (currentPage < possiblePages && Widgets.ButtonText(button2, ">"))
            {
                energyTrackerIndex += possibleSlots;
            }

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(pageLabel, currentPage.ToString() + " / " + possiblePages.ToString());
            Text.Anchor = TextAnchor.UpperLeft;

            foreach (EnergyTracker et in energyTrackers.GetRange(energyTrackerIndex,
                Math.Min(energyTrackers.Count() - energyTrackerIndex, 5)))
            {
                et.DrawOnGUI(schemaRect);
                schemaRect.y += schemaRect.height + 1;
            }
            Text.Font = GameFont.Small;
        }

        public Comp_ItsSorcery SorceryComp
        {
            get
            {
                if(sorceryComp == null)
                {
                    sorceryComp = base.SelPawn.GetComp<Comp_ItsSorcery>();
                }

                return sorceryComp;
            }
        }


        private Comp_ItsSorcery sorceryComp = null;

        public int energyTrackerIndex = 0;

    }
}
