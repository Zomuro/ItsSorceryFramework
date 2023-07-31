using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTracker_InvertedTurnBased : EnergyTracker_Inverted
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_InvertedTurnBased(Pawn pawn) : base(pawn)
        {
        }

        public EnergyTracker_InvertedTurnBased(Pawn pawn, EnergyTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool HasTurn
        {
            get
            {
                return true;
            }
        }

        public override int TurnTicks
        {
            get
            {
                return Math.Max(def.turnTicks, 1);
            }
        }

        public override void EnergyTrackerTick()
        {
            if (Find.TickManager.TicksGame % TurnTicks == 0)
            {
                float tempEnergy;
                if (currentEnergy < 0)
                {
                    tempEnergy = Math.Min(currentEnergy + EnergyRecoveryRate * UnderBarRecoveryFactor,
                        MaxEnergy);
                }
                else if (currentEnergy <= MaxEnergy) // when energy is under or equal the normal max
                {
                    tempEnergy = Math.Min(currentEnergy - EnergyRecoveryRate, MaxEnergy);
                }
                else // when energy is over the normal max
                {
                    tempEnergy = Math.Min(currentEnergy - EnergyRecoveryRate * OverBarRecoveryFactor,
                        OverMaxEnergy);
                }

                this.currentEnergy = Math.Max(tempEnergy, MinEnergy);

                if (Find.Selector.FirstSelectedObject == pawn && pawn.Drafted && Schema.turnTimerOn) Find.TickManager.Pause();
                
            }

            countdownTick = Find.TickManager.TicksGame % TurnTicks;
        }

        public override float DrawOnGUI(ref Rect rect)
        {
            // get original rect
            Rect orgRect = new Rect(rect);
            float coordY = 0;

            // add space
            coordY += 10;
            rect.y += coordY;

            // set up label and bar rects
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect labelBox = new Rect(rect);
            labelBox.width = rect.width / 2;
            labelBox.ContractedBy(5);
            Rect barBox = new Rect(labelBox);
            barBox.x = rect.width * 2 / 5 + rect.x;

            // energy label
            Widgets.LabelCacheHeight(ref labelBox, def.energyLabelKey.Translate().CapitalizeFirst());

            // draws power bar
            barBox.height = labelBox.height; // set barbox to labelbox height for consistency
            DrawEnergyBar(barBox);

            // draw amount of energy
            string energyLabel = this.currentEnergy.ToString("F0") + " / " + this.MaxEnergy.ToString("F0");
            string countdown = " (" + (TurnTicks - countdownTick).ToStringSecondsFromTicks() + ")";
            Widgets.Label(barBox, energyLabel + countdown);
            Text.Anchor = TextAnchor.UpperLeft;

            // highlight energy costs
            HightlightEnergyCost(barBox);

            // add label/barbox height
            coordY += labelBox.height;
            // reset rectangle
            rect = orgRect;
            // return accumulated height
            return coordY;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            // see EnergyTracker_RPG.SpecialDisplayStats(req)
            // adds all the entries from that method into this one
            foreach (StatDrawEntry entry in base.SpecialDisplayStats(req))
            {
                yield return entry;
            }

            StatCategoryDef finalCat = tempStatCategory is null ? StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF : tempStatCategory;

            // returns how long a "turn" takes (time before auto-pause when the pawn with this energytracker is drafted)
            yield return new StatDrawEntry(finalCat,
                    def.turnInfoKey.Translate(), def.turnTicks.TicksToSeconds().ToString(),
                    def.turnInfoDescKey.Translate(),
                    20, null, null, false);

        }

        public int countdownTick = 0;

    }
}
