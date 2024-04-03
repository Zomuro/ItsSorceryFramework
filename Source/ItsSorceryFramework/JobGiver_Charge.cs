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

		public static Job MakeChargeEnergyJob(Pawn pawn, SorcerySchema schema, EnergyTrackerDef energyTrackerDef, Thing target, int count, float energyPerTarget = 1f)
		{
			// add critical jobdriver information into a context dict
			EnergyTrackerContext.onConsumeContext[pawn.GetUniqueLoadID()] = new Tuple<SorcerySchema, EnergyTrackerDef, float>(schema, energyTrackerDef, energyPerTarget);

			// build job
			Job job = JobMaker.MakeJob(JobDefOf_ItsSorcery.ISF_ChargeSchema, pawn);
			job.targetB = new LocalTargetInfo(target);
			job.count = count;
			return job;
		}

	}
}
