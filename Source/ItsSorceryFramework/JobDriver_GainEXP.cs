using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    public class JobDriver_GainEXP: JobDriver_Reload
    {
		public SorcerySchema schema;

		public int ammoCountUse = 0;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref schema, "schema");
			Scribe_Values.Look(ref ammoCountUse, "ammoCountUse", 0, false);
		}

		public Thing consumable => job.GetTarget(TargetIndex.B).Thing;

		public override string GetReport()
		{
			return "ISF_ReportOnConsumeProgress".Translate(consumable.Label.Named("ITEM"), schema.def.LabelCap.Named("SCHEMA"));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			// retrive values from context; if they are null, rely on saved values
			Tuple<SorcerySchema, float> context = ProgressTrackerContext.GetConsumeContext(pawn.GetUniqueLoadID());
			schema = schema is null ? context.Item1 : schema;
			
			// save the ammo to use for later
			ammoCountUse = (ammoCountUse == 0f && job.count > -1) ? job.count : ammoCountUse;

			//SorcerySchemaDef schemaDef = schema.def; //(this.job.def as SchemaJobDef).schemaDef;
			int count = job.count;

			// dev mode message
			if (Prefs.DevMode) Log.Message($"Job {job.GetUniqueLoadID()}: schema {schema.def.LabelCap}, item count {count}");

			// toils for moving to target item and picking it up
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
			Toil getNextIngredient = Toils_General.Label();
			yield return getNextIngredient;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).
				FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false, true).
				FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Jump.JumpIf(getNextIngredient, () => !this.job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());
			
			// conduct the xp gain method to get its toils
			foreach (Toil reloadToil in GainEXPAsMuchAsPossible()) yield return reloadToil;

			Toil toil3 = ToilMaker.MakeToil("MakeNewToils");
			toil3.initAction = delegate ()
			{
				Thing carriedThing = this.pawn.carryTracker.CarriedThing;
				if (carriedThing != null && !carriedThing.Destroyed)
				{
					Thing thing;
					pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out thing, null);
				}
			};
			toil3.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil3;
			yield break;
		}

		public IEnumerable<Toil> GainEXPAsMuchAsPossible()
		{
			Toil done = Toils_General.Label();
			yield return Toils_Jump.JumpIf(done, () => pawn.carryTracker.CarriedThing == null ||
				pawn.carryTracker.CarriedThing.stackCount < this.job.count);
			yield return Toils_General.Wait(60, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			Toil toil = ToilMaker.MakeToil("ChargeAsMuchAsPossible");
			toil.initAction = delegate ()
			{
				GainEXPFrom(pawn.carryTracker.CarriedThing);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield return done;
			yield break;
		}

		public void GainEXPFrom(Thing thing)
		{
			if (thing.stackCount < 1) return;

			foreach (var worker in schema.progressTracker.def.Workers)
			{
				if (worker.GetType() == typeof(ProgressEXPWorker_UseItem))
				{
					foreach (var item in worker.def.expItems)
					{
						if (item.thingDef != thing.def) continue;
						float factor = item.expFactorStat != null ? pawn.GetStatValue(item.expFactorStat) : 1f;
						schema.progressTracker.AddExperience(item.exp * factor);
						MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, (item.exp * factor).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset) + " EXP");
						break;
					}
					break;
				}
			}
			thing.SplitOff(1).Destroy(DestroyMode.Vanish);
			ProgressTrackerContext.RemoveConsumeContext(pawn.GetUniqueLoadID());
		}

	}
}
