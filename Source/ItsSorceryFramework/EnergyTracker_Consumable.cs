using System;
using System.Collections.Generic;
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

        public EnergyTracker_Consumable(Pawn pawn, EnergyTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void EnergyTrackerTick()
        {
            float tempEnergy = Math.Min(currentEnergy + 1.TicksToSeconds() * EnergyRecoveryRate, MaxEnergy);
            currentEnergy = Math.Max(tempEnergy, 0);
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

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            StatDef statDef;

            StatRequest pawnReq = StatRequest.For(pawn);

            StatCategoryDef finalCat = tempStatCategory is null ? StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF : tempStatCategory;

            // shows the maximum energy of the whole sorcery schema if max > 0 (thus isn't uncapped)
            statDef = def.energyMaxStatDef != null ? def.energyMaxStatDef : StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery;
            if (MaxEnergy > 0)
            {
                yield return new StatDrawEntry(finalCat,
                        statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);
            }
            else
            {
                yield return new StatDrawEntry(finalCat,
                    statDef.LabelForFullStatList, "∞",
                    statDef.description.Translate(),
                    statDef.displayPriorityInCategory, null, null, false);
            }

            // show recovery amount per refresh period
            statDef = def.energyRecoveryStatDef != null ? def.energyRecoveryStatDef : StatDefOf_ItsSorcery.EnergyRecovery_ItsSorcery; 
            if(EnergyRecoveryRate != 0)
            {
                yield return new StatDrawEntry(finalCat,
                    statDef, pawn.GetStatValue(statDef), pawnReq, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);
            }

            String ammo = "";
            foreach(var item in def.consumables)
            {
                if(ammo == "")
                {
                    ammo = item.thingDef.LabelCap + " ({0})".Translate(item.energy);
                }
                else ammo = ammo + ", "+ item.thingDef.LabelCap + " ({0})".Translate(item.energy);
            }
            if (ammo == "") ammo = "None";

            yield return new StatDrawEntry(finalCat,
                    "ISF_EnergyTrackerAmmo".Translate(), ammo,
                    "ISF_EnergyTrackerAmmoDesc".Translate(),
                    10, null, null, false);
        }



    }
}
