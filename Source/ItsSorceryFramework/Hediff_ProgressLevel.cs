using RimWorld;
using Verse;
namespace ItsSorceryFramework
{
    public class Hediff_ProgressLevel : HediffWithComps
	{
		public override string Label
		{
			get
			{
				if (progressTracker != null)
				{
					if(progressTracker.CurLevelLabel.NullOrEmpty()) 
						return def.label + " "+ "ISF_LevelLabel".Translate(level.ToString(), progressTracker.CurrProgress.ToString("P2"));

					return def.label + " " + "ISF_LevelLabelCustom".Translate(progressTracker.CurLevelLabel, progressTracker.CurrProgress.ToString("P2"));
				}
				return def.label + " x" + this.level;
			}
		}

		public virtual int level
        {
            get
            {
				return (int) this.Severity;
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
				return this.curStage;
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
				return this.Severity <= 0;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref progressTracker, "progressTracker", new object[] { pawn });
		}

		public ProgressTracker progressTracker;

		public HediffStage curStage;

		public string temp = " (level {0} | {1})";
	}
}
