using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            
            if(tickCount > 0) Widgets.Label(rect, def.CooldownTranslationKey.Translate(GenDate.ToStringTicksToPeriod(tickCount)));
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

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    def.RefreshInfoTranslationKey.Translate(), def.refreshTicks.TicksToSeconds().ToString(),
                    def.RefreshInfoDescTranslationKey.Translate(),
                    10, null, null, false);
        }

        public override string DisableCommandReason()
        {
            return def.DisableReasonTranslationKey ?? "ISF_CommandDisableReasonCooldown";
        }

        public override string TopRightLabel(SorceryDef sorceryDef)
        {
            if (sorceryDef.verbProperties.defaultCooldownTime == null) return "";
            return "T: " + sorceryDef.verbProperties.defaultCooldownTime;
        }

        public List<Sorcery> recentSorceries = new List<Sorcery> ();

        public int tickCount = 0;

    }
}
