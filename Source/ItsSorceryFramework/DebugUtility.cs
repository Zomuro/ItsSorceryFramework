using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ItsSorceryFramework
{
    public static class DebugUtility
    {
		static DebugUtility()
		{
			DebugActionCategories.categoryOrders.Add("It's Sorcery!", 1600);
		}

        // Adds choice of schema to target pawn
        [DebugAction("It's Sorcery!", "Add SorcerySchema", false, false, false, false, false, 0, false, 
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void AddSorcerySchema(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();

            foreach (SorcerySchemaDef schemaDef in SorcerySchemaUtility.AllSorcerySchemaDefs)
            {
                options.Add(new DebugMenuOption(schemaDef.label, DebugMenuOptionMode.Tool, delegate ()
                {
                    SorcerySchemaUtility.AddSorcerySchema(pawn, schemaDef);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }

        // Removes a schema from target pawn
        [DebugAction("It's Sorcery!", "Remove SorcerySchema", false, false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void RemoveSorcerySchema(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if(comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }
            
            List<DebugMenuOption> options = new List<DebugMenuOption>();


            foreach (SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                options.Add(new DebugMenuOption(schema.def.label, DebugMenuOptionMode.Tool, delegate ()
                {
                    SorcerySchemaUtility.RemoveSorcerySchema(pawn, schema);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }

        // Removes all schemas from target pawn
        [DebugAction("It's Sorcery!", "Remove all SorcerySchema", false, false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void RemoveAllSorcerySchema(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            // clear out all sorcery schemas
            List<SorcerySchema> schemas = comp?.schemaTracker?.sorcerySchemas;
            foreach (SorcerySchema schema in schemas) pawn.health.RemoveHediff(schema.progressTracker.Hediff);
            schemas.Clear();
        }

        // Completely refresh ProgressTrackers
        [DebugAction("It's Sorcery!", "Refresh all ProgressTrackers", false, false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void RefreshProgressTrackers(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            foreach(SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                SorcerySchemaUtility.RefreshProgressTracker(schema);
            }
        }

        [DebugAction("It's Sorcery!", "Level Up ProgressTracker", false, false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void LevelUpProgressTracker(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();

            foreach (SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                options.Add(new DebugMenuOption(schema.def.label, DebugMenuOptionMode.Tool, delegate ()
                {
                    int priorLevel = schema.progressTracker.CurrLevel;
                    schema.progressTracker.ForceLevelUp(1, true);
                    Log.Message($"[It's Sorcery!] Prior level: {priorLevel}; Current level: {schema.progressTracker.CurrLevel}; " +
                        $"Level range: {schema.progressTracker.currClassDef.levelRange.TrueMin}-{schema.progressTracker.currClassDef.levelRange.TrueMax}");
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }

        [DebugAction("It's Sorcery!", "Max Level ProgressTracker", false, false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void MaxLevelProgressTracker(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();

            foreach (SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                options.Add(new DebugMenuOption(schema.def.label, DebugMenuOptionMode.Tool, delegate ()
                {
                    int priorLevel = schema.progressTracker.CurrLevel;
                    schema.progressTracker.ForceLevelUp(Math.Max(schema.progressTracker.currClassDef.levelRange.TrueMax - priorLevel, 1), true);
                    Log.Message($"[It's Sorcery!] Prior level: {priorLevel}; Current level: {schema.progressTracker.CurrLevel}; " +
                        $"Level range: {schema.progressTracker.currClassDef.levelRange.TrueMin}-{schema.progressTracker.currClassDef.levelRange.TrueMax}");
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }

        [DebugAction("It's Sorcery!", "Get ProgressDiffLog Ledgers", false, false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void PrintProgressDiffLogLedgers(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();

            foreach (SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                options.Add(new DebugMenuOption(schema.def.label, DebugMenuOptionMode.Tool, delegate ()
                {
                    string returnStr = "";
                    foreach(var ledger in schema.progressTracker.progressDiffLog.progressDiffLedgers)
                    {
                        returnStr += $"\nIndex {ledger.index}:";
                        foreach (var c in ledger.classDiffLedgers)
                        {
                            returnStr += $"\nClass ({c.Key})\nLevel ({ledger.level})\n{c.Value}\n";
                        }
                    }

                    Log.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} {schema.def.label} Diff Log Ledgers:{returnStr}");
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }

        [DebugAction("It's Sorcery!", "Get Overall ProgressDiffLog TotalDiff", false, false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void PrintProgressDiffLogTotalDiff(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();

            foreach (SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                options.Add(new DebugMenuOption(schema.def.label, DebugMenuOptionMode.Tool, delegate ()
                {
                    string returnStr = schema.progressTracker.progressDiffLog.TotalDiff(null).ToString();
                    Log.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} {schema.def.label} Total Diff:\n{returnStr}");
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }

        [DebugAction("It's Sorcery!", "Get Class ProgressDiffLog TotalDiff", false, false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void PrintProgressClassDiffLogTotalDiff(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();         
            foreach (SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                options.Add(new DebugMenuOption(schema.def.label, DebugMenuOptionMode.Tool, delegate ()
                {
                    // get class defs
                    HashSet<ProgressTrackerClassDef> classDefsSet = DefDatabase<ProgressTrackerClassDef>.AllDefs.Where(x => x.progressTrackerDef == schema.progressTracker.def).ToHashSet();
                    classDefsSet.Add(schema.progressTracker.def.baseClass);

                    // select which class to see the total diff of
                    List<DebugMenuOption> classOptions = new List<DebugMenuOption>();
                    foreach (ProgressTrackerClassDef classDef in classDefsSet)
                    {
                        classOptions.Add(new DebugMenuOption(classDef.label, DebugMenuOptionMode.Tool, delegate ()
                        {
                            string returnStr = schema.progressTracker.progressDiffLog.TotalDiff(classDef).ToString();
                            Log.Message($"[It's Sorcery!] {pawn.Name.ToStringShort} {schema.def.label} {classDef.label} Total Diff:\n{returnStr}");
                        }));
                    }
                    Find.WindowStack.Add(new Dialog_DebugOptionListLister(classOptions, null));

                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }

    }
}
