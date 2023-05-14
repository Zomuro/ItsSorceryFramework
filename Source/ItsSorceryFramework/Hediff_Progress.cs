using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class Hediff_Progress : HediffWithComps
	{
		public override string Label
		{
			get
			{
				return def.label;
			}
		}

		public override HediffStage CurStage
		{
			get
			{
				if (curStage == null)
				{
					return progressTracker?.RefreshCurStage() ?? new HediffStage();
				}
				return curStage;
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

		public override bool ShouldRemove
		{
			get
			{
				return Severity <= 0;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref progressTracker, "progressTracker", new object[] { pawn });
		}

		public ProgressTracker progressTracker;

		public HediffStage curStage;
	}
}
