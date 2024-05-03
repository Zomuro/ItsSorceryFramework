using Verse;

namespace ItsSorceryFramework
{
    public class Hediff_Progress : HediffWithComps
	{
		public ProgressTracker progressTracker;

		public HediffStage cachedCurStage;

		public override string Label => def.label;

		public override HediffStage CurStage
		{
			get
			{
				if (cachedCurStage == null) cachedCurStage = progressTracker?.RefreshCurStage() ?? new HediffStage();
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
			Scribe_Deep.Look(ref progressTracker, "progressTracker", new object[] { pawn });

            if (Scribe.mode == LoadSaveMode.LoadingVars) // after loading stuff, get cur stage
            {
                if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
                {
					Log.Message($"Hediff {def.defName} ProgressTracker null? {progressTracker is null}" +
						$"\nProgressTracker offets: {progressTracker.statOffsetsTotal.ToStringSafeEnumerable()}" +
						$"\nProgressTracker factors: {progressTracker.statFactorsTotal.ToStringSafeEnumerable()}" +
						$"\nProgressTracker cap mods: {progressTracker.capModsTotal.ToStringSafeEnumerable()}");
				}
                cachedCurStage = progressTracker?.RefreshCurStage() ?? new HediffStage();
            }
        }
	}
}
