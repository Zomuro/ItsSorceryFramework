using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using LudeonTK;
using UnityEngine;

namespace ItsSorceryFramework
{
    public static class DebugUtility
    {
		static DebugUtility()
		{
			DebugActionCategories.categoryOrders.Add("It's Sorcery!", 1600);
		}

		[DebugAction("It's Sorcery!", "Add SorcerySchema", false, false, false, false, 0, false, 
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        public static void AddSorcerySchema(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"{pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
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

        [DebugAction("It's Sorcery!", "Remove SorcerySchema", false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void RemoveSorcerySchema(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if(comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"{pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
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

        [DebugAction("It's Sorcery!", "Remove all SorcerySchema", false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void RemoveAllSorcerySchema(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"{pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            // clear out all sorcery schemas
            List<SorcerySchema> schemas = comp?.schemaTracker?.sorcerySchemas;
            foreach (SorcerySchema schema in schemas) pawn.health.RemoveHediff(schema.progressTracker.hediff);
            schemas.Clear();
        }

        [DebugAction("It's Sorcery!", "Refresh all ProgressTrackers", false, false, false, false, 0, false,
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 999)]
        public static void RefreshProgressTrackers(Pawn pawn)
        {
            Comp_ItsSorcery comp = pawn.GetComp<Comp_ItsSorcery>();
            if (comp?.schemaTracker?.sorcerySchemas is null)
            {
                Messages.Message($"{pawn.Name.ToStringShort} has no It's Sorcery! comp.", MessageTypeDefOf.RejectInput);
                return;
            }

            foreach(SorcerySchema schema in comp?.schemaTracker?.sorcerySchemas)
            {
                SorcerySchemaUtility.RefreshProgressTracker(schema);
            }
        }


    }
}
