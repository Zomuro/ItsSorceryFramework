using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    public class JobDriver_GainEXP: JobDriver_Reload
    {
		protected override IEnumerable<Toil> MakeNewToils()
		{
			SorcerySchemaDef schemaDef = (this.job.def as SchemaJobDef).schemaDef;
			int count = job.count;
			Log.Message(schemaDef.label);
			Log.Message(count.ToString());
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
			Toil getNextIngredient = Toils_General.Label();
			yield return getNextIngredient;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).
				FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false, true).
				FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Jump.JumpIf(getNextIngredient, () => !this.job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());
			
			foreach (Toil reloadToil in this.GainEXPAsMuchAsPossible(schemaDef))
			{
				yield return reloadToil;
			}
		
			Toil toil3 = ToilMaker.MakeToil("MakeNewToils");
			toil3.initAction = delegate ()
			{
				Thing carriedThing = this.pawn.carryTracker.CarriedThing;
				if (carriedThing != null && !carriedThing.Destroyed)
				{
					Thing thing;
					this.pawn.carryTracker.TryDropCarriedThing(this.pawn.Position, ThingPlaceMode.Near, out thing, null);
				}
			};
			toil3.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil3;
			yield break;
		}

		public IEnumerable<Toil> GainEXPAsMuchAsPossible(SorcerySchemaDef schemaDef)
		{
			Toil done = Toils_General.Label();
			yield return Toils_Jump.JumpIf(done, () => this.pawn.carryTracker.CarriedThing == null || 
				this.pawn.carryTracker.CarriedThing.stackCount < this.job.count);
			yield return Toils_General.Wait(60, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			Toil toil = ToilMaker.MakeToil("ChargeAsMuchAsPossible");
			toil.initAction = delegate ()
			{
				Thing carriedThing = this.pawn.carryTracker.CarriedThing;
				this.GainEXPFrom(carriedThing, schemaDef);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield return done;
			yield break;
		}

		public void GainEXPFrom(Thing thing, SorcerySchemaDef schemaDef)
		{
			if (thing.stackCount < 1)
			{
				return;
			}
			ProgressTracker pt = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef).progressTracker;

			foreach (var worker in pt.def.Workers)
            {
				if(worker.GetType() == typeof(ProgressEXPWorker_UseItem))
                {
					foreach(var item in worker.def.expItems)
                    {
						if (item.thingDef != thing.def) continue;
						float factor = item.expFactorStat != null ? pawn.GetStatValue(item.expFactorStat) : 1f;
						pt.addExperience(item.exp * factor);
						MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, (item.exp * factor).ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset) + " EXP");
						break;
					}
					break;
                }
            }
			thing.SplitOff(1).Destroy(DestroyMode.Vanish);
		}

	}
}
