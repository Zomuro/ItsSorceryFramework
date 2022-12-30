using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

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
            EnergyTracker energyTracker = SorcerySchemaUtility.FindSorcerySchema(pawn, sorceryDef).energyTracker;

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
            }

            /*foreach (ProgressEXPDef expDef in Schema.progressTracker.def.expTags)
            {
                if (expDef.workerClass == typeof(ProgressEXPWorker_CastEnergyCost))
                {
                    expDef.Worker.TryExecute(Schema.progressTracker, sorceryDef.EnergyCost);
                    break;
                }
            }*/

            //Log.Message("cast: "+ sorceryDef.EnergyCost.ToString());
            return base.Activate(target);
            
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            EnergyTracker energyTracker = SorcerySchemaUtility.FindSorcerySchema(pawn, sorceryDef).energyTracker;
            if (energyTracker == null) return false;

            float finalEnergyCost = sorceryDef.EnergyCost * energyTracker.EnergyCostFactor;
            if (!energyTracker.TryAlterEnergy(finalEnergyCost, sorceryDef, this))
            {
                return false;
            }

            foreach (ProgressEXPWorker worker in Schema.progressTracker.def.Workers)
            {
                if (worker.GetType() == typeof(ProgressEXPWorker_CastEnergyCost))
                {
                    worker.TryExecute(Schema.progressTracker, sorceryDef.EnergyCost);
                    break;
                }
            }

            //Log.Message("cast: " + (sorceryDef.EnergyCost * energyTracker.EnergyCostFactor).ToString());
            return base.Activate(target, dest);
        }

        public virtual string SorceryTooltip
        {
            get
            {
                string text = this.sorceryDef.GetSorceryTooltip(this.pawn);
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
