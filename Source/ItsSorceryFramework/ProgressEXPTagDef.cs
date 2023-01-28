using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPTagDef : Def
    {
		/*public ProgressEXPWorker Worker
		{
			get
			{
				if (progressEXPWorker == null)
				{
					progressEXPWorker = (ProgressEXPWorker)Activator.CreateInstance(workerClass);
					progressEXPWorker.def = this;
				}
				return progressEXPWorker;
			}
		}*/

		public Type workerClass = typeof(ProgressEXPWorker);

        public float expFactor = 1f;

        public float fixedEXP = 0f;

		public List<EXPJobSet> jobSets = new List<EXPJobSet>();

		public List<DamageDef> damageDefs = new List<DamageDef>();

		public List<SkillDef> skillDefs = new List<SkillDef>();

		//public List<ThingDef> expItems = new List<ThingDef>();

		public List<EXPConsumableItems> expItems = new List<EXPConsumableItems>();

		//public ProgressEXPTagWorker progressEXPWorker;

    }

	public class EXPJobSet
	{
		public string label = "";

		public List<JobDef> jobDefs = new List<JobDef>();
	}

	public class EXPConsumableItems
    {
		public ThingDef thingDef;

		public float exp = 100f;

		public StatDef expFactorStat;

		public string gainEXPTransKey = "ISF_UseEXPItem";

	}
}
