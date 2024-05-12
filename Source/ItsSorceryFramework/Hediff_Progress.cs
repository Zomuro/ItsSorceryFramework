using Verse;

namespace ItsSorceryFramework
{
    public class Hediff_Progress : HediffWithComps
	{
		//public ProgressTracker progressTracker;

		public SorcerySchema schema;

		public HediffStage cachedCurStage;

		public override string Label => def.label;

		public override HediffStage CurStage
		{
			get
			{
				if (cachedCurStage == null) cachedCurStage = schema?.progressTracker?.RefreshCurStage() ?? new HediffStage();
				return cachedCurStage;
			}
		}

		public override void Tick()
		{
			base.Tick();
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
		}

		public override bool ShouldRemove => Severity <= 0;

		public override void ExposeData()
		{
			base.ExposeData();
			//Scribe_Deep.Look(ref progressTracker, "progressTracker", new object[] { pawn });

			//Scribe_Deep.Look(ref schema, "schema", new object[] { pawn });
			Scribe_References.Look(ref schema, "schema");

			if (Scribe.mode == LoadSaveMode.PostLoadInit) // after loading stuff, get cur stage
            {
                if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
                {
					Log.Message($"Hediff {def.defName} ProgressTracker null? {schema?.progressTracker is null}" +
						$"\nProgressTracker offets: {schema?.progressTracker.statOffsetsTotal.ToStringSafeEnumerable()}" +
						$"\nProgressTracker factors: {schema?.progressTracker.statFactorsTotal.ToStringSafeEnumerable()}" +
						$"\nProgressTracker cap mods: {schema?.progressTracker.capModsTotal.ToStringSafeEnumerable()}");
				}
                cachedCurStage = schema?.progressTracker?.RefreshCurStage() ?? new HediffStage();
            }
        }
	}
}
