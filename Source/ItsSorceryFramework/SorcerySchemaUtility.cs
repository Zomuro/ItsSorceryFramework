﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ItsSorceryFramework
{
    public static class SorcerySchemaUtility
    {
        public static List<EnergyTrackerDef> cachedEnergyTrackerDefs = new List<EnergyTrackerDef>();

        public static List<LearningTrackerDef> cachedLearningTrackerDefs = new List<LearningTrackerDef>();

        public static List<ProgressTrackerDef> cachedProgressTrackerDefs = new List<ProgressTrackerDef>();

        public static List<SorcerySchemaDef> cachedSorcerySchemaDef = new List<SorcerySchemaDef>();

        public static List<EnergyTrackerDef> AllEnergyTrackerDefs
        {
            get
            {
                if (cachedEnergyTrackerDefs.NullOrEmpty()) cachedEnergyTrackerDefs = DefDatabase<EnergyTrackerDef>.AllDefsListForReading;
                return cachedEnergyTrackerDefs;
            }
        }

        public static List<LearningTrackerDef> AllLearningTrackerDefs
        {
            get
            {
                if (cachedLearningTrackerDefs.NullOrEmpty()) cachedLearningTrackerDefs = DefDatabase<LearningTrackerDef>.AllDefsListForReading;
                return cachedLearningTrackerDefs;
            }
        }

        public static List<ProgressTrackerDef> AllProgressTrackerDefs
        {
            get
            {
                if (cachedProgressTrackerDefs.NullOrEmpty()) cachedProgressTrackerDefs = DefDatabase<ProgressTrackerDef>.AllDefsListForReading;
                return cachedProgressTrackerDefs;
            }
        }

        public static List<SorcerySchemaDef> AllSorcerySchemaDefs
        {
            get
            {
                if (cachedSorcerySchemaDef.NullOrEmpty()) cachedSorcerySchemaDef = DefDatabase<SorcerySchemaDef>.AllDefsListForReading;
                return cachedSorcerySchemaDef;
            }
        }

        public static SorcerySchema InitializeSorcerySchema(Pawn pawn, SorcerySchemaDef def)
        {
            return new SorcerySchema(pawn, def);
        }

        public static Comp_ItsSorcery GetSorceryComp(Pawn pawn)
        {
            return pawn.TryGetComp<Comp_ItsSorcery>();
        }

        public static void AddSorcerySchema(Pawn pawn, SorcerySchemaDef def)
        {
            Comp_ItsSorcery schemaComp = GetSorceryComp(pawn);
            if (schemaComp.schemaTracker.sorcerySchemas.FirstOrDefault(x => x.def == def) != null) return;
            schemaComp.schemaTracker.sorcerySchemas.Add(InitializeSorcerySchema(pawn, def));
        }

        public static void AddSorcerySchema(Pawn pawn, SorcerySchemaDef def, out SorcerySchema schema)
        {
            Comp_ItsSorcery schemaComp = GetSorceryComp(pawn);
            if (schemaComp?.schemaTracker?.sorcerySchemas?.FirstOrDefault(x => x.def == def) is SorcerySchema found && found != null)
            {
                schema = found;
                return;
            }

            schema = InitializeSorcerySchema(pawn, def);
            schemaComp.schemaTracker.sorcerySchemas.Add(schema);
        }

        public static void RemoveSorcerySchema(Pawn pawn, SorcerySchemaDef def)
        {
            Comp_ItsSorcery schemaComp = GetSorceryComp(pawn);

            // nullcheck if the pawn has a schema
            SorcerySchema targetSchema = schemaComp?.schemaTracker?.sorcerySchemas?.FirstOrDefault(x => x.def == def);
            if (targetSchema is null) return;

            // remove schema hediff and schema
            pawn.health.RemoveHediff(targetSchema.progressTracker.Hediff);
            schemaComp.schemaTracker.sorcerySchemas.Remove(targetSchema);
        }

        public static void RemoveSorcerySchema(Pawn pawn, SorcerySchema schema)
        {
            Comp_ItsSorcery schemaComp = GetSorceryComp(pawn);

            // nullcheck if the pawn has a schema
            SorcerySchema targetSchema = schemaComp?.schemaTracker?.sorcerySchemas?.FirstOrDefault(x => x == schema);
            if (targetSchema is null) return;

            // remove schema hediff and schema
            pawn.health.RemoveHediff(targetSchema.progressTracker.Hediff);
            schemaComp.schemaTracker.sorcerySchemas.Remove(targetSchema);
        }

        public static List<SorcerySchema> GetSorcerySchemaList(Pawn pawn)
        {
            Comp_ItsSorcery comp = GetSorceryComp(pawn);

            if (comp != null)
            {
                return comp?.schemaTracker?.sorcerySchemas ?? null;
            }

            return null;
        }

        public static SorcerySchema FindSorcerySchema(Pawn pawn, SorceryDef def)
        {
            if (pawn == null) return null;
            Comp_ItsSorcery comp = GetSorceryComp(pawn);
            return comp?.schemaTracker?.sorcerySchemas.FirstOrDefault(s => s.def == def.sorcerySchema);
        }

        public static SorcerySchema FindSorcerySchema(Pawn pawn, SorcerySchemaDef def)
        {
            if (pawn == null) return null;
            Comp_ItsSorcery comp = GetSorceryComp(pawn);
            return comp?.schemaTracker?.sorcerySchemas.FirstOrDefault(s => s.def == def);
        }

        public static EnergyTracker GetEnergyTracker(SorcerySchema schema, StatDef unitStat)
        {
            foreach(var et in schema.energyTrackers)
            {
                if (et.def.energyUnitStatDef == unitStat) return et;
            }
            return null;
        }

        public static void RefreshProgressTracker(SorcerySchema schema)
        {
            schema.progressTracker.Hediff.cachedCurStage = schema.progressTracker.RefreshCurStage();
        }

    }
}
