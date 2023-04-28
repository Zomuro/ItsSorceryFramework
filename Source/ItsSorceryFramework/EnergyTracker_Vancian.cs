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

        public EnergyTracker_Vancian(Pawn pawn, EnergyTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {
            InitalizeSorceries();
            tickCount = def.refreshTicks;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref vancianCasts, "vancianCasts", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref tickCount, "tickCount");

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                InitalizeSorceries();
            }
        }

        /*public StatDef MaxCastStatDef
        {
            get
            {
                return def.energyMaxCastStatDef is null ? StatDefOf_ItsSorcery.Sorcery_MaxCasts : def.energyMaxCastStatDef;
            }
        }*/

        public virtual void InitalizeSorceries()
        {
            /*foreach(SorceryDef sd in from sorceryDef in DefDatabase<SorceryDef>.AllDefs 
                                     where sorceryDef.sorcerySchema.energyTrackerDefs.Contains(def)
                                     select sorceryDef)
            {
                if(!vancianCasts.ContainsKey(sd)) vancianCasts.Add(sd, (int) Math.Ceiling(sd.MaximumCasts * CastFactor));
            }*/

            //float maxCastsTemp = 0;

            foreach(var sorceryDef in from sorceryDef in DefDatabase<SorceryDef>.AllDefs
                                      where sorceryDef.sorcerySchema == sorcerySchemaDef && SorceryDefMaxCasts(sorceryDef) > 0
                                      select sorceryDef)
            {
                if (!vancianCasts.ContainsKey(sorceryDef))
                {
                    vancianCasts.Add(sorceryDef, (int)Math.Ceiling(SorceryDefMaxCasts(sorceryDef) * CastFactor));
                } 
            }

            vancianCasts = CleanVancianCasts(vancianCasts);

            Log.Message("count all: " + DefDatabase<SorceryDef>.AllDefs.Count());
            foreach(var sorcery in DefDatabase<SorceryDef>.AllDefs)
            {
                Log.Message(sorcery.label + " same schema: " + (sorcery.sorcerySchema == sorcerySchemaDef));
                Log.Message(sorcery.label + " max casts: " + SorceryDefMaxCasts(sorcery));
            }
            Log.Message("all sorceries: " + vancianCasts.Count);
        }

        public int SorceryDefMaxCasts(SorceryDef sorceryDef)
        {
            return (int) Mathf.Ceil(sorceryDef.statBases.GetStatValueFromList(def.energyMaxCastStatDef ?? StatDefOf_ItsSorcery.Sorcery_MaxCasts, 0));
        }

        public Dictionary<SorceryDef, int> CleanVancianCasts(Dictionary<SorceryDef, int> original)
        {
            IEnumerable<SorceryDef> allSorceries = DefDatabase<SorceryDef>.AllDefs;
            Dictionary<SorceryDef, int> temp = new Dictionary<SorceryDef, int>();
            foreach (var pair in original) // check if the sorcery was removed or not- otherwise not add to list
            {
                if (allSorceries.Contains(pair.Key)) temp.Add(pair.Key, pair.Value);
            }

            return temp;
        }

        public override void EnergyTrackerTick()
        {
            if(tickCount > 0)
            {
                tickCount--;
                if(tickCount == 0)
                {
                    tickCount = this.def.refreshTicks;
                    RefreshAllCasts();
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
                refreshed[pair.Key] = (int)Math.Ceiling(SorceryDefMaxCasts(pair.Key) * CastFactor);
            }

            vancianCasts = refreshed;
        }

        public override bool WouldReachLimitEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (vancianCasts[sorceryDef] - (int) (energyCost) < 0) return true;
            return false;
        }

        public override bool TryAlterEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (!WouldReachLimitEnergy(energyCost, sorceryDef))
            {
                vancianCasts[sorceryDef] -= (int) (energyCost);
                return true;
            }
            
            return false;
        }

        public override float DrawOnGUI(ref Rect rect)
        {
            // get original rect
            Rect orgRect = new Rect(rect);
            float coordY = 0;

            // add space
            coordY += 10;
            rect.y += coordY;

            // refresh label
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.LabelCacheHeight(ref rect, def.refreshNotifKey.Translate(GenDate.ToStringTicksToPeriod(tickCount)));
            Text.Anchor = TextAnchor.UpperLeft;

            // add label/barbox height + add a small boundary space for appearance
            coordY += rect.height; // + 10;
            // reset rectangle
            rect = orgRect;
            // return accumulated height
            return coordY;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            StatDef statDef;
            StatRequest pawnReq = StatRequest.For(pawn);

            StatCategoryDef finalCat = tempStatCategory is null ? StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF : tempStatCategory;

            yield return new StatDrawEntry(finalCat,
                        "ISF_EnergyTrackerMaxUnit".Translate(), def.energyMaxCastStatDef.LabelCap,
                        def.energyMaxCastStatDef.description, 99999, null, null, false);

            statDef = def.castFactorStatDef != null ? def.castFactorStatDef : StatDefOf_ItsSorcery.CastFactor_ItsSorcery;
            yield return new StatDrawEntry(finalCat,
                        statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            yield return new StatDrawEntry(finalCat,
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
            // reminder: implement string caching of sorceryDefMaxCasts
            if (vancianCasts.ContainsKey(sorceryDef))
            {
                return (def.energyLabelKey.Translate().CapitalizeFirst()[0]) + ": " +
                                (vancianCasts[sorceryDef]).ToString() + "/" +
                                ((int)Math.Ceiling(SorceryDefMaxCasts(sorceryDef) * CastFactor)).ToString();
            }
            else return "";
            
        }

        public Dictionary<SorceryDef, int> vancianCasts = new Dictionary<SorceryDef, int> ();

        public int tickCount = 0;

    }
}
