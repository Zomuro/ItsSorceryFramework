using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class EnergyTracker_Vancian : EnergyTracker
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_Vancian(Pawn pawn) : base(pawn)
        {
        }

        public EnergyTracker_Vancian(Pawn pawn, EnergyTrackerDef def) : base(pawn, def)
        {
            InitalizeSorceries();
            tickCount = def.refreshTicks;
        }

        public EnergyTracker_Vancian(Pawn pawn, SorcerySchemaDef def) : base(pawn, def)
        {
            InitalizeSorceries();
            tickCount = this.def.refreshTicks;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref vancianCasts, "vancianCasts", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref tickCount, "tickCount");
        }

        public virtual void InitalizeSorceries()
        {
            foreach(SorceryDef sd in from sorceryDef in DefDatabase<SorceryDef>.AllDefs 
                                     where sorceryDef.sorcerySchema.energyTrackerDef == def
                                     select sorceryDef)
            {
                if(!vancianCasts.ContainsKey(sd)) vancianCasts.Add(sd, (int) Math.Ceiling(sd.MaximumCasts * CastFactor));
            }
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
            if(tickCount > 0)
            {
                tickCount--;
                if(tickCount == 0)
                {
                    tickCount = this.def.refreshTicks;
                    this.RefreshAllCasts();
                }
            }
        }

        public virtual void RefreshAllCasts()
        {
            Dictionary<SorceryDef, int> refreshed = new Dictionary<SorceryDef, int>();
            //int count = vancianCasts.Count();

            /*while(count > 0)
            {

            }*/

            foreach (var pair in vancianCasts)
            {
                //vancianCasts[key] = (int) Math.Ceiling(key.MaximumCasts * CastFactor);
                refreshed[pair.Key] = (int)Math.Ceiling(pair.Key.MaximumCasts * CastFactor);
            }

            vancianCasts = refreshed;
        }

        public override bool WouldReachLimitEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (vancianCasts[sorceryDef] <= 0) return true;
            return false;
        }

        public override bool TryAlterEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (!WouldReachLimitEnergy(energyCost, sorceryDef))
            {
                vancianCasts[sorceryDef]--;
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
            Text.Anchor = TextAnchor.UpperLeft;

            // add label/barbox height + add a small boundary space for appearance
            coordY += rect.height + 10;
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

            statDef = def.castFactorStatDef != null ? def.castFactorStatDef : StatDefOf_ItsSorcery.CastFactor_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                        statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    def.refreshInfoKey.Translate(), def.refreshTicks.TicksToSeconds().ToString(),
                    def.refreshInfoDescKey.Translate(),
                    10, null, null, false); 
        }

        public override string DisableCommandReason()
        {
            return def.disableReasonKey ?? "ISF_CommandDisableReasonVancian";
        }

        public override string TopRightLabel(SorceryDef sorceryDef)
        {
            return (sorceryDef?.sorcerySchema.energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst()[0]) + ": " +
                vancianCasts[sorceryDef].ToString() + "/" +
                ((int) Math.Ceiling(sorceryDef.MaximumCasts * this.CastFactor)).ToString();
        }

        public Dictionary<SorceryDef, int> vancianCasts = new Dictionary<SorceryDef, int> ();

        public int tickCount = 0;

    }
}
