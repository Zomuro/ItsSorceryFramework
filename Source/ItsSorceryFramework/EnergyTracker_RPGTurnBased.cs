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

                if (Find.Selector.FirstSelectedObject == pawn && pawn.Drafted) Find.TickManager.Pause();
                
            }

            countdownTick = Find.TickManager.TicksGame % TurnTicks;
        }

        public override void DrawOnGUI(Rect rect)
        {
            this.SchemaViewBox(rect);

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

            Widgets.Label(labelBox, sorcerySchemaDef.energyTrackerDef.EnergyLabelTranslationKey.Translate().CapitalizeFirst());

            // draws power bar
            DrawEnergyBar(barBox);

            string energyLabel = this.currentEnergy.ToString("F0") + " / " + this.MaxEnergy.ToString("F0");
            string countdown = " ("+ (TurnTicks - countdownTick).ToStringSecondsFromTicks()+")";
            Widgets.Label(barBox, energyLabel + countdown);

            Widgets.DrawBoxSolidWithOutline(rect, Color.clear, Color.grey);
            Text.Anchor = TextAnchor.UpperLeft;

            HightlightEnergyCost(barBox);
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
                    def.TurnInfoTranslationKey.Translate(), def.turnTicks.TicksToSeconds().ToString(),
                    def.TurnInfoDescTranslationKey.Translate(),
                    20, null, null, false);

        }


        public int countdownTick = 0;

    }
}
