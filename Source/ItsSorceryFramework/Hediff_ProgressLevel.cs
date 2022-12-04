using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class Hediff_ProgressLevel : Hediff_Level
    {
		public override string Label
		{
			get
			{
				if (!this.def.levelIsQuantity)
				{
					return this.def.label + " (" + "LevelNum".Translate(this.level).ToString() + ")";
				}
				return this.def.label + " x" + this.level;
			}
		}

		public override void Tick()
		{
			/*base.Tick();
			this.Severity = (float)this.level;*/
		}

		public override bool ShouldRemove
		{
			get
			{
				return this.level <= 0;
			}
		}
    }
}
