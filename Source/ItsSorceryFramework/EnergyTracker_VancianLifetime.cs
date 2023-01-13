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

        /*public virtual int RefreshTick
        {
            get
            {
                return def.refreshTicks % 60000;
            }
        }*/

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
            Widgets.Label(rect, def.RefreshNotifTranslationKey.Translate(GenDate.ToStringTicksToPeriod(tickCount)));

            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(rect, def.CastsCountTranslationKey.Translate(currentCasts, MaxCasts));

            Text.Anchor = TextAnchor.UpperLeft;
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
                    def.RefreshInfoTranslationKey.Translate(), def.refreshTicks.TicksToSeconds().ToString(),
                    def.RefreshInfoDescTranslationKey.Translate(),
                    10, null, null, false);
        }

        public override string TopRightLabel(SorceryDef sorceryDef)
        {
            return (sorceryDef?.sorcerySchema.energyTrackerDef.EnergyLabelTranslationKey.Translate().CapitalizeFirst()[0]) + ": " +
                currentCasts.ToString() + "/" + MaxCasts.ToString();
        }

        public int tickCount = 0;

    }
}
