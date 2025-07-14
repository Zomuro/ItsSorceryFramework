using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ItsSorceryFramework
{
    public class FloatMenuOptionProvider_ProgressEXPWorkerUseItem : FloatMenuOptionProvider
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
                if (schema.progressTracker.currClassDef.Workers.EnumerableNullOrEmpty()) continue; // no energytrackers in the schema => don't bother

                foreach (var o in GetOptionsForProgressEXPWorkers(schema, clickedThing.Position)) 
                    yield return o;
            }

            yield break;
        }

        public IEnumerable<FloatMenuOption> GetOptionsForProgressEXPWorkers(SorcerySchema schema, IntVec3 vec)
        {
            String text;
            ProgressEXPWorker_UseItem itemWorker = schema.progressTracker.currClassDef.Workers.FirstOrDefault(x => x.GetType() == typeof(ProgressEXPWorker_UseItem)) as ProgressEXPWorker_UseItem;
            if (itemWorker == null || itemWorker.def.expItems.NullOrEmpty()) yield break;
            foreach (var item in itemWorker.def.expItems)
            {
                Thing EXPItem = vec.GetFirstThing(schema.pawn.Map, item.thingDef);
                if (EXPItem == null) continue;

                float factor = item.expFactorStat != null ? schema.pawn.GetStatValue(item.expFactorStat) : 1f;
                if (!schema.pawn.CanReach(EXPItem, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                {
                    text = "ISF_UseEXPItemNoPath".Translate() + item.gainEXPTransKey.Translate(item.thingDef.label, item.exp * factor, schema.def.LabelCap.ToString());
                    yield return new FloatMenuOption(text, null, MenuOptionPriority.Default,
                        null, null, 0f, null, null, true, 0);
                }

                else
                {
                    text = item.gainEXPTransKey.Translate(item.thingDef.label, item.exp * factor, schema.def.LabelCap.ToString());

                    Action chargeSchema = delegate ()
                    {
                        schema.pawn.jobs.TryTakeOrderedJob(JobGiver_GainEXP.MakeChargeEXPJob(schema.pawn, schema, EXPItem, 1),
                            new JobTag?(JobTag.Misc), false);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, chargeSchema,
                        MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), schema.pawn, EXPItem, "ReservedBy", null);
                }
            }

            yield break;
        }



    }
}
