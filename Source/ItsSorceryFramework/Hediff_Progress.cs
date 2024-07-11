using System.Collections.Generic;
using Verse;

namespace ItsSorceryFramework
{
    public class Hediff_Progress : HediffWithComps
	{
		//public ProgressTracker progressTracker;

		public SorcerySchema schema;

		public HediffStage cachedCurStage;

		public override string Label => def.label;

        public virtual SorcerySchema Schema
        {
            get
            {
                if (schema is null) ResetSchema(); // paranoid nullcheck and schema recovery
                return schema;
            }
            set
            {
                schema = value;
            }
        }

        public virtual void ResetSchema()
        {
            List<SorcerySchema> schemaList = SorcerySchemaUtility.GetSorcerySchemaList(pawn); // look through pawn's list of schemas
            if (schemaList.NullOrEmpty()) // no schemas -> this hediff shouldn't be here, as it is linked to a schema
            {
                pawn.health.RemoveHediff(this);
                return;
            }
            SorcerySchema tempSchema = schemaList.FirstOrDefault(x => x.def.progressTrackerDef.progressHediff == def); // find schema w/ this hediff's def
            if (tempSchema is null) // if the schema linked to this hediff doesn't exist, remove this hediff
            {
                pawn.health.RemoveHediff(this);
                return;
            }
            schema = tempSchema; // otherwise, update the hediff's linked schema
        }

        public override HediffStage CurStage
		{
			get
			{
				if (cachedCurStage == null) cachedCurStage = Schema?.progressTracker?.RefreshCurStage() ?? new HediffStage();
				return cachedCurStage;
			}
		}

		public override void Tick() => base.Tick();

		public override void PostAdd(DamageInfo? dinfo) => base.PostAdd(dinfo);

		public override bool ShouldRemove => Severity <= 0;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref schema, "schema");

			if (Scribe.mode == LoadSaveMode.PostLoadInit) // after loading stuff, get curstage
            {
                if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
                {
					Log.Message($"[It's Sorcery!] Hediff {def.defName} ProgressTracker null? {Schema?.progressTracker is null}" +
						$"\nProgressTracker offets: {Schema?.progressTracker.statOffsetsTotal.ToStringSafeEnumerable()}" +
						$"\nProgressTracker factors: {Schema?.progressTracker.statFactorsTotal.ToStringSafeEnumerable()}" +
						$"\nProgressTracker cap mods: {Schema?.progressTracker.capModsTotal.ToStringSafeEnumerable()}");
				}
                cachedCurStage = Schema?.progressTracker?.RefreshCurStage() ?? new HediffStage();
            }
        }
	}
}
