using RimWorld;
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

        public static Comp_ItsSorcery GetSorceryComp(Pawn pawn)
        {
            return pawn.TryGetComp<Comp_ItsSorcery>();
        }

        public static SorcerySchema InitializeSorcerySchema(Pawn pawn, SorcerySchemaDef def)
        {
            return new SorcerySchema(pawn, def);
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

        public static EnergyTracker GetEnergyTracker(SorcerySchema schema, EnergyTrackerDef energyTrackerDef)
        {
            return schema.energyTrackers.FirstOrDefault(x => x.def == energyTrackerDef);
        }

        public static EnergyTracker GetEnergyTracker(Pawn pawn, SorcerySchemaDef schemaDef, EnergyTrackerDef energyTrackerDef)
        {
            List<SorcerySchema> schemas = GetSorcerySchemaList(pawn);
            if (schemas.NullOrEmpty()) return null;

            SorcerySchema schema = schemas.FirstOrDefault(x => x.def == schemaDef);
            if (schema is null) return null;

            return schema.energyTrackers.FirstOrDefault(x => x.def == energyTrackerDef);
        }

        public static EnergyTracker GetEnergyTracker(Pawn_SorcerySchemaTracker schemaTracker, SorcerySchemaDef schemaDef, EnergyTrackerDef energyTrackerDef)
        {
            SorcerySchema schema = schemaTracker.sorcerySchemas.FirstOrDefault(x => x.def == schemaDef);
            if (schema is null) return null;
            return schema.energyTrackers.FirstOrDefault(x => x.def == energyTrackerDef);
        }

        public static void RefreshProgressTracker(SorcerySchema schema)
        {
            schema.progressTracker.Hediff.cachedCurStage = schema.progressTracker.RefreshCurStage();
        }

        public static void AddQuickEnergyEntry(Pawn pawn, SorcerySchema schema, EnergyTracker energyTracker)
        {
            // null check pawn/comp
            if (pawn == null) return;
            Comp_ItsSorcery comp = GetSorceryComp(pawn);
            if (comp is null) return;

            // check if there's an entry already w/ the schema/energytracker combo - if so, skip
            if (comp.schemaTracker.quickEnergyEntries.
                FirstOrDefault(x => x.sorcerySchemaDef == schema.def && x.energyTrackerDef == energyTracker.def) != null)
            {
                // add quick msg/popup stating a prior entry should be removed
                Messages.Message("ISF_QuickEnergyGizmoEntryExisting".Translate(energyTracker.EnergyLabel.CapitalizeFirst()), MessageTypeDefOf.RejectInput);
                return;
            }

            // if adding another entry > 5, skip it.
            if (comp.schemaTracker.quickEnergyEntries.Count + 1 > 5)
            {
                // add quick msg/popup stating a prior entry should be removed
                Messages.Message("ISF_QuickEnergyGizmoEntryFull".Translate(pawn.Named("PAWN")), MessageTypeDefOf.RejectInput);
                return;
            }

            // else add it
            comp.schemaTracker.quickEnergyEntries.Add(new GizmoEntry_QuickEnergy(schema.def, energyTracker.def));
            Messages.Message("ISF_QuickEnergyGizmoEntryAdded".Translate(comp.schemaTracker.quickEnergyEntries.Count, 5), MessageTypeDefOf.TaskCompletion);
            comp.schemaTracker.UpdateGizmo();
        }

        public static void RemoveQuickEnergyEntry(Pawn pawn, SorcerySchema schema, EnergyTracker energyTracker)
        {
            if (pawn == null) return;
            Comp_ItsSorcery comp = GetSorceryComp(pawn);
            if (comp is null) return;

            GizmoEntry_QuickEnergy entry = comp.schemaTracker.quickEnergyEntries.
                FirstOrDefault(x => x.sorcerySchemaDef == schema.def && x.energyTrackerDef == energyTracker.def);
            if (entry is null) return;
            if (comp.schemaTracker.quickEnergyEntries.Remove(entry)) // successful removal => update gizmo
            {
                comp.schemaTracker.UpdateGizmo();
                Messages.Message("ISF_QuickEnergyGizmoEntryRemoved".Translate(comp.schemaTracker.quickEnergyEntries.Count, 5), MessageTypeDefOf.TaskCompletion);
            }
        }

    }
}
