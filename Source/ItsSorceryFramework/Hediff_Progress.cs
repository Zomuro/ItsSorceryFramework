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
		}
	}
}
