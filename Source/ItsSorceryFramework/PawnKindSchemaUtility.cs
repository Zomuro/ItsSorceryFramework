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

            foreach (var schemaSet in allSets.schemaSets) // else, for each schema set
            {
                SchemaNodeMap mapping = schemaSet.GetRandSchema(); // get random schema 
                SorcerySchemaUtility.AddSorcerySchema(pawn, mapping.schema, out SorcerySchema schema); // add it
                ResolveForcedLevel(mapping, ref schema); // if the mapping forces a minimum level, levels the pawn up to said stage
                ResolveForcedPoints(mapping, ref schema); // adds points to match the forced point requirement or the current points, whichever is larger
                ResolvePrereqs(mapping, ref schema); // complete prerequisites as possible
                ResolveSchemaEnergy(ref schema); // finally, adjust the energytracker's current levels for spawn.
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

        public static void ResolvePrereqs(SchemaNodeMap mapping, ref SorcerySchema schema)
        {
            // unlock relevant nodes
            HashSet<LearningTrackerDef> learningTrackerDefs = new HashSet<LearningTrackerDef>();
            foreach (var node in mapping.requiredNodes) learningTrackerDefs.Add(node.nodeDef.learningTrackerDef); // slim down to only learningtrackers covered by required nodes
            foreach (var learningTrackerDef in learningTrackerDefs) // iterate through learning trackers
            {
                LearningTracker l = schema.learningTrackers.FirstOrDefault(x => x.def == learningTrackerDef); //sanity nullcheck
                if(l != null) l.locked = false; // make sure they are not null; then make sure it is unlocked
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
                    ResolveForceHediff(nodeReq, ref schema); // if set, forces pawn to have hediffs before completing the node
                    ResolveForceSkill(nodeReq, ref schema); // if set, forces pawn to have the proper skill level before completing the node
                    ResolveForceLevelNode(nodeReq, ref schema); // if set, forces pawn to be leveled up to a certain level

                    schema.learningNodeRecord.CompletionAbilities(nodeReq.nodeDef); // adjust abilities
                    schema.learningNodeRecord.CompletionHediffs(nodeReq.nodeDef); // adjust hediffs
                    schema.learningNodeRecord.CompletionModifiers(nodeReq.nodeDef); // adjust stat modifiers
                }
                else
                {
                    if (!Prefs.DevMode || !ItsSorceryUtility.settings.ShowItsSorceryDebug) continue;
                    if (schema.learningNodeRecord.completion[nodeReq.nodeDef]) Log.Message(schema.pawn.Name.ToStringShort + ": " + nodeReq.nodeDef.defName + " is already complete.");
                    else Log.Message(schema.pawn.Name.ToStringShort + ": " + nodeReq.nodeDef.defName + " could not be completed due to missing prerequisites.");
                }
            }
        }

        public static void ResolveForceHediff(SchemaNodeReq nodeReq, ref SorcerySchema schema)
        {
            if (!nodeReq.forceHediff) return; // if the node req doesn't force hediff requirements, skip

            foreach (var hediffReq in nodeReq.nodeDef.prereqsHediff) // otherwise, check and add if needed
            {
                if (schema.pawn.health.hediffSet.GetFirstHediffOfDef(hediffReq.Key) is Hediff hediff && hediff != null)
                {
                    hediff.Severity = hediffReq.Value;
                }
                else HealthUtility.AdjustSeverity(schema.pawn, hediffReq.Key, hediffReq.Value);
            }
        }

        public static void ResolveForceSkill(SchemaNodeReq nodeReq, ref SorcerySchema schema)
        {
            if (!nodeReq.forceSkill) return; // if the node req doesn't force skill requirements, skip

            SkillRecord currSkill;
            foreach (var skillReq in nodeReq.nodeDef.prereqsSkills) // otherwise, adjust skill levels as needed
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
                            if (currSkill.GetLevel() == skillLevel.ClampedLevel) currSkill.Level = Mathf.Clamp(skillLevel.ClampedLevel - 1, 0 , 20);
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

        public static void ResolveForceLevelNode(SchemaNodeReq nodeReq, ref SorcerySchema schema)
        {
            if (!nodeReq.forceLevel) return; // if the node req doesn't force level requirements, skip
            while (!schema.progressTracker.Maxed && nodeReq.nodeDef.prereqLevel > schema.progressTracker.CurrLevel) schema.progressTracker.ForceLevelUp(1, true);
        }

        public static void ResolveSchemaEnergy(ref SorcerySchema schema)
        {
            foreach(var e in schema.energyTrackers)
            {
                if (e.def.inverse) e.currentEnergy = e.MinEnergy; // for inverse energy systems; energy is set to the normal minimum value
                else e.currentEnergy = e.MaxEnergy; // for normal energy systems; energy is set to the normal maximum value
            }
        }

    }
}
