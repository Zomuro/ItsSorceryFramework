﻿using Verse;
namespace ItsSorceryFramework
{
    public class Hediff_ProgressLevel : Hediff_Progress
	{
		public override string Label
		{
			get
			{
				if (Schema?.progressTracker != null)
				{
					if(Schema.progressTracker.CurLevelLabel.NullOrEmpty()) // no current level label from progress tracker?
                    {
						if (Level == (int)def.maxSeverity)
							return $"{def.label} (lvl. {Level}, MAX)";
						return $"{def.label} (lvl. {Level})";  //def.label + " " + "ISF_LevelLabel".Translate(Level.ToString());
					}

					else
					{
						if (Level == (int)def.maxSeverity)
							return $"{def.label} ({Schema.progressTracker.CurLevelLabel}, MAX)"; //def.label + " " + "ISF_LevelLabelCustomMax".Translate(schema.progressTracker.CurLevelLabel);
						return $"{def.label} ({Schema.progressTracker.CurLevelLabel})"; //def.label + " " + "ISF_LevelLabelCustom".Translate(schema.progressTracker.CurLevelLabel);
					}
				}
				return def.label + " x" + Level;
			}
		}

		public virtual int Level
        {
            get
            {
				return (int)Severity;
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
