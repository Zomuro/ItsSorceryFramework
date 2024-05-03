using RimWorld;
using Verse;
namespace ItsSorceryFramework
{
    public class Hediff_ProgressLevel : Hediff_Progress
	{
		public override string Label
		{
			get
			{
				if (progressTracker != null)
				{
					if(progressTracker.CurLevelLabel.NullOrEmpty())
                    {
						if (level == (int)def.maxSeverity)
							return def.label + " " + "ISF_LevelLabelMax".Translate(level.ToString());
						return def.label + " " + "ISF_LevelLabel".Translate(level.ToString());
					}

					else
					{
						if (level == (int)def.maxSeverity)
							return def.label + " " + "ISF_LevelLabelCustomMax".Translate(progressTracker.CurLevelLabel);
						return def.label + " " + "ISF_LevelLabelCustom".Translate(progressTracker.CurLevelLabel);
					}
				}
				return def.label + " x" + this.level;
			}
		}

		public virtual int level
        {
            get
            {
				return (int) Severity;
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
			
		}

		public string temp = " (level {0} | {1})";
	}
}
