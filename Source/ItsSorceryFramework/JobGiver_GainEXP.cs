using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    public class JobGiver_GainEXP : JobGiver_Reload
    {
		protected override Job TryGiveJob(Pawn pawn)
		{
			return null;
		}

		public static Job MakeChargeEXPJob(Pawn pawn, SorcerySchema schema, Thing target, int count)
		{
			SchemaJobDef schemaJobDef = SchemaJobDefOf.GainEXPSchema;
			schemaJobDef.schemaDef = schema.def;
			Job job = JobMaker.MakeJob(schemaJobDef, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;
			return job;
		}

	}
}
