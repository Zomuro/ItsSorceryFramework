using RimWorld;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    public class JobGiver_Charge : JobGiver_Reload
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
			return null;
		}

		public static Job MakeChargeEnergyJob(Pawn pawn, SorcerySchema schema, Thing target, int count)
		{
			SchemaJobDef schemaJobDef = SchemaJobDefOf.ChargeSchema;
			schemaJobDef.schemaDef = schema.def;
			Job job = JobMaker.MakeJob(schemaJobDef, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;
			return job;
		}

	}
}
