using System;
using System.Collections.Generic;
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
			// add critical jobdriver information into a context dict
			ProgressTrackerContext.onConsumeContext[pawn.GetUniqueLoadID()] = new Tuple<SorcerySchema, float>(schema, count);

			// build job
			Job job = JobMaker.MakeJob(JobDefOf_ItsSorcery.ISF_GainEXPSchema, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;
			return job;
		}

	}
}
