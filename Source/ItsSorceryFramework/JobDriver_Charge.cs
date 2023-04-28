using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    public class JobDriver_Charge : JobDriver_Reload
    {
		protected override IEnumerable<Toil> MakeNewToils()
		{
			SorcerySchemaDef schemaDef = (this.job.def as SchemaJobDef).schemaDef;
			EnergyTrackerDef energyTrackerDef = (this.job.def as SchemaJobDef).energyTrackerDef;
			int count = job.count;
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
			Toil getNextIngredient = Toils_General.Label();
			yield return getNextIngredient;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).
				FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false, true).
				FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Jump.JumpIf(getNextIngredient, () => !this.job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());

			foreach (Toil reloadToil in ChargeAsMuchAsPossible(schemaDef, energyTrackerDef, count))
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

		public IEnumerable<Toil> ChargeAsMuchAsPossible(SorcerySchemaDef schemaDef, EnergyTrackerDef energyTrackerDef, int count)
		{
			Toil done = Toils_General.Label();
			yield return Toils_Jump.JumpIf(done, () => this.pawn.carryTracker.CarriedThing == null ||
				this.pawn.carryTracker.CarriedThing.stackCount < this.job.count);
			yield return Toils_General.Wait(60, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			Toil toil = ToilMaker.MakeToil("ChargeAsMuchAsPossible");
			toil.initAction = delegate ()
			{
				Thing carriedThing = this.pawn.carryTracker.CarriedThing;
				//ChargeFrom(carriedThing, schemaDef, count);
				ChargeFrom(carriedThing, schemaDef, energyTrackerDef, count);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield return done;
			yield break;
		}

		public void ChargeFrom(Thing ammo, SorcerySchemaDef schemaDef, EnergyTrackerDef energyTrackerDef, int count)
		{
			// temp disable for now
			if (ammo.stackCount < count)
			{
				return;
			}
			EnergyTracker et = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef).energyTrackers.FirstOrDefault(x => x.def == energyTrackerDef);
			if (et == null || et.def.consumables.NullOrEmpty()) return;

			EnergyConsumable consume = et.def.consumables.FirstOrDefault(x => x.thingDef == ammo.def);
			if (consume is null) return;

			et.currentEnergy += Math.Min(count * consume.energy, et.MaxEnergy - et.currentEnergy);
			ammo.SplitOff(count).Destroy(DestroyMode.Vanish);
		}

	}
}
