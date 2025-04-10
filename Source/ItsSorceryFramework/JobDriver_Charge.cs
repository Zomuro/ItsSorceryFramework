using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    public class JobDriver_Charge : JobDriver_Reload
    {
		public SorcerySchema schema;

		public EnergyTrackerDef energyTrackerDef;

		public float energyPerAmmo;

		public int ammoCountUse = 0;

		public override void ExposeData()
		{
			base.ExposeData();
            Scribe_References.Look(ref schema, "schema");
            Scribe_Defs.Look(ref energyTrackerDef, "energyTrackerDef");
			Scribe_Values.Look(ref energyPerAmmo, "energyPerAmmo", 1f, false);
			Scribe_Values.Look(ref ammoCountUse, "ammoCountUse", 0, false);
		}

		public Thing consumable => job.GetTarget(TargetIndex.B).Thing;

		public override string GetReport()
		{
			if(schema is null) return "ISF_ReportOnConsumeEnergyPrep".Translate();
			return "ISF_ReportOnConsumeEnergy".Translate(energyTrackerDef.energyUnitStatDef.label.Named("UNIT"), schema.def.LabelCap.Named("SCHEMA"),
				consumable.Label.Named("ITEM"));
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			// retrive values from context; if they are null, rely on saved values
			Tuple<SorcerySchema, EnergyTrackerDef, float> context = EnergyTrackerContext.GetConsumeContext(pawn.GetUniqueLoadID());
			schema = schema is null ? context.Item1 : schema;
			energyTrackerDef = energyTrackerDef is null ? context.Item2 : energyTrackerDef;
			energyPerAmmo = context.Item3 != 0f ? context.Item3 : energyPerAmmo;

			// save the ammo to use for later
			ammoCountUse = (ammoCountUse == 0f && job.count > -1) ? job.count : ammoCountUse;

			// dev mode message
			if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug) Log.Message($"[It's Sorcery!] Job {job.GetUniqueLoadID()}: " +
				$"schema {schema.def.LabelCap}, energytracker {energyTrackerDef.defName}, energy per item {energyPerAmmo}, item count {ammoCountUse}");

			//int count = job.count;
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
			Toil getNextIngredient = Toils_General.Label();
			yield return getNextIngredient;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).
				FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false, true).
				FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());

			EnergyTracker energyTracker = schema.energyTrackers.FirstOrDefault(x => x.def == energyTrackerDef);
			if (energyTracker is null) yield break;

			foreach (Toil reloadToil in ChargeAsMuchAsPossible(energyTracker, ammoCountUse)) yield return reloadToil;
			Toil toil3 = ToilMaker.MakeToil("MakeNewToils");
			toil3.initAction = delegate ()
			{
				Thing carriedThing = pawn.carryTracker.CarriedThing;
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

		public IEnumerable<Toil> ChargeAsMuchAsPossible(EnergyTracker energyTracker, int count)
		{
			Toil done = Toils_General.Label();
			yield return Toils_Jump.JumpIf(done, () => pawn.carryTracker.CarriedThing == null ||
				this.pawn.carryTracker.CarriedThing.stackCount < job.count);
			yield return Toils_General.Wait(60, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			Toil toil = ToilMaker.MakeToil("ChargeAsMuchAsPossible");
			toil.initAction = delegate ()
			{
				Thing carriedThing = pawn.carryTracker.CarriedThing;
				ChargeFrom(carriedThing, energyTracker, count);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield return done;
			yield break;
		}

		public void ChargeFrom(Thing ammo, EnergyTracker energyTracker, int count)
		{
            // inconsistency between the count to remove and the stack count such that we can't fufill the job? skip it
            if (ammo.stackCount < count) return;

			float directEnergyDiff = 0f;
			if (!energyTracker.def.inverse) directEnergyDiff = Mathf.Max(0f, energyTracker.schema.limitLocked ? 
				energyTracker.MaxEnergy - energyTracker.currentEnergy : energyTracker.AbsMaxEnergy - energyTracker.currentEnergy);
			else directEnergyDiff = Mathf.Max(0f, energyTracker.schema.limitLocked ? 
				energyTracker.currentEnergy - energyTracker.MinEnergy : energyTracker.currentEnergy - energyTracker.AbsMinEnergy);

			energyTracker.AddEnergy(Mathf.Min(count * energyPerAmmo, directEnergyDiff));
			//energyTracker.currentEnergy += energyTracker.InvMult * Mathf.Min(count * energyPerAmmo, directEnergyDiff);
			ammo.SplitOff(count).Destroy(DestroyMode.Vanish);
			EnergyTrackerContext.RemoveConsumeContext(pawn.GetUniqueLoadID());
		}

	}
}
