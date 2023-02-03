using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTracker_Cooldown : EnergyTracker
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_Cooldown(Pawn pawn) : base(pawn)
        {

        }

        public EnergyTracker_Cooldown(Pawn pawn, EnergyTrackerDef def) : base(pawn, def)
        {

        }

        public EnergyTracker_Cooldown(Pawn pawn, SorcerySchemaDef def) : base(pawn, def)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref recentSorceries, "recentSorceries", LookMode.Def);
            Scribe_Values.Look(ref tickCount, "tickCount");
        }


        public virtual float CastFactor
        {
            get
            {
                return this.pawn.GetStatValue(def.castFactorStatDef ?? StatDefOf_ItsSorcery.CastFactor_ItsSorcery, true);
            }
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
                //sorceryDef.uiIcon;
                if (recentSorceries.Contains(sorcery)) recentSorceries.Remove(sorcery);
                else if (recentSorceries.Count >= 6) recentSorceries.RemoveLast();
                recentSorceries.Insert(0, sorcery);
                return true;
            }
            
            return false;
        }

        public override void DrawOnGUI(Rect rect)
        {
            this.SchemaViewBox(rect);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            
            if(tickCount > 0) Widgets.Label(rect, def.cooldownKey.Translate(GenDate.ToStringTicksToPeriod(tickCount)));
            else if (recentSorceries.NullOrEmpty())
            {
                Widgets.Label(rect, "No sorceries cast yet.");
            }
            else
            {
                Rect stackRect = rect.ContractedBy(16f);
                stackRect.yMin = rect.y + rect.height / 3 + 2;

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
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override float DrawOnGUI(ref Rect rect)
        {
            // get original rect
            Rect orgRect = new Rect(rect);
            float coordY = 0;

            // draws info, learningtracker buttons + schema title
            coordY += SchemaViewBox(ref rect);

            // add space
            coordY += 10;
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
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
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
            if (sorceryDef.verbProperties.defaultCooldownTime <= 0) return "";
            return "T: " + sorceryDef.verbProperties.defaultCooldownTime;
        }

        public List<Sorcery> recentSorceries = new List<Sorcery> ();

        public int tickCount = 0;

    }
}
