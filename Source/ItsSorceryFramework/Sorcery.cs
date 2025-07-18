﻿using RimWorld;
using RimWorld.Planet;
using System.Linq;
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
            Scribe_Values.Look(ref visible, "visible", true);
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

        public override AcceptanceReport CanCast
        {
            get
            {
                if (!base.CanCast) return base.CanCast;
                foreach (var et in Schema.energyTrackers) if (et.WouldReachLimitEnergy(def.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0) * et.EnergyCostFactor, sorceryDef)) return false;
                return true;
            }
        }

        public override bool Activate(GlobalTargetInfo target)
        {
            foreach (var et in Schema.energyTrackers)
            {
                if (!et.TryAlterEnergy(sorceryDef.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0f) * et.EnergyCostFactor, sorceryDef)) return false;
            }

            // Schema.progressTracker.def.Workers
            var worker = Schema.progressTracker.currClassDef.Workers.FirstOrDefault(x => x.GetType() == typeof(ProgressEXPWorker_CastEnergyCost));
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
            foreach(var et in Schema.energyTrackers)
            {
                if (!et.TryAlterEnergy(sorceryDef.statBases.GetStatValueFromList(et.def.energyUnitStatDef, 0f) * et.EnergyCostFactor, sorceryDef)) return false;
            }

            // Schema.progressTracker.def.Workers
            var worker = Schema.progressTracker.currClassDef.Workers.FirstOrDefault(x => x.GetType() == typeof(ProgressEXPWorker_CastEnergyCost));
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

        public bool visible = true;
    }
}
