using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;

namespace ItsSorceryFramework
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Zomuro.ItsSorcery.Framework");

            // FloatMenu_EnergyTracker_Consumable
            // if a pawn has a SorcerySchema with a Consumable class EnergyTracker, show the float menu
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHumanlikeOrders_EnergyTracker_Consumable)));

           
        }

        // POSTFIX: when right clicking items that can reload the schema, provide FloatMenu option to "reload" with them
        public static void AddHumanlikeOrders_EnergyTracker_Consumable(Vector3 __0, Pawn __1, List<FloatMenuOption> __2)
        {
            Comp_ItsSorcery comp = __1.TryGetComp<Comp_ItsSorcery>() as Comp_ItsSorcery;
            String text = "";
            foreach (SorcerySchema schema in from schema in comp.schemaTracker.sorcerySchemas
                                         where schema.energyTracker.GetType() == typeof(EnergyTracker_Consumable)
                                         select schema)
            {
                EnergyTracker energyTracker = schema.energyTracker;
                Dictionary<ThingDef, float> ammoRef = schema.energyTracker.def.sorceryAmmoDict;
                foreach (ThingDef thingDef in ammoRef.Keys)
                {
                    Thing ammo = __0.ToIntVec3().GetFirstThing(__1.Map, thingDef);
                    if (ammo == null)
                    {
                        continue;
                    }

                    if (!__1.CanReach(ammo, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                    {
                        text = "ISF_Charge".Translate(schema.def.LabelCap.ToString(), ammo.def.LabelCap.ToString())
                            + "ISF_ChargeNoPath".Translate();
                        __2.Add(new FloatMenuOption(text, null, MenuOptionPriority.Default,
                            null, null, 0f, null, null, true, 0));
                    }
                    else if (energyTracker.MaxEnergy != 0 &&
                        energyTracker.currentEnergy == energyTracker.MaxEnergy)
                    {
                        text = "ISF_Charge".Translate(schema.def.LabelCap.ToString(), ammo.def.LabelCap.ToString())
                            + "ISF_ChargeFull".Translate();
                        __2.Add(new FloatMenuOption(text, null, MenuOptionPriority.Default, 
                            null, null, 0f, null, null, true, 0));
                    }
                    else
                    {
                        int count = 0;
                        int endcount = ammo.stackCount;
                        float gain = endcount * ammoRef[ammo.def];
                        if (energyTracker.MaxEnergy == 0)
                        {
                            text = "ISF_Charge".Translate(schema.def.LabelCap.ToString(), ammo.def.ToString())
                            + "ISF_ChargeCalc".Translate(ammo.stackCount, ammo.def.ToString(),
                                ammo.stackCount * ammoRef[ammo.def],
                                energyTracker.def.energyLabelTranslationKey.Translate());
                        }
                        else
                        {
                            count = (int)Math.Ceiling((energyTracker.MaxEnergy - energyTracker.currentEnergy) / ammoRef[ammo.def]);
                            endcount = Math.Min(count, ammo.stackCount);
                            gain = Math.Min(endcount * ammoRef[ammo.def], energyTracker.MaxEnergy - energyTracker.currentEnergy);
                            text = "ISF_Charge".Translate(schema.def.LabelCap.ToString(), ammo.def.ToString())
                            + "ISF_ChargeCalc".Translate(endcount, ammo.def.ToString(),
                                gain, energyTracker.def.energyLabelTranslationKey.Translate());
                        }
                            
                        Action chargeSchema = delegate ()
                        {
                            __1.jobs.TryTakeOrderedJob(JobGiver_Charge.MakeChargeEnergyJob(__1, schema, ammo, endcount), 
                                new JobTag?(JobTag.Misc), false);
                        };
                        __2.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, chargeSchema, 
                            MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), __1, ammo, "ReservedBy", null));
                    }
                }
            }

            return;
        }

    }
}
