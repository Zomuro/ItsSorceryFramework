using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class EnergyTrackerComp_OnConsume : EnergyTrackerComp
    {
        public EnergyTrackerCompProperties_OnConsume Props => (EnergyTrackerCompProperties_OnConsume)props;

        public Pawn Pawn => parent.pawn;

        public bool AtLimit => parent.schema.limitLocked && (!parent.def.inverse ? parent.currentEnergy >= parent.MaxEnergy : parent.currentEnergy <= parent.MinEnergy);
        
        public override IEnumerable<FloatMenuOption> CompPostConsume(Vector3 vec3) // for effects when using a "consumption" item
        {
            if (Props.consumables.NullOrEmpty()) yield break; // no consumables to look for = don't look for them.

            String text = "";
            foreach (var consume in Props.consumables) // for each consumable type
            {
                // find the consumable at the location; if not, skip to the next item in the list.
                Thing ammo = vec3.ToIntVec3().GetFirstThing(Pawn.Map, consume.thingDef);
                if (ammo is null) continue;

                if (!Pawn.CanReach(ammo, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn)) // No path = no consumption.
                {
                    text = "ISF_Charge".Translate(parent.schema.def.LabelCap.ToString(), parent.def.LabelCap, ammo.def.label)
                        + "ISF_ChargeNoPath".Translate();
                    yield return new FloatMenuOption(text, null, MenuOptionPriority.Default,
                        null, null, 0f, null, null, true, 0);
                }
                else if (AtLimit) // if limit locked AND curr energy > max or curr energy < min (depending on inverse; i.e. 100 >= 50)
                {
                    text = "ISF_Charge".Translate(parent.schema.def.LabelCap.ToString(), parent.def.LabelCap, ammo.def.label)
                        + "ISF_ChargeFull".Translate();
                    yield return new FloatMenuOption(text, null, MenuOptionPriority.Default,
                        null, null, 0f, null, null, true, 0);
                }
                else
                {
                    int count = 0;
                    float energyDiff = 0;
                    int endcount = ammo.stackCount;
                    float gain = endcount * consume.energy;

                    if (!parent.def.inverse)
                    {
                        energyDiff = Mathf.Max(0f, parent.schema.limitLocked ? parent.MaxEnergy - parent.currentEnergy : parent.AbsMaxEnergy - parent.currentEnergy);
                        count = (int)Math.Ceiling(energyDiff / consume.energy);
                        endcount = Math.Min(count, ammo.stackCount);
                        gain = Math.Min(endcount * consume.energy, energyDiff);

                        text = "ISF_Charge".Translate(parent.schema.def.LabelCap.ToString(), parent.def.LabelCap, ammo.def.label)
                        + "ISF_ChargeCalc".Translate(endcount, ammo.def.label,
                            gain, parent.def.energyLabelKey.Translate());
                    }
                    else
                    {
                        energyDiff = Mathf.Max(0f, parent.schema.limitLocked ? parent.currentEnergy - parent.MinEnergy: parent.currentEnergy - parent.AbsMinEnergy);
                        count = (int)Math.Ceiling(energyDiff / consume.energy);
                        endcount = Math.Min(count, ammo.stackCount);
                        gain = Math.Min(endcount * consume.energy, energyDiff);

                        text = "ISF_Charge".Translate(parent.schema.def.LabelCap.ToString(), parent.def.LabelCap, ammo.def.label)
                        + "ISF_ChargeCalc".Translate(endcount, ammo.def.label,
                            gain, parent.def.energyLabelKey.Translate());
                    }

                    Action chargeSchema = delegate ()
                    {
                        Job job = JobGiver_Charge.MakeChargeEnergyJob(parent.pawn, parent.schema, parent.def, ammo, endcount, consume.energy);
                        parent.pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, chargeSchema,
                        MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), parent.pawn, ammo, "ReservedBy", null);
                }

            }


            yield break;
        }

        public override IEnumerable<StatDrawEntry> CompSpecialDisplayStats(StatRequest req, StatCategoryDef catDef = null) // provides special display stats, which show how energy gets recovered
        {
            //StatDef statDef;
            //StatRequest pawnReq = StatRequest.For(parent.pawn);
            StatCategoryDef finalCat = catDef ?? StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF;

            String ammo = "";
            foreach (var item in Props.consumables)
            {
                if (ammo == "")
                {
                    ammo = item.thingDef.LabelCap + " ({0})".Translate(item.energy);
                }
                else ammo = ammo + ", " + item.thingDef.LabelCap + " ({0})".Translate(item.energy);
            }
            if (ammo == "") ammo = "None";

            yield return new StatDrawEntry(finalCat,
                    "ISF_EnergyTrackerAmmo".Translate(), ammo,
                    "ISF_EnergyTrackerAmmoDesc".Translate(),
                    10, null, null, false);

            yield break;
        }

    }

}
