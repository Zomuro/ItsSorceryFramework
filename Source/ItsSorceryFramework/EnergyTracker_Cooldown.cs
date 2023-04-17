using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    // unused energytracker class- its role is basically made obselete by ability cooldowns.
    public class EnergyTracker_Cooldown : EnergyTracker
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_Cooldown(Pawn pawn) : base(pawn)
        {

        }

        public EnergyTracker_Cooldown(Pawn pawn, EnergyTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref recentSorceries, "recentSorceries", LookMode.Def);
            Scribe_Values.Look(ref tickCount, "tickCount");
        }

        public override void EnergyTrackerTick()
        {
            if (tickCount > 0) tickCount--;
        }

        public override bool WouldReachLimitEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (tickCount > 0) return true;
            return false;
        }

        public override bool TryAlterEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (!WouldReachLimitEnergy(energyCost, sorceryDef, sorcery))
            {
                tickCount = this.def.refreshTicks;
                if (recentSorceries.Contains(sorcery)) recentSorceries.Remove(sorcery);
                else if (recentSorceries.Count >= 6) recentSorceries.RemoveLast();
                recentSorceries.Insert(0, sorcery);
                return true;
            }
            
            return false;
        }

        public override float DrawOnGUI(ref Rect rect)
        {
            // get original rect
            Rect orgRect = new Rect(rect);
            float coordY = 0;

            coordY += 10; // add space
            rect.y += coordY;

            // refresh label
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            // if tickcount > 0
            if (tickCount > 0) 
            {
                Widgets.LabelCacheHeight(ref rect, def.cooldownKey.Translate(GenDate.ToStringTicksToPeriod(tickCount)));
                coordY += rect.height + 10;
            } 
            // if recent sorceries are none
            else if (recentSorceries.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "No sorceries cast yet.");
                coordY += rect.height + 10;
            }
            // otherwise if the pawn has cast a sorcery
            else
            {
                Rect stackRect = rect.ContractedBy(16f);

                GenUI.DrawElementStack<Sorcery>(stackRect, 32f, this.recentSorceries, delegate (Rect r, Sorcery sorcery)
                {
                    GUI.DrawTexture(r, BaseContent.ClearTex);
                    if (Mouse.IsOver(r))
                    {
                        Widgets.DrawHighlight(r);
                        TipSignal tip = new TipSignal(sorcery.SorceryTooltip);
                        TooltipHandler.TipRegion(r, tip);
                    }
                    if (Widgets.ButtonImage(r, sorcery.def.uiIcon, false))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(sorcery.sorceryDef, null));
                    }
                }, (Sorcery sorcery) => 32f, 10f, 10f, true);
                GUI.color = Color.white;
                coordY += stackRect.height + 10;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            rect = orgRect; // reset rectangle
            return coordY; // return accumulated height
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            StatCategoryDef finalCat = tempStatCategory is null ? StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF : tempStatCategory;
            yield return new StatDrawEntry(finalCat,
                    def.refreshInfoKey.Translate(), def.refreshTicks.TicksToSeconds().ToString(),
                    def.refreshInfoDescKey.Translate(),
                    10, null, null, false);
        }

        public override string DisableCommandReason()
        {
            return def.disableReasonKey ?? "ISF_CommandDisableReasonCooldown";
        }

        public override string TopRightLabel(SorceryDef sorceryDef)
        {
            /*if (sorceryDef.verbProperties.defaultCooldownTime <= 0) return "";
            return "T: " + sorceryDef.verbProperties.defaultCooldownTime;*/

            return "T: " + def.refreshTicks.TicksToSeconds();
        }

        public List<Sorcery> recentSorceries = new List<Sorcery> ();

        public int tickCount = 0;

    }
}
