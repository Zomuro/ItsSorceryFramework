using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

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
					return this.def.label + temp.Translate(level.ToString(), progressTracker.currProgress.ToString("P2"));
				}
				return this.def.label + " x" + this.level;
			}
		}

		public virtual int level
        {
            get
            {
				return (int) this.Severity;
            }
        }

		/*public override HediffStage CurStage
		{
			get
			{
				bool flag = this.curStage == null;
				if (flag)
				{
					this.RecacheCurStage();
				}
				return this.curStage;
			}
		}*/

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
