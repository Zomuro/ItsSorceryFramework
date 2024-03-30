using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace ItsSorceryFramework
{
    public class JobGiver_Charge : JobGiver_Reload
    {
		protected override Job TryGiveJob(Pawn pawn) => null; // do not just give jobs automatically

		public static Job MakeChargeEnergyJob(Pawn pawn, SorcerySchema schema, Thing target, int count)
		{
			SchemaJobDef schemaJobDef = SchemaJobDefOf.ChargeSchema;
			schemaJobDef.schemaDef = schema.def;
			Job job = JobMaker.MakeJob(schemaJobDef, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;
			return job;
		}

		public static Job MakeChargeEnergyJobOld(Pawn pawn, SorcerySchema schema, EnergyTracker energyTracker, Thing target, int count)
		{
			SchemaJobDef schemaJobDef = SchemaJobDefOf.ChargeSchema;
			schemaJobDef.schemaDef = schema.def;
			schemaJobDef.energyTrackerDef = energyTracker.def;
			Job job = JobMaker.MakeJob(schemaJobDef, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;
			return job;
		}

		public static Job MakeChargeEnergyJob(Pawn pawn, SorcerySchema schema, EnergyTrackerDef energyTrackerDef, Thing target, int count, float energyPerTarget = 1f)
		{
			// add critical jobdriver information into a context dict
			EnergyTrackerContext.onConsumeContext[pawn.GetUniqueLoadID()] = new Tuple<SorcerySchema, EnergyTrackerDef, float>(schema, energyTrackerDef, energyPerTarget);

			// build job based on custom job def we constructed.
			Job job = JobMaker.MakeJob(JobDefOf_ItsSorcery.ISF_ChargeSchema, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;
			return job;
		}

	}
}
