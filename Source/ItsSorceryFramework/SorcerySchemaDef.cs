using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class SorcerySchemaDef : Def
    {
        public Pawn TempPawn
        {
            get
            {
                return pawn;
            }
            set
            {
                this.pawn = value;
            }
        }
        
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            // no TempPawn in the def assigned = no special display stats.
            if (TempPawn == null) yield break;

            //Pawn held = TempPawn;

            base.SpecialDisplayStats(req);

            
            

            // more or less all energytrackers have a "unit" that is used
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    "ISF_EnergyTrackerUnit".Translate(), energyTrackerDef.EnergyLabelTranslationKey.Translate().CapitalizeFirst(),
                    energyTrackerDef.EnergyDescTranslationKey.Translate(), 99999, null, null, false);

            // depending on energytrackers, alter which ones show up
            Log.Message((SorcerySchemaUtility.FindSorcerySchema(TempPawn, this) == null).ToString());
            foreach(StatDrawEntry entry in SorcerySchemaUtility.FindSorcerySchema(TempPawn, this).energyTracker.SpecialDisplayStats(req))
            {
                yield return entry;
            }


            /*statDef = energyTrackerDef.energyMaxStatDef != null ? energyTrackerDef.energyMaxStatDef : StatDefOf_ItsSorcery.MaxEnergy_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, TempPawn.GetStatValue(statDef), req, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            statDef = energyTrackerDef.energyOverMaxStatDef != null ? energyTrackerDef.energyOverMaxStatDef : StatDefOf_ItsSorcery.OverMaxEnergy_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, TempPawn.GetStatValue(statDef), req, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            statDef = energyTrackerDef.energyMinStatDef != null ? energyTrackerDef.energyMinStatDef : StatDefOf_ItsSorcery.MinEnergy_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, TempPawn.GetStatValue(statDef), req, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            statDef = energyTrackerDef.energyRecoveryStatDef != null ? energyTrackerDef.energyRecoveryStatDef : StatDefOf_ItsSorcery.EnergyRecovery_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, TempPawn.GetStatValue(statDef), req, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            statDef = energyTrackerDef.energyCostFactorStatDef != null ? energyTrackerDef.energyCostFactorStatDef : StatDefOf_ItsSorcery.EnergyCostFactor_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, TempPawn.GetStatValue(statDef), req, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            statDef = energyTrackerDef.castFactorStatDef != null ? energyTrackerDef.castFactorStatDef : StatDefOf_ItsSorcery.CastFactor_ItsSorcery;
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    statDef, TempPawn.GetStatValue(statDef), req, ToStringNumberSense.Undefined, statDef.displayPriorityInCategory, false);

            //over and under bar drain multiplier
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    "EnergyTrackerUnderBarFactor_ISF".Translate(), energyTrackerDef.underBarRecoveryFactor.ToString(),
                    "A multiplier on recovery rate when the current energy level is below 0.", 40, null, null, false);

            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    "EnergyTrackerOverBarFactor_ISF".Translate(), energyTrackerDef.overBarRecoveryFactor.ToString(),
                    "A multiplier on recovery rate when the current energy level is above the maximum energy.", 
                    30, null, null, false);

            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    "Turn time (secs)", energyTrackerDef.turnTicks.TicksToSeconds().ToString(),
                    "The amount of time a \"turn\" (the period of time between automatic game pauses when the pawn is drafted) takes in seconds.", 
                    20, null, null, false);

            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    "Refresh time (secs)", energyTrackerDef.refreshTicks.TicksToSeconds().ToString(),
                    "The amount of time it takes a sorcery schema to refresh various values, often discrete casts, in seconds.", 
                    10, null, null, false);*/

            yield break;
        }

        // don't know if we need this: most of the custom stuff arises from the trackers linked to the schema
        //public Type sorcerySchemaClass = typeof(SorcerySchema);

        //public StatDef energyStat; 

        public EnergyTrackerDef energyTrackerDef;

        public List<LearningTrackerDef> learningTrackerDefs = new List<LearningTrackerDef>();

        public ProgressTrackerDef progressTrackerDef;

        public List<SorcerySchemaDef> incompatibleSchemas;

        private Pawn pawn;

        
    }
}
