using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ItsSorceryFramework
{
    public class Sorcery : Ability
    {
        public Sorcery(Pawn pawn) : base(pawn)
        {
            this.pawn = pawn;
        }

        public Sorcery(Pawn pawn, SorceryDef def) : base(pawn, def)
        {
            this.pawn = pawn;
            this.def = def;
            this.sorceryDef = def;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref this.sorceryDef, "sorceryDef");
        }

        public SorcerySchema Schema
        {
            get
            {
                if(sorcerySchema == null)
                {
                    sorcerySchema = SorcerySchemaUtility.FindSorcerySchema(pawn, sorceryDef);
                }
                return sorcerySchema;
            }
        }

        public override bool Activate(GlobalTargetInfo target)
        {
            // disable for now; get the proper energytrackers first
            /*EnergyTracker energyTracker = SorcerySchemaUtility.FindSorcerySchema(pawn, sorceryDef).energyTracker;

            if (energyTracker == null) return false;

            if (!energyTracker.TryAlterEnergy(sorceryDef.EnergyCost))
            {
                return false;
            }

            foreach (ProgressEXPWorker worker in Schema.progressTracker.def.Workers)
            {
                if(worker.GetType() == typeof(ProgressEXPWorker_CastEnergyCost))
                {
                    worker.TryExecute(Schema.progressTracker, sorceryDef.EnergyCost);
                    break;
                }
            }*/

            foreach (var et in Schema.energyTrackers)
            {
                if (!et.TryAlterEnergy(sorceryDef.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0f) * et.EnergyCostFactor, sorceryDef)) return false;
            }

            var worker = Schema.progressTracker.def.Workers.FirstOrDefault(x => x.GetType() == typeof(ProgressEXPWorker_CastEnergyCost));
            if (worker != null)
            {
                foreach (var et in Schema.energyTrackers)
                {
                    worker.TryExecute(Schema.progressTracker, sorceryDef.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0f));
                }
            }

            return base.Activate(target);
            
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            /*EnergyTracker energyTracker = SorcerySchemaUtility.FindSorcerySchema(pawn, sorceryDef).energyTracker;
            if (energyTracker == null) return false;

            float finalEnergyCost = sorceryDef.EnergyCost * energyTracker.EnergyCostFactor;
            if (!energyTracker.TryAlterEnergy(finalEnergyCost, sorceryDef, this))
            {
                return false;
            }

            var worker = Schema.progressTracker.def.Workers.FirstOrDefault(x => x.GetType() == typeof(ProgressEXPWorker_CastEnergyCost));
            if(worker != null) worker.TryExecute(Schema.progressTracker, sorceryDef.EnergyCost);*/

            foreach(var et in Schema.energyTrackers)
            {
                if (!et.TryAlterEnergy(sorceryDef.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0f) * et.EnergyCostFactor, sorceryDef)) return false;
            }

            var worker = Schema.progressTracker.def.Workers.FirstOrDefault(x => x.GetType() == typeof(ProgressEXPWorker_CastEnergyCost));
            if (worker != null)
            {
                foreach (var et in Schema.energyTrackers)
                {
                    worker.TryExecute(Schema.progressTracker, sorceryDef.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0f));
                }
            }
            return base.Activate(target, dest);
        }

        public virtual string SorceryTooltip
        {
            get
            {
                string text = sorceryDef.GetSorceryTooltip(this.pawn);
                if (this.EffectComps != null)
                {
                    foreach (CompAbilityEffect compAbilityEffect in this.EffectComps)
                    {
                        string text2 = compAbilityEffect.ExtraTooltipPart();
                        if (!text2.NullOrEmpty())
                        {
                            text = text + "\n\n" + text2;
                        }
                    }
                }
                return text;
            }
        }


        public SorceryDef sorceryDef;

        public SorcerySchema sorcerySchema;
    }
}
