using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public static class PawnKindSchemaUtility
    {
        public static void SetupSchemas(ref Pawn pawn, PawnGenerationRequest request)
        {
            // no modextension to the pawnkind for schemas = no work
            if (!request.KindDef.HasModExtension<ModExtension_PawnGenSchemaSet>()) return;
            ModExtension_PawnGenSchemaSet allSets = request.KindDef.GetModExtension<ModExtension_PawnGenSchemaSet>(); // else get all schema sets

            if (allSets.schemaSets.NullOrEmpty()) return; // if nothing in schema set, ignore this method

            try // attempt generating pawn
            {
                foreach (var schemaSet in allSets.schemaSets) // else, for each schema set
                {
                    // add schema at random
                    SchemaNodeMap mapping = schemaSet.GetRandSchema(); // get random schema 
                    SorcerySchemaUtility.AddSorcerySchema(pawn, mapping.schema, out SorcerySchema schema, mapping.baseClassDef); // add schema w/ specified base class

                    // complete class changes first (with its own level/point/prereqs)
                    ResolveClassChange(mapping.classChanges, ref schema); // class changes

                    // force level/point/preqs
                    ResolveForcedLevel(mapping, ref schema); // if the mapping forces a minimum level, levels the pawn up to said stage if the final class permits
                    ResolveForcedPoints(mapping, ref schema); // adds points to match the forced point requirement or the current points, whichever is larger
                    ResolvePrereqsNode(mapping, ref schema); // complete prerequisites as possible
                    ResolveSchemaEnergy(ref schema); // finally, adjust the energytracker's current levels for spawn.
                }
            }
            catch (Exception e) // any issues cause the pawn to null out -> pawn doesn't get generated
            {
                Log.Message(e);
                Log.Warning($"[It's Sorcery!] Failed to add SorcerySchema to pawnkind {request.KindDef.LabelCap}; ending PawnGen process.");
                pawn = null;
            }
        }

        public static void ResolveForcedLevel(SchemaNodeMap mapping, ref SorcerySchema schema)
        {
            if (!mapping.forceLevel) return; // if a forced level is not implemented, just skip

            // depending on the mapping's requirements, it will level up the schema and grant points depending on the system
            // level up until it reachs the right level OR the maximum level, whichever is first
            while (!schema.progressTracker.Maxed && schema.progressTracker.CurrLevel < mapping.level) schema.progressTracker.ForceLevelUp(1, true);
        }

        public static void ResolveForcedPoints(SchemaNodeMap mapping, ref SorcerySchema schema)
        {
            // if the mapping forces points at a minimum level, sets point either at that level or at the points at the current level, whichever is higher
            if (mapping.forcePoints) schema.progressTracker.points = Math.Max(schema.progressTracker.points, mapping.points); 
        }

        public static void ResolvePrereqsNodeClass(ProgressTrackerClassDef targetClassDef, ref SorcerySchema schema)
        {
            // unlock relevant nodes
            HashSet<LearningTrackerDef> learningTrackerDefs = new HashSet<LearningTrackerDef>();
            foreach (var node in targetClassDef.prereqsNodes) learningTrackerDefs.Add(node.learningTrackerDef); // slim down to only learningtrackers covered by required nodes
            foreach (var learningTrackerDef in learningTrackerDefs) // iterate through learning trackers
            {
                LearningTracker l = schema.learningTrackers.FirstOrDefault(x => x.def == learningTrackerDef); //sanity nullcheck
                if (l != null) l.locked = false; // make sure it exists; then make sure it is unlocked
            }

            foreach (var nodeDef in targetClassDef.prereqsNodes) // for each node requirement within the mapping
            {
                if (!schema.learningNodeRecord.completion.ContainsKey(nodeDef)) continue; // null check
                if (!schema.learningNodeRecord.ExclusiveNodeFufilled(nodeDef)) // if there is an exlusive node conflict
                {
                    Log.Message(schema.pawn.Name.ToStringShort + ": " + nodeDef.defName + " could not be completed due to an exclusive node in the pawnkind.");
                    continue;
                }

                // if current node isn't completed, prereqsfufilled are fufilled
                if (!schema.learningNodeRecord.completion[nodeDef] && schema.learningNodeRecord.PrereqFufilled(nodeDef))
                {
                    schema.learningNodeRecord.completion[nodeDef] = true; // handle node completion first
                    schema.progressTracker.usedPoints += nodeDef.pointReq; // increase the number of required points

                    // level and point adjustment for consistency //
                    // level up magic till used points are exceeded by total points OR pawn is at maximum level
                    while (!schema.progressTracker.Maxed && schema.progressTracker.points < schema.progressTracker.usedPoints)
                        schema.progressTracker.ForceLevelUp(1, true); // force level up one at a time, don't show msgs

                    // if the system happens to be maxed out, instead try to make up the point difference directly
                    if (schema.progressTracker.Maxed)
                        schema.progressTracker.points += Math.Max(0, schema.progressTracker.usedPoints - schema.progressTracker.points);

                    // other prereq resolution //
                    // stats changes have many sources - thus, it is easier to assume that the pawn had met a requirement previously
                    // hediffs and skill level are more difficult to wave away
                    ResolveForceHediffClass(targetClassDef, ref schema); // if set, forces pawn to have hediffs before completing the node
                    ResolveForceSkillClass(targetClassDef, ref schema); // if set, forces pawn to have the proper skill level before completing the node
                    ResolveForceLevelClass(targetClassDef, ref schema); // if set, forces pawn to be leveled up to a certain level

                    // complete and record node completion results //
                    // prep record in diff log
                    ProgressDiffLog diffLog = schema.progressTracker.progressDiffLog;
                    ProgressDiffLedger progressDiffLedger = diffLog.PrepNewLedger(schema.progressTracker);
                    ProgressDiffClassLedger progressDiffClassLedger = new ProgressDiffClassLedger();

                    // complete node and record results
                    schema.learningNodeRecord.CompletionAbilities(nodeDef, ref progressDiffClassLedger); // adjust abilities
                    schema.learningNodeRecord.CompletionHediffs(nodeDef, ref progressDiffClassLedger); // adjust hediffs
                    schema.learningNodeRecord.CompletionModifiers(nodeDef, ref progressDiffClassLedger); // adjust stat modifiers

                    // add record to diff log
                    progressDiffLedger.classDiffLedgers[ISF_DefOf.ISF_Generic_Class] = progressDiffClassLedger;
                    diffLog.AddLedger(progressDiffLedger);
                }
                else
                {
                    // debug info if nodes are completed or cannot be done due to missing prereqs
                    if (!Prefs.DevMode || !ItsSorceryUtility.settings.ShowItsSorceryDebug) continue;
                    if (schema.learningNodeRecord.completion[nodeDef]) Log.Message(schema.pawn.Name.ToStringShort + ": " + nodeDef.defName + " is already complete.");
                    else Log.Message(schema.pawn.Name.ToStringShort + ": " + nodeDef.defName + " could not be completed due to missing prerequisites.");
                }
            }
        }

        public static void ResolvePrereqsNode(SchemaNodeMap mapping, ref SorcerySchema schema)
        {
            // unlock relevant nodes
            HashSet<LearningTrackerDef> learningTrackerDefs = new HashSet<LearningTrackerDef>();
            foreach (var node in mapping.requiredNodes) learningTrackerDefs.Add(node.nodeDef.learningTrackerDef); // slim down to only learningtrackers covered by required nodes
            foreach (var learningTrackerDef in learningTrackerDefs) // iterate through learning trackers
            {
                LearningTracker l = schema.learningTrackers.FirstOrDefault(x => x.def == learningTrackerDef); //sanity nullcheck
                if(l != null) l.locked = false; // make sure it exists; then make sure it is unlocked
            }
            
            foreach (var nodeReq in mapping.requiredNodes) // for each node requirement within the mapping
            {
                if (!schema.learningNodeRecord.completion.ContainsKey(nodeReq.nodeDef)) continue; // null check
                if (!schema.learningNodeRecord.ExclusiveNodeFufilled(nodeReq.nodeDef)) // if there is an exlusive node conflict
                {
                    Log.Message(schema.pawn.Name.ToStringShort +": " + nodeReq.nodeDef.defName + " could not be completed due to an exclusive node in the pawnkind.");
                    continue;
                }

                // if current node isn't completed, prereqsfufilled are fufilled
                if (!schema.learningNodeRecord.completion[nodeReq.nodeDef] && schema.learningNodeRecord.PrereqFufilled(nodeReq.nodeDef))
                {
                    schema.learningNodeRecord.completion[nodeReq.nodeDef] = true; // handle node completion first
                    schema.progressTracker.usedPoints += nodeReq.nodeDef.pointReq; // increase the number of required points

                    // level and point adjustment for consistency //
                    // level up magic till used points are exceeded by total points OR pawn is at maximum level
                    while (!schema.progressTracker.Maxed && schema.progressTracker.points < schema.progressTracker.usedPoints)
                        schema.progressTracker.ForceLevelUp(1, true); // force level up one at a time, don't show msgs

                    // if the system happens to be maxed out, instead try to make up the point difference directly
                    if (schema.progressTracker.Maxed) 
                        schema.progressTracker.points += Math.Max(0, schema.progressTracker.usedPoints - schema.progressTracker.points);

                    // other prereq resolution //
                    // stats changes have many sources - thus, it is easier to assume that the pawn had met a requirement previously
                    // hediffs and skill level are more difficult to wave away
                    ResolveForceHediffNode(nodeReq, ref schema); // if set, forces pawn to have hediffs before completing the node
                    ResolveForceSkillNode(nodeReq, ref schema); // if set, forces pawn to have the proper skill level before completing the node
                    ResolveForceLevelNode(nodeReq, ref schema); // if set, forces pawn to be leveled up to a certain level

                    // complete and record node completion results //
                    // prep record in diff log
                    ProgressDiffLog diffLog = schema.progressTracker.progressDiffLog;
                    ProgressDiffLedger progressDiffLedger = diffLog.PrepNewLedger(schema.progressTracker);
                    ProgressDiffClassLedger progressDiffClassLedger = new ProgressDiffClassLedger();

                    // complete node and record results
                    schema.learningNodeRecord.CompletionAbilities(nodeReq.nodeDef, ref progressDiffClassLedger); // adjust abilities
                    schema.learningNodeRecord.CompletionHediffs(nodeReq.nodeDef, ref progressDiffClassLedger); // adjust hediffs
                    schema.learningNodeRecord.CompletionModifiers(nodeReq.nodeDef, ref progressDiffClassLedger); // adjust stat modifiers

                    // add record to diff log
                    progressDiffLedger.classDiffLedgers[ISF_DefOf.ISF_Generic_Class] = progressDiffClassLedger;
                    diffLog.AddLedger(progressDiffLedger);
                }
                else
                {
                    // debug info if nodes are completed or cannot be done due to missing prereqs
                    if (!Prefs.DevMode || !ItsSorceryUtility.settings.ShowItsSorceryDebug) continue;
                    if (schema.learningNodeRecord.completion[nodeReq.nodeDef]) Log.Message(schema.pawn.Name.ToStringShort + ": " + nodeReq.nodeDef.defName + " is already complete.");
                    else Log.Message(schema.pawn.Name.ToStringShort + ": " + nodeReq.nodeDef.defName + " could not be completed due to missing prerequisites.");
                }
            }
        }

        public static void ResolveForceHediffNode(SchemaNodeReq nodeReq, ref SorcerySchema schema)
        {
            if (!nodeReq.forceHediff) return; // if the node req doesn't force hediff requirements, skip
            ResolveForceHediff(nodeReq.nodeDef.prereqsHediff, ref schema);
        }

        public static void ResolveForceHediffClass(ProgressTrackerClassDef targetClassDef, ref SorcerySchema schema)
        {
            ResolveForceHediff(targetClassDef.prereqsHediff, ref schema);
        }

        public static void ResolveForceHediff(Dictionary<HediffDef, float> prereqsHediff, ref SorcerySchema schema)
        {
            foreach (var hediffReq in prereqsHediff) // otherwise, check and add if needed
            {
                if (schema.pawn.health.hediffSet.GetFirstHediffOfDef(hediffReq.Key) is Hediff hediff && hediff != null)
                {
                    hediff.Severity = hediffReq.Value;
                }
                else HealthUtility.AdjustSeverity(schema.pawn, hediffReq.Key, hediffReq.Value);
            }
        }

        public static void ResolveForceSkillClass(ProgressTrackerClassDef targetClassDef, ref SorcerySchema schema)
        {
            ResolveForceSkill(targetClassDef.prereqsSkills, ref schema);
        }

        public static void ResolveForceSkillNode(SchemaNodeReq nodeReq, ref SorcerySchema schema)
        {
            if (!nodeReq.forceSkill) return; // if the node req doesn't force skill requirements, skip
            ResolveForceSkill(nodeReq.nodeDef.prereqsSkills, ref schema);
        }

        public static void ResolveForceSkill(List<NodeSkillReqs> skillReqsList, ref SorcerySchema schema)
        {
            SkillRecord currSkill;
            foreach (var skillReq in skillReqsList) // otherwise, adjust skill levels as needed
            {
                foreach (var skillLevel in skillReq.skillReqs)
                {
                    currSkill = schema.pawn.skills.GetSkill(skillLevel.skillDef);
                    if (currSkill.TotallyDisabled) continue;
                    switch (skillReq.mode)
                    {
                        case LearningNodeStatPrereqMode.Equal: // ensure equal skill level
                            if (currSkill.GetLevel() != skillLevel.ClampedLevel) currSkill.Level = skillLevel.ClampedLevel;
                            break;

                        case LearningNodeStatPrereqMode.NotEqual:
                            if (currSkill.GetLevel() == skillLevel.ClampedLevel) currSkill.Level = Mathf.Clamp(skillLevel.ClampedLevel - 1, 0, 20);
                            break;

                        case LearningNodeStatPrereqMode.Greater:
                            if (currSkill.GetLevel() <= skillLevel.ClampedLevel) currSkill.Level = Mathf.Clamp(skillLevel.ClampedLevel + 1, 0, 20);
                            break;

                        case LearningNodeStatPrereqMode.GreaterEqual:
                            if (currSkill.GetLevel() < skillLevel.ClampedLevel) currSkill.Level = skillLevel.ClampedLevel;
                            break;

                        case LearningNodeStatPrereqMode.Lesser:
                            if (currSkill.GetLevel() >= skillLevel.ClampedLevel) currSkill.Level = Mathf.Clamp(skillLevel.ClampedLevel - 1, 0, 20);
                            break;

                        case LearningNodeStatPrereqMode.LesserEqual:
                            if (currSkill.GetLevel() > skillLevel.ClampedLevel) currSkill.Level = skillLevel.ClampedLevel;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public static void ResolveSchemaEnergy(ref SorcerySchema schema)
        {
            foreach(var e in schema.energyTrackers)
            {
                if (e.def.inverse) e.currentEnergy = e.MinEnergy; // for inverse energy systems; energy is set to the normal minimum value
                else e.currentEnergy = e.MaxEnergy; // for normal energy systems; energy is set to the normal maximum value
            }
        }

        public static void ResolveForceLevelNode(SchemaNodeReq nodeReq, ref SorcerySchema schema)
        {
            if (!nodeReq.forceLevel) return; // if the node req doesn't force level requirements, skip
            ResolveForceLevel(nodeReq.nodeDef.prereqLevel, ref schema);
        }

        public static void ResolveForceLevelClass(ProgressTrackerClassDef targetClassDef, ref SorcerySchema schema)
        {
            if (schema.progressTracker.currClassDef.levelRange.TrueMax < targetClassDef.prereqLevel) return; // if current class cannot reach target class prereq level, just skip it
            ResolveForceLevel(targetClassDef.prereqLevel, ref schema);
        }

        public static void ResolveForceLevel(int prereqLevel, ref SorcerySchema schema)
        {
            while (!schema.progressTracker.Maxed && prereqLevel > schema.progressTracker.CurrLevel) schema.progressTracker.ForceLevelUp(1, true);
        }       

        public static void ResolveClassChange(List<ProgressLinkedClassMap> classMappings, ref SorcerySchema schema)
        {
            if (classMappings.NullOrEmpty()) return; // nullcheck list => skip running code
            
            ProgressTracker progressTracker = schema.progressTracker;
            ProgressDiffLog diffLog = progressTracker.progressDiffLog;
            // iterate through the class change mappings
            foreach(var classMapping in classMappings)
            {
                // validate that the current class is able to have the class mapping - if not, throw error and fail the pawngeneration
                if (!progressTracker.currClassDef.linkedClasses.Contains(classMapping))
                {
                    Log.Error($"[It's Sorcery!] PawnGen class mapping error: unable to change from class {progressTracker.currClassDef.label} to {classMapping.classDef.label}.");
                    throw new Exception($"[It's Sorcery!] SchemaSet misconfiguration.");
                }

                // level up till the class opportunity arises
                while (!progressTracker.classChangeOpps.Contains(classMapping) && !progressTracker.Maxed) progressTracker.ForceLevelUp(1, true);

                ProgressTrackerClassDef targetClassDef = classMapping.classDef;

                // other prereq resolution //
                // stats changes have many sources - thus, it is easier to assume that the pawn had met a requirement previously
                // hediffs and skill level are more difficult to wave away
                ResolvePrereqsNodeClass(targetClassDef, ref schema); // if set, forces pawn to have learning tree nodes
                ResolveForceHediffClass(targetClassDef, ref schema); // if set, forces pawn to have hediffs done
                ResolveForceSkillClass(targetClassDef, ref schema); // if set, forces pawn to have the proper skill level
                ResolveForceLevelClass(targetClassDef, ref schema); // if set, forces pawn to be leveled up to a certain level

                diffLog.AdjustClass(progressTracker, targetClassDef, classMapping.levelReset, classMapping.benefitReset);
                ProgressTrackerUtility.CompletionLearningUnlock(ref progressTracker, targetClassDef);
                progressTracker.ResetLevelLabel();
                foreach (var et in progressTracker.schema.energyTrackers) et.ForceClearEnergyStatCaches(); //.ClearStatCache();
                progressTracker.CleanClassChangeOpps();
            }
        }

        

    }
}
