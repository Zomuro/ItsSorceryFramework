using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class EnergyTracker_RPGTurnBased : EnergyTracker_RPG
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_RPGTurnBased(Pawn pawn) : base(pawn)
        {
        }

        public EnergyTracker_RPGTurnBased(Pawn pawn, EnergyTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {
        }

        /*public EnergyTracker_RPGTurnBased(Pawn pawn, SorcerySchemaDef def) : base(pawn, def)
        {
        }*/

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref countdownTick, "countdownTick");
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
                    tempEnergy = Math.Min(currentEnergy + EnergyRecoveryRate, MaxEnergy);
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

            // draws info, learningtracker buttons + schema title
            //coordY += SchemaViewBox(ref rect);
            // if energy system allows for using more energy than usual, show toggle button
            //if (MinEnergy < 0) LimitButton(rect.x + rect.width - 5 - 24, rect.y + 5);
            //TurnButton(rect.x + rect.width - 5 - 24 - 24, rect.y + 5); // draws turn pause toggle button
            coordY += 10; // add space
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
            
            coordY += labelBox.height; // add label/barbox height
            /*coordY += 10; // add a small boundary space for appearance
            rect.y = orgRect.y; // set rect y to original, and rect height to coordY
            //rect.height = coordY;

            // draw outline of the entire rectangle when it's all done
            //DrawOutline(rect, Color.grey, 1);*/

            rect = orgRect; // reset rectangle
            return coordY; // return accumulated height
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {

            // see EnergyTracker_RPG.SpecialDisplayStats(req)
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
