using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class FloatMenuOptionProvider_EnergyTrackerCompOnConsume : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        protected override bool RequiresManipulation => true;

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        {
            if (context.FirstSelectedPawn is null) yield break;
            
            List<SorcerySchema> schemas = SorcerySchemaUtility.GetSorcerySchemaList(context.FirstSelectedPawn);
            if (schemas.NullOrEmpty()) yield break;

            foreach (SorcerySchema schema in schemas)
            {
                if (schema.energyTrackers.NullOrEmpty()) continue; // no energytrackers in the schema => don't bother

                foreach (var o in GetOptionsForEnergyTrackers(schema, clickedThing.Position.ToVector3())) 
                    yield return o;
            }

            yield break;
        }

        public IEnumerable<FloatMenuOption> GetOptionsForEnergyTrackers(SorcerySchema schema, Vector3 vec)
        {
            foreach (var energyTracker in schema.energyTrackers)
            {
                if (energyTracker.comps.NullOrEmpty()) continue; // if the energytracker has no comps, skip check

                foreach (var c in energyTracker.comps)
                {
                    foreach (var o in c.CompPostConsume(vec)) yield return o;
                }
            }

            yield break;
        }



    }
}
