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
    public class EnergyTracker_Consumable : EnergyTracker_RPG
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker_Consumable(Pawn pawn) : base(pawn)
        {
        }

        public EnergyTracker_Consumable(Pawn pawn, EnergyTrackerDef def) : base(pawn, def)
        {
        }

        public EnergyTracker_Consumable(Pawn pawn, SorcerySchemaDef def) : base(pawn, def)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void EnergyTrackerTick()
        {
            float tempEnergy = Math.Min(currentEnergy - 1.TicksToSeconds() * EnergyRecoveryRate / def.refreshTicks, MaxEnergy);
            this.currentEnergy = Math.Max(tempEnergy, 0);
        }

        public override bool WouldReachLimitEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (currentEnergy - energyCost < 0) return true;
            return false;
        }

        public override bool TryAlterEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            if (!WouldReachLimitEnergy(energyCost, sorceryDef))
            {
                currentEnergy = Math.Max(0, currentEnergy - energyCost);
                return true;
            }
            
            return false;
        }

        public override void DrawOnGUI(Rect rect)
        {
            if (MaxEnergy > 0) base.DrawOnGUI(rect);
            else
            {
                this.SchemaViewBox(rect);

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, sorcerySchemaDef.energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst() + ": " +
                    currentEnergy);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            StatDef statDef;

            StatRequest pawnReq = StatRequest.For(pawn);

            // shows the maximum energy of the whole sorcery schema if max > 0 (thus isn't uncapped)
            statDef = def.energyMaxStatDef != null ? def.energyMaxStatDef : StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery;
            if (MaxEnergy > 0)
            {
                yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                        statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);
            }
            else
            {
                yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef.LabelForFullStatList, "∞",
                    statDef.description.Translate(),
                    statDef.displayPriorityInCategory, null, null, false);
            }

            // show recovery amount per refresh period
            statDef = def.energyRecoveryStatDef != null ? def.energyRecoveryStatDef : StatDefOf_ItsSorcery.EnergyRecovery_ItsSorcery; 
            if(EnergyRecoveryRate != 0)
            {
                yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);
            }

            String ammo = "";
            foreach(var item in def.sorceryAmmoDict)
            {
                if(ammo == "")
                {
                    ammo = item.Key.LabelCap + " ({0})".Translate(item.Value);
                }
                else ammo = ammo + ", "+ item.Key.LabelCap + " ({0})".Translate(item.Value);
            }
            if (ammo == "") ammo = "None";

            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    "ISF_EnergyTrackerAmmo".Translate(), ammo,
                    "ISF_EnergyTrackerAmmoDesc".Translate(),
                    10, null, null, false);



        }



    }
}
