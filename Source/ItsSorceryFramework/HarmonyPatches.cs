﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Zomuro.ItsSorcery.Framework");

            // EnergyTracker Patches //

            // AddHumanlikeOrders_EnergyTracker_Consumable
            // if a pawn has a SorcerySchema with a Consumable class EnergyTracker, show the float menu
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHumanlikeOrders_EnergyTracker_Consumable)));

            // Ability Patches //

            // DefIconAbilities
            // allows DefIcon to show abilitydef icons
            harmony.Patch(AccessTools.Method(typeof(Widgets), "DefIcon"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DefIconAbilities)));

            // ProgressEXPWorker Patches

            // TakeDamage_AddEXP
            // for every magic system with the correct EXP tag, give xp depending on damage
            harmony.Patch(AccessTools.Method(typeof(Thing), "TakeDamage"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(TakeDamage_AddEXP)));

            // SkillLearn_AddEXP
            // for every magic system with the correct EXP tag, give xp depending on skill being learned
            harmony.Patch(AccessTools.Method(typeof(SkillRecord), "Learn"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Learn_AddEXP)));

            // DoKillSideEffects_AddEXP
            // for every magic system with the correct EXP tag, give xp on kill
            harmony.Patch(AccessTools.Method(typeof(Pawn), "DoKillSideEffects"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DoKillSideEffects_AddEXP)));

            // AddHumanlikeOrders_EXPUseItem
            // allow items to be used to level up experience systems
            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHumanlikeOrders_EXPUseItem)));

            // PawnGen Patches //

            // GenerateNewPawnInternal_Schema
            // adds sorcery schema based on pawnkind def
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateNewPawnInternal"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerateNewPawnInternal_Schema)));

            // GainTrait_Schema
            // adds sorcery schema through trait
            harmony.Patch(AccessTools.Method(typeof(TraitSet), "GainTrait"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(GainTrait_Schema)));

            // AddGene_Schema
            // adds sorcery schema through gene
            harmony.Patch(AccessTools.Method(typeof(Pawn_GeneTracker), "AddGene", new[] { typeof(GeneDef), typeof(bool) }), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(AddGene_Schema)));

        }

        // POSTFIX: when right clicking items that can reload the schema, provide FloatMenu option to "reload" with them
        public static void AddHumanlikeOrders_EnergyTracker_Consumable(Vector3 __0, Pawn __1, List<FloatMenuOption> __2)
        {
            List<SorcerySchema> schemas = SorcerySchemaUtility.GetSorcerySchemaList(__1);
            if (schemas.NullOrEmpty()) return;

            foreach (SorcerySchema schema in schemas)
            {
                EnergyTracker_AddOrders(schema, __0, __1, __2);
            }
            return;
        }

        // HELPER METHOD
        public static void EnergyTracker_AddOrders(SorcerySchema schema, Vector3 vec, Pawn pawn, List<FloatMenuOption> options)
        {
            if (schema.energyTrackers.NullOrEmpty()) return;
            foreach (var energyTracker in schema.energyTrackers)
            {
                if (energyTracker.GetType() != typeof(EnergyTracker_Consumable) || energyTracker.def.consumables.NullOrEmpty()) continue;

                String text = "";
                List<EnergyConsumable> consumables = energyTracker.def.consumables;
                foreach (var consume in consumables)
                {
                    Thing ammo = vec.ToIntVec3().GetFirstThing(pawn.Map, consume.thingDef);
                    if (ammo == null)
                    {
                        continue;
                    }

                    if (!pawn.CanReach(ammo, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                    {
                        text = "ISF_Charge".Translate(schema.def.LabelCap.ToString(), energyTracker.def.LabelCap, ammo.def.label)
                            + "ISF_ChargeNoPath".Translate();
                        options.Add(new FloatMenuOption(text, null, MenuOptionPriority.Default,
                            null, null, 0f, null, null, true, 0));
                    }
                    else if (energyTracker.MaxEnergy != 0 &&
                        energyTracker.currentEnergy == energyTracker.MaxEnergy)
                    {
                        text = "ISF_Charge".Translate(schema.def.LabelCap.ToString(), energyTracker.def.LabelCap, ammo.def.label)
                            + "ISF_ChargeFull".Translate();
                        options.Add(new FloatMenuOption(text, null, MenuOptionPriority.Default,
                            null, null, 0f, null, null, true, 0));
                    }
                    else
                    {
                        int count = 0;
                        int endcount = ammo.stackCount;
                        float gain = endcount * consume.energy;
                        if (energyTracker.MaxEnergy == 0)
                        {
                            text = "ISF_Charge".Translate(schema.def.LabelCap.ToString(), energyTracker.def.LabelCap, ammo.def.label)
                            + "ISF_ChargeCalc".Translate(ammo.stackCount, ammo.def.label,
                                ammo.stackCount * consume.energy,
                                energyTracker.def.energyLabelKey.Translate());
                        }
                        else
                        {
                            count = (int)Math.Ceiling((energyTracker.MaxEnergy - energyTracker.currentEnergy) / consume.energy);
                            endcount = Math.Min(count, ammo.stackCount);
                            gain = Math.Min(endcount * consume.energy, energyTracker.MaxEnergy - energyTracker.currentEnergy);
                            text = "ISF_Charge".Translate(schema.def.LabelCap.ToString(), energyTracker.def.LabelCap, ammo.def.label)
                            + "ISF_ChargeCalc".Translate(endcount, ammo.def.label,
                                gain, energyTracker.def.energyLabelKey.Translate());
                        }

                        Action chargeSchema = delegate ()
                        {
                            pawn.jobs.TryTakeOrderedJob(JobGiver_Charge.MakeChargeEnergyJob(pawn, schema, energyTracker, ammo, endcount),
                                new JobTag?(JobTag.Misc), false);
                        };
                        options.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, chargeSchema,
                            MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), pawn, ammo, "ReservedBy", null));
                    }
                }
            }
        }

        // POSTFIX: enables the skill tree to show ability icon
        public static void DefIconAbilities(Rect __0, Def __1, float __3, Material __7)
        {
            AbilityDef abilityDef;
            if ((abilityDef = (__1 as AbilityDef)) != null)
            {
                GUI.color = Color.white;
                Widgets.DrawTextureFitted(__0, abilityDef.uiIcon, __3, __7);
                return;
            }

            return;
        }

        // HELPER METHOD
        public static void CacheComp(Pawn pawn)
        {
            if (!cachedSchemaComps.ContainsKey(pawn))
            {
                cachedSchemaComps[pawn] = SorcerySchemaUtility.GetSorceryComp(pawn);
            }
        }

        // POSTFIX: if a pawn has a tag with the OnDamage or OnDamaged worker, add exp based on amount
        public static void TakeDamage_AddEXP(Thing __instance, DamageInfo __0)
        {
            if (__0.Def.ExternalViolenceFor(__instance))
            {
                Pawn caster;
                if (__0.Instigator != null && (caster = __0.Instigator as Pawn) != null && caster.IsColonist) 
                    ApplyDamageEXP(caster, __0, typeof(ProgressEXPWorker_OnDamage)); 

                Pawn target;
                if ((target = __instance as Pawn) != null && target.IsColonist)
                    ApplyDamageEXP(target, __0, typeof(ProgressEXPWorker_OnDamaged));
            }

            return;
        }

        // HELPER METHOD
        public static void ApplyDamageEXP(Pawn pawn, DamageInfo dinfo, Type progressWorkerClass)
        {
            CacheComp(pawn);
            Comp_ItsSorcery comp = cachedSchemaComps[pawn];
            if (comp is null || comp.schemaTracker.sorcerySchemas.NullOrEmpty()) return;

            foreach (var schema in comp.schemaTracker.sorcerySchemas)
            {
                if (schema.def.progressTrackerDef.Workers.EnumerableNullOrEmpty()) continue;
                foreach (var worker in schema.def.progressTrackerDef.Workers.Where(x => x.GetType() == progressWorkerClass))
                {
                    if (!worker.def.damageDefs.NullOrEmpty() && !worker.def.damageDefs.Contains(dinfo.Def)) continue;
                    worker.TryExecute(schema.progressTracker, dinfo.Amount);
                }
            }
            return;
        }

        // HELPER METHOD
        public static void Learn_AddEXP(SkillRecord __instance, float __0)
        {
            // player won't care if it isn't their own pawn getting skill exp, and they won't really notice.
            // the patch should also not fire when the pawn is losing exp, only when gaining exp
            if (!__instance.Pawn.IsColonist || __0 < 0) return;

            CacheComp(__instance.Pawn);
            Comp_ItsSorcery comp = cachedSchemaComps[__instance.Pawn];
            if (comp is null) return;

            foreach (var schema in comp.schemaTracker.sorcerySchemas)
            {
                if (schema.def.progressTrackerDef.Workers.EnumerableNullOrEmpty()) continue;
                foreach (var worker in schema.def.progressTrackerDef.Workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_OnSkillEXP)))
                {
                    if (!worker.def.skillDefs.NullOrEmpty() && !worker.def.skillDefs.Contains(__instance.def)) continue;
                    worker.TryExecute(schema.progressTracker, __0);
                }
            }
        }


        // POSTFIX: if a pawn kills another pawn, execute the OnKill EXPWorker if their schema has the tag
        public static void DoKillSideEffects_AddEXP(DamageInfo? __0)
        {
            if (__0 == null || __0.Value.Instigator == null) return;

            Pawn killer;
            if ((killer = __0.Value.Instigator as Pawn) != null && killer.IsColonist)
            {
                CacheComp(killer);
                Comp_ItsSorcery comp = cachedSchemaComps[killer];
                if (comp is null) return;

                foreach (var schema in comp.schemaTracker.sorcerySchemas)
                {
                    if (schema.def.progressTrackerDef.Workers.EnumerableNullOrEmpty()) continue;
                    foreach (var worker in schema.def.progressTrackerDef.Workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_OnKill)))
                    {
                        if (!worker.def.damageDefs.NullOrEmpty() && !worker.def.damageDefs.Contains(__0.Value.Def)) continue;
                        worker.TryExecute(schema.progressTracker);
                    }
                }
            }
        }

        // POSTFIX: when right clicking items that can give xp to schemas, provide FloatMenu option to use them
        public static void AddHumanlikeOrders_EXPUseItem(Vector3 __0, Pawn __1, List<FloatMenuOption> __2)
        {
            Comp_ItsSorcery comp = __1.TryGetComp<Comp_ItsSorcery>() as Comp_ItsSorcery;
            String text;
            foreach (SorcerySchema schema in comp.schemaTracker.sorcerySchemas)
            {
                if (schema.progressTracker.def.Workers.EnumerableNullOrEmpty()) continue;

                ProgressEXPWorker_UseItem itemWorker = schema.progressTracker.def.Workers.FirstOrDefault(x => x.GetType() == typeof(ProgressEXPWorker_UseItem)) as ProgressEXPWorker_UseItem;
                if (itemWorker == null || itemWorker.def.expItems.NullOrEmpty()) continue;
                foreach(var item in itemWorker.def.expItems)
                {
                    Thing EXPItem = __0.ToIntVec3().GetFirstThing(__1.Map, item.thingDef);
                    if (EXPItem == null)
                    {
                        continue;
                    }

                    float factor = item.expFactorStat != null ? __1.GetStatValue(item.expFactorStat) : 1f;
                    if (!__1.CanReach(EXPItem, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                    {
                        
                        text = "ISF_UseEXPItemNoPath".Translate() + item.gainEXPTransKey.Translate(item.thingDef.label, item.exp * factor, schema.def.LabelCap.ToString());
                        __2.Add(new FloatMenuOption(text, null, MenuOptionPriority.Default,
                            null, null, 0f, null, null, true, 0));
                    }

                    else
                    {
                        text = item.gainEXPTransKey.Translate(item.thingDef.label, item.exp * factor, schema.def.LabelCap.ToString());

                        Action chargeSchema = delegate ()
                        {
                            __1.jobs.TryTakeOrderedJob(JobGiver_GainEXP.MakeChargeEXPJob(__1, schema, EXPItem, 1),
                                new JobTag?(JobTag.Misc), false);
                        };
                        __2.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, chargeSchema,
                            MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), __1, EXPItem, "ReservedBy", null));
                    }
                }
            }

            return;
        }
           
        // POSTFIX: using a specific mod extension, allow pawns to be generated with custom magic systems
        public static void GenerateNewPawnInternal_Schema(ref Pawn __result, ref PawnGenerationRequest __0)
        {
            if (__result is null) return; // no pawn generated -> don't bother trying to add a magic schema

            PawnKindSchemaUtility.SetupSchemas(ref __result, __0); // run a static method for generating magic schema on pawns
        }

        // POSTFIX: using a specific mod extension, allow pawns to gain custom magic systems through traits
        public static void GainTrait_Schema(TraitSet __instance, Trait __0)
        {
            // no modextension for schemas = no work
            if (!__0.def.HasModExtension<ModExtension_SchemaAddition>()) return;
            ModExtension_SchemaAddition schemaExt = __0.def.GetModExtension<ModExtension_SchemaAddition>();
            SorcerySchemaUtility.AddSorcerySchema(Traverse.Create(__instance).Field("pawn").GetValue<Pawn>(), schemaExt.schema);
        }

        // POSTFIX: using a specific mod extension, allow pawns to gain custom magic systems through genes
        public static void AddGene_Schema(Pawn_GeneTracker __instance, GeneDef __0)
        {
            // no modextension for schemas = no work
            if (!__0.HasModExtension<ModExtension_SchemaAddition>()) return;
            ModExtension_SchemaAddition schemaExt = __0.GetModExtension<ModExtension_SchemaAddition>();
            SorcerySchemaUtility.AddSorcerySchema(__instance.pawn, schemaExt.schema);
        }


        public static Dictionary<Pawn, Comp_ItsSorcery> cachedSchemaComps = new Dictionary<Pawn, Comp_ItsSorcery>();

    }
}
