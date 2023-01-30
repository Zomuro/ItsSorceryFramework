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
    public class EnergyTracker_RPGTurnBased : EnergyTracker_RPG
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_RPGTurnBased(Pawn pawn) : base(pawn)
        {
        }

        public EnergyTracker_RPGTurnBased(Pawn pawn, EnergyTrackerDef def) : base(pawn, def)
        {
        }

        public EnergyTracker_RPGTurnBased(Pawn pawn, SorcerySchemaDef def) : base(pawn, def)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref countdownTick, "countdownTick");
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

                if (Find.Selector.FirstSelectedObject == pawn && pawn.Drafted && turnTimerOn) Find.TickManager.Pause();
                
            }

            countdownTick = Find.TickManager.TicksGame % TurnTicks;
        }

        public override void DrawOnGUI(Rect rect)
        {
            this.SchemaViewBox(rect);

            // draws limit toggle button
            if (MinEnergy < 0) LimitButton(rect.x + rect.width - 5 - 24, rect.y + 5);
            // draws turn pause toggle button
            TurnButton(rect.x + rect.width - 5 - 24 - 24, rect.y + 5);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect labelBox = new Rect(rect);
            labelBox.width = rect.width / 2;
            labelBox.y = rect.y + rect.height / 2;
            labelBox.height = 22;
            Rect barBox = new Rect(labelBox);
            barBox.x = rect.width * 2 / 5 + rect.x;
            barBox.y = labelBox.y;
            barBox.height = 22;

            Widgets.Label(labelBox, sorcerySchemaDef.energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst());

            // draws power bar
            DrawEnergyBar(barBox);

            string energyLabel = this.currentEnergy.ToString("F0") + " / " + this.MaxEnergy.ToString("F0");
            string countdown = " ("+ (TurnTicks - countdownTick).ToStringSecondsFromTicks()+")";
            Widgets.Label(barBox, energyLabel + countdown);

            Widgets.DrawBoxSolidWithOutline(rect, Color.clear, Color.grey);
            Text.Anchor = TextAnchor.UpperLeft;

            HightlightEnergyCost(barBox);
        }

        public override float DrawOnGUI(ref Rect rect)
        {
            // get original rect
            Rect orgRect = new Rect(rect);
            float coordY = 0;

            // draws info, learningtracker buttons + schema title
            coordY += SchemaViewBox(ref rect);
            // if energy system allows for using more energy than usual, show toggle button
            if (MinEnergy < 0) LimitButton(rect.x + rect.width - 5 - 24, rect.y + 5);
            // draws turn pause toggle button
            TurnButton(rect.x + rect.width - 5 - 24 - 24, rect.y + 5);

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
            Widgets.LabelCacheHeight(ref labelBox, sorcerySchemaDef.energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst());

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
            // add a small boundary space for appearance
            coordY += 10;
            // set rect y to original, and rect height to coordY
            rect.y = orgRect.y;
            rect.height = coordY;

            // draw outline of the entire rectangle when it's all done
            DrawOutline(rect, Color.grey, 1);

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

            // returns how long a "turn" takes (time before auto-pause when the pawn with this energytracker is drafted)
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    def.turnInfoKey.Translate(), def.turnTicks.TicksToSeconds().ToString(),
                    def.turnInfoDescKey.Translate(),
                    20, null, null, false);

        }


        public int countdownTick = 0;

    }
}
