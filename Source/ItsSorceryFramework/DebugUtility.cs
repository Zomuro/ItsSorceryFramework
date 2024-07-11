using LudeonTK;
using RimWorld;
using System.Collections.Generic;
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
        [DebugAction("It's Sorcery!", "Add SorcerySchema", false, false, false, false, 0, false, 
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
        [DebugAction("It's Sorcery!", "Remove SorcerySchema", false, false, false, false, 0, false,
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
        [DebugAction("It's Sorcery!", "Remove all SorcerySchema", false, false, false, false, 0, false,
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
        [DebugAction("It's Sorcery!", "Refresh all ProgressTrackers", false, false, false, false, 0, false,
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

        [DebugAction("It's Sorcery!", "Level Up ProgressTracker", false, false, false, false, 0, false,
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
                    schema.progressTracker.ForceLevelUp();
                    Log.Message($"[It's Sorcery!] Prior level: {priorLevel}; Current level: {schema.progressTracker.CurrLevel}; " +
                        $"Level range: {schema.progressTracker.def.levelRange.TrueMin}-{schema.progressTracker.def.levelRange.TrueMax}");
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }

        // Used to test new recovery method for hediffs
        /*[DebugAction("It's Sorcery!", "Force ProgressTracker Reset", false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void ForceProgressTrackerReset(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"{pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();

            foreach (SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                options.Add(new DebugMenuOption(schema.def.label, DebugMenuOptionMode.Tool, delegate ()
                {
                    //pawn.health.RemoveHediff(schema.progressTracker.ProgressHediff);
                    schema.progressTracker.Hediff = null;
                    Log.Message($"Hediff Removed: {schema.progressTracker.Hediff.def.label}");
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, null));
        }*/

    }
}
