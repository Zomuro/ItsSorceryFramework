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
			JobDef jobDef = JobDefOf_ItsSorcery.ISF_ChargeSchema;
			Job job = JobMaker.MakeJob(jobDef, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;

			//Traverse traverse = new Traverse(job);
			//JobDriver_Charge driver = job.GetCachedDriverDirect as JobDriver_Charge;

			// use traverse to retrive jobdriver
			JobDriver_Charge driver = job.MakeDriver(pawn) as JobDriver_Charge;
			driver.schema = schema;
			driver.energyTrackerDef = energyTrackerDef;
			driver.energyPerAmmo = energyPerTarget;
			Traverse.Create(job).Field("cachedDriver").SetValue(driver);

			Log.Message(Traverse.Create(job).Field("cachedDriver").GetValue<JobDriver_Charge>().schema.def.defName);

			//JobDriver_Charge driver = 
			/*if (driver is null) return null; // no driver of type JobDriver_Charge is a problem.

			// assign energyTracker and energyPerAmmo value to it.
			driver.energyTracker = energyTracker;
			driver.energyPerAmmo = energyPerTarget;*/

			return job;
		}

		/*public static Job MakeChargeEnergyJobTest(Pawn pawn, EnergyTrackerComp comp, Thing target, int count)
		{
			JobDef jobDef = JobDefOf_ItsSorcery.ISF_ChargeSchema;
			*//*schemaJobDef.schemaDef = schema.def;
			schemaJobDef.energyTrackerDef = energyTracker.def;*//*
			Job job = JobMaker.MakeJob(jobDef, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;
			(job.GetCachedDriverDirect as JobDriver_Charge).energyComp = comp as EnergyTrackerComp_OnConsume;


			return job;
		}*/

	}
}
