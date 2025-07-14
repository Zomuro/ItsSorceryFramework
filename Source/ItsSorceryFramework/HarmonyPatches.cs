using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            // Setup stopwatch to time harmony patches
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Harmony harmony = new Harmony("Zomuro.ItsSorceryFramework");

            // Ability Patches //

            // DefIconAbilities
            // allows DefIcon to show abilitydef icons
            harmony.Patch(AccessTools.Method(typeof(Widgets), "DefIcon"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DefIconAbilities)));

            // EnergyTracker and ProgressEXPWorker Patches

            // TakeDamage_ISF_Postfix
            // for every magic system with the correct EXP tag, give xp depending on damage
            harmony.Patch(AccessTools.Method(typeof(Thing), "TakeDamage"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(TakeDamage_ISF_Postfix)));

            // SkillLearn_AddEXP
            // for every magic system with the correct EXP tag, give xp depending on skill being learned
            harmony.Patch(AccessTools.Method(typeof(SkillRecord), "Learn"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(Learn_AddEXP)));

            // DoKillSideEffects_ISF_Postfix
            // for every magic system with the correct EXP tag, give xp on kill
            harmony.Patch(AccessTools.Method(typeof(Pawn), "DoKillSideEffects"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DoKillSideEffects_ISF_Postfix)));

            // TryInteractWith_ISF_Postfix
            // after a successful (true) TryInteractWith, look through magic systems to add XP/energy 
            harmony.Patch(AccessTools.Method(typeof(Pawn_InteractionsTracker), "TryInteractWith"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(TryInteractWith_ISF_Postfix)));

            // EndQuest_ISF_Postfix
            // on quest end, check workers of all colonists and see if we need to add XP
            harmony.Patch(AccessTools.Method(typeof(Quest), "End"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(EndQuest_ISF_Postfix)));

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

            // end stopwatch and print harmony patching results
            stopwatch.Stop();
            Log.Message(
                string.Format("[It's Sorcery!] Successfully completed {0} Harmony patches in {1} secs.",
                    harmony.GetPatchedMethods().Select(new Func<MethodBase, Patches>(Harmony.GetPatchInfo)).SelectMany(
                        (Patches p) => p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers)).Count((Patch p) => p.owner == harmony.Id),
                    stopwatch.Elapsed.TotalSeconds
                )
            );
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
        public static void TakeDamage_ISF_Postfix(Thing __instance, DamageInfo __0)
        {
            if (__0.Def.ExternalViolenceFor(__instance))
            {
                // progress tracker portion
                Pawn caster;
                if (__0.Instigator != null && (caster = __0.Instigator as Pawn) != null && caster.IsColonist) 
                    ApplyDamageEXP(caster, __0, typeof(ProgressEXPWorker_OnDamage)); 

                Pawn target;
                if ((target = __instance as Pawn) != null && target.IsColonist)
                    ApplyDamageEXP(target, __0, typeof(ProgressEXPWorker_OnDamaged));

                // energy tracker portion
                ApplyDamageEnergy(__instance, __0);
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
                // schema.def.progressTrackerDef.Workers
                HashSet<ProgressEXPWorker> workers = schema.progressTracker.currClassDef.Workers;
                if (workers.EnumerableNullOrEmpty()) continue;
                foreach (var worker in workers.Where(x => x.GetType() == progressWorkerClass))
                {
                    if (!worker.def.damageDefs.NullOrEmpty() && !worker.def.damageDefs.Contains(dinfo.Def)) continue;
                    worker.TryExecute(schema.progressTracker, dinfo.Amount);
                }
            }
            return;
        }

        // HELPER METHOD
        public static void ApplyDamageEnergy(Thing targetThing, DamageInfo dinfo)
        {
            Pawn targetPawn = targetThing as Pawn;
            if (targetPawn != null)
            {
                CacheComp(targetPawn);
                Comp_ItsSorcery targetComp = cachedSchemaComps[targetPawn];
                if (targetComp != null && !targetComp.schemaTracker.sorcerySchemas.NullOrEmpty())
                {
                    foreach (var schema in targetComp.schemaTracker.sorcerySchemas) // for every sorceryschema
                    {
                        if (schema.energyTrackers.NullOrEmpty()) continue; //nullcheck energytrackers
                        foreach (var energyTracker in schema.energyTrackers)  // look in a schema's energytrackers
                        {
                            if (energyTracker.comps.NullOrEmpty()) continue; // nullcheck energycomps
                            foreach (var energyComp in energyTracker.comps) energyComp.CompPostDamageRecieved(dinfo); // call relevant damage-related comp methods
                        }
                    }
                }
            }

            Pawn instigatorPawn = dinfo.Instigator is null ? null : dinfo.Instigator as Pawn;
            if (instigatorPawn != null)
            {
                CacheComp(instigatorPawn);
                Comp_ItsSorcery instigatorComp = cachedSchemaComps[instigatorPawn];
                if (instigatorComp != null && !instigatorComp.schemaTracker.sorcerySchemas.NullOrEmpty())
                {
                    foreach (var schema in instigatorComp.schemaTracker.sorcerySchemas) // for every sorceryschema
                    {
                        if (schema.energyTrackers.NullOrEmpty()) continue; //nullcheck energytrackers
                        foreach (var energyTracker in schema.energyTrackers)  // look in a schema's energytrackers
                        {
                            if (energyTracker.comps.NullOrEmpty()) continue; // nullcheck energycomps
                            // call relevant damage-related comp methods
                            foreach (var energyComp in energyTracker.comps) energyComp.CompPostDamageDealt(dinfo);
                        }
                    }
                }
            }
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
                // schema.def.progressTrackerDef.Workers
                HashSet<ProgressEXPWorker> workers = schema.progressTracker.currClassDef.Workers;
                if (workers.EnumerableNullOrEmpty()) continue;
                foreach (var worker in workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_OnSkillEXP)))
                {
                    if (!worker.def.skillDefs.NullOrEmpty() && !worker.def.skillDefs.Contains(__instance.def)) continue;
                    worker.TryExecute(schema.progressTracker, __0);
                }
            }
        }


        // POSTFIX: if a pawn kills another pawn, execute the OnKill EXPWorker and energycomps if their schema has the tag/comp
        public static void DoKillSideEffects_ISF_Postfix(DamageInfo? __0)
        {
            if (__0 == null || __0.Value.Instigator == null) return;
            if (__0.Value.Instigator is Pawn killer && killer.IsColonist)
            {
                CacheComp(killer);
                Comp_ItsSorcery comp = cachedSchemaComps[killer];
                if (comp is null) return;

                foreach (var schema in comp.schemaTracker.sorcerySchemas)
                {
                    // schema.def.progressTrackerDef.Workers
                    HashSet<ProgressEXPWorker> workers = schema.progressTracker.currClassDef.Workers;
                    if (workers.EnumerableNullOrEmpty()) continue;
                    foreach (var worker in workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_OnKill)))
                    {
                        if (!worker.def.damageDefs.NullOrEmpty() && !worker.def.damageDefs.Contains(__0.Value.Def)) continue;
                        worker.TryExecute(schema.progressTracker);
                    }

                    if (schema.energyTrackers.NullOrEmpty()) continue;
                    foreach (var energyTracker in schema.energyTrackers)
                    {
                        if (energyTracker.comps.NullOrEmpty()) continue;
                        foreach (var energyComp in energyTracker.comps) energyComp.CompPostKill(__0);
                    }
                }
            }
        }

        // POSTFIX: if a pawn successfully interacts w/ another pawn, check interactiondef and see if we need to add XP or energy
        public static void TryInteractWith_ISF_Postfix(ref bool __result, Pawn_InteractionsTracker __instance, Pawn __0, InteractionDef __1)
        {
            // if we find that interactiondef is null OR the original TryInteractWith result is false, skip rest
            if (__1 == null || __result == false) return;

            // check if we need to add XP or energy for pawns in the interaction
            Helper_TryInteractWith_ISF_Postfix(Traverse.Create(__instance).Field("pawn").GetValue<Pawn>(), __1); // interaction initiator
            Helper_TryInteractWith_ISF_Postfix(__0, __1); // interaction recipient
        }

        public static void Helper_TryInteractWith_ISF_Postfix(Pawn pawn, InteractionDef intDef)
        {
            if (!pawn.IsColonist) return;
            CacheComp(pawn);
            Comp_ItsSorcery comp = cachedSchemaComps[pawn];
            if (comp is null) return;

            foreach (var schema in comp.schemaTracker.sorcerySchemas)
            {
                // progressEXPworker component
                HashSet<ProgressEXPWorker> workers = schema.progressTracker.currClassDef.Workers;
                if (!workers.EnumerableNullOrEmpty())
                {
                    foreach (var worker in workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_OnInteraction)))
                    {
                        if (!worker.def.interactionDefs.NullOrEmpty() && !worker.def.interactionDefs.Contains(intDef)) continue;
                        worker.TryExecute(schema.progressTracker);
                    }
                }
                
                // energytracker component
                if (!schema.energyTrackers.NullOrEmpty())
                {
                    foreach (var energyTracker in schema.energyTrackers)
                    {
                        if (energyTracker.comps.NullOrEmpty()) continue;
                        foreach (var energyComp in energyTracker.comps) energyComp.CompPostInteraction(intDef);
                    }
                }
                
            }
        }

        // POSTFIX: on quest end, check workers of all colonists and see if we need to add XP
        public static void EndQuest_ISF_Postfix(Quest __instance, QuestEndOutcome __0)
        {
            // get all pawns that are player's faction
            List<Pawn> pawnList = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists;

            // get ending quest's def to find in defs
            QuestScriptDef endingQuestDef = __instance.root;

            // iterate over all colonists and level them up
            foreach (var pawn in pawnList)
            {
                CacheComp(pawn);
                Comp_ItsSorcery comp = cachedSchemaComps[pawn];
                if (comp is null) continue;

                foreach (var schema in comp.schemaTracker.sorcerySchemas)
                {
                    // progressEXPworker component
                    HashSet<ProgressEXPWorker> workers = schema.progressTracker.currClassDef.Workers;
                    if (workers.EnumerableNullOrEmpty()) continue;
                    foreach (var worker in workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_OnQuestFinish)))
                    {
                        if (__0 != worker.def.questOutcome) continue;
                        if (!worker.def.questDefs.NullOrEmpty() && !worker.def.questDefs.Contains(endingQuestDef)) continue;
                        worker.TryExecute(schema.progressTracker);
                    }
                }
            }
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

            SorcerySchema added_schema;
            SorcerySchemaUtility.AddSorcerySchema(Traverse.Create(__instance).Field("pawn").GetValue<Pawn>(), schemaExt.schema, out added_schema, schemaExt.baseClassDef);
            PawnKindSchemaUtility.ResolveSchemaEnergy(ref added_schema);
        }

        // POSTFIX: using a specific mod extension, allow pawns to gain custom magic systems through genes
        public static void AddGene_Schema(Pawn_GeneTracker __instance, GeneDef __0)
        {
            // no modextension for schemas = no work
            if (!__0.HasModExtension<ModExtension_SchemaAddition>()) return;
            ModExtension_SchemaAddition schemaExt = __0.GetModExtension<ModExtension_SchemaAddition>();

            SorcerySchema added_schema;
            SorcerySchemaUtility.AddSorcerySchema(__instance.pawn, schemaExt.schema, out added_schema, schemaExt.baseClassDef);
            PawnKindSchemaUtility.ResolveSchemaEnergy(ref added_schema);
        }

        public static Dictionary<Pawn, Comp_ItsSorcery> cachedSchemaComps = new Dictionary<Pawn, Comp_ItsSorcery>();

    }
}
