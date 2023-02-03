using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTracker_VancianLifetime : EnergyTracker
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_VancianLifetime(Pawn pawn) : base(pawn)
        {
        }

        public EnergyTracker_VancianLifetime(Pawn pawn, EnergyTrackerDef def) : base(pawn, def)
        {
            currentEnergy = MaxCasts;
            tickCount = def.refreshTicks;
        }

        public EnergyTracker_VancianLifetime(Pawn pawn, SorcerySchemaDef def) : base(pawn, def)
        {
            currentEnergy = MaxCasts;
            tickCount = this.def.refreshTicks;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Collections.Look(ref vancianCasts, "vancianCasts", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref tickCount, "tickCount");
        }

        public virtual int MaxCasts
        {
            get
            {
                return (int) (this.pawn.GetStatValue(def.energyMaxStatDef ?? StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery, true));
            }
        }

        public virtual int CastRecoveryRate
        {
            get
            {
                return (int) (this.pawn.GetStatValue(def.energyRecoveryStatDef ?? StatDefOf_ItsSorcery.EnergyRecovery_ItsSorcery, true));
            }
        }

        public virtual int currentCasts
        {
            get
            {
                return (int)currentEnergy;
            }
        }

        public override void EnergyTrackerTick()
        {
            if(tickCount > 0)
            {
                tickCount--;
                if(tickCount == 0)
                {
                    tickCount = def.refreshTicks;
                    currentEnergy = Math.Max(0, Math.Min(currentCasts + CastRecoveryRate, MaxCasts));
                }
            }
        }

        public override bool WouldReachLimitEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (currentCasts <= 0) return true;
            return false;
        }

        public override bool TryAlterEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (!WouldReachLimitEnergy(energyCost, sorceryDef))
            {
                currentEnergy--;
                return true;
            }
            
            return false;
        }

        public override void DrawOnGUI(Rect rect)
        {
            this.SchemaViewBox(rect);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, def.refreshNotifKey.Translate(GenDate.ToStringTicksToPeriod(tickCount)));

            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(rect, def.castCountKey.Translate(currentCasts, MaxCasts));

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
            Widgets.LabelCacheHeight(ref rect, def.refreshNotifKey.Translate(GenDate.ToStringTicksToPeriod(tickCount)));
            // add label height + add a small boundary space for appearance
            coordY += rect.height + 5;
            rect.y += rect.height + 5;

            // show total casts left
            Widgets.LabelCacheHeight(ref rect, def.castCountKey.Translate(currentCasts, MaxCasts));
            // add label height + add a small boundary space for appearance
            coordY += rect.height + 10;
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
            StatDef statDef;

            StatRequest pawnReq = StatRequest.For(pawn);

            // shows the maximum energy of the whole sorcery schema
            statDef = def.energyMaxStatDef != null ? def.energyMaxStatDef : StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            // show recovery amount per refresh period
            statDef = def.energyRecoveryStatDef != null ? def.energyRecoveryStatDef : StatDefOf_ItsSorcery.EnergyRecovery_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            statDef = def.castFactorStatDef != null ? def.castFactorStatDef : StatDefOf_ItsSorcery.CastFactor_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                        statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    def.refreshInfoKey.Translate(), def.refreshTicks.TicksToSeconds().ToString(),
                    def.refreshInfoDescKey.Translate(),
                    10, null, null, false);
        }

        public override string TopRightLabel(SorceryDef sorceryDef)
        {
            return (sorceryDef?.sorcerySchema.energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst()[0]) + ": " +
                currentCasts.ToString() + "/" + MaxCasts.ToString();
        }

        public int tickCount = 0;

    }
}
