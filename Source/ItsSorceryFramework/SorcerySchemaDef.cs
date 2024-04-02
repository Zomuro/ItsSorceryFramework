﻿using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class SorcerySchemaDef : Def
    {
        
        public List<EnergyTrackerDef> energyTrackerDefs = new List<EnergyTrackerDef>();

        public List<LearningTrackerDef> learningTrackerDefs = new List<LearningTrackerDef>();

        public ProgressTrackerDef progressTrackerDef;

        public List<SorcerySchemaDef> incompatibleSchemas;

        private Pawn pawn; // used for infocard

        private SorcerySchema cachedSchema; // used for infocard

        // don't know if we need this: most of the custom stuff arises from the trackers linked to the schema
        //public Type sorcerySchemaClass = typeof(SorcerySchema);

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

        public SorcerySchema Schema
        {
            get
            {
                if(cachedSchema is null)
                {
                    cachedSchema = SorcerySchemaUtility.FindSorcerySchema(TempPawn, this);
                }
                return cachedSchema;
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            // no TempPawn in the def assigned = no special display stats.
            if (TempPawn == null) yield break;

            //Pawn held = TempPawn;

            base.SpecialDisplayStats(req);

            int i = 0;
            foreach (var et in Schema.energyTrackers)
            {
                // creates temporary statcategeorydef to properly sort stats into their right place
                // CANNOT BE SAVED
                StatCategoryDef tempCat = new StatCategoryDef();
                tempCat.defName = StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF.defName + "_TEMP" + i;
                tempCat.label = StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF.LabelCap + " (" + et.def.label + ")";
                tempCat.displayOrder = StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF.displayOrder + i;

                et.tempStatCategory = tempCat;
                foreach (StatDrawEntry entry in et.SpecialDisplayStats(req)) yield return entry; // depending on energytrackers, alter which ones show up

                i++;
            }

            /*
            // more or less all energytrackers have a "unit" that is used
            yield return new StatDrawEntry(StatCategoryDefOf_ItsSorcery.EnergyTracker_ISF,
                    "ISF_EnergyTrackerUnit".Translate(), energyTrackerDef.energyLabelKey.Translate().CapitalizeFirst(),
                    energyTrackerDef.energyDescKey.Translate(), 99999, null, null, false);

            // depending on energytrackers, alter which ones show up
            foreach(StatDrawEntry entry in Schema.energyTracker.SpecialDisplayStats(req))
            {
                yield return entry;
            }*/

            yield break;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            bool error = false;

            if (energyTrackerDefs.NullOrEmpty())
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} cannot have null or no EnergyTrackers.");
            }

            if (!energyTrackerDefs.NullOrEmpty() && energyTrackerDefs.Count != energyTrackerDefs.Distinct().Count())
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} cannot have duplicate EnergyTrackers.");
            }

            if (learningTrackerDefs.NullOrEmpty())
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} cannot have null or no LearningTrackers.");
            }

            if (!learningTrackerDefs.NullOrEmpty() && learningTrackerDefs.Count != learningTrackerDefs.Distinct().Count())
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} cannot have duplicate LearningTrackers.");
            }

            if (progressTrackerDef is null)
            {
                error = true;
                Log.Warning($"It's Sorcery! Error: {defName} cannot have a null ProgressTracker.");
            }

            if (error) Log.Error("The LearningTrackerDef " + defName + " has errors.");

        }



    }
}
