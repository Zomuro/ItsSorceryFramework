using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPDef : Def
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

        public WorkTypeDef workTypeDef;

		public WorkGiverDef workGiverDef;


		//public List<JobDef> jobDefs = new List<JobDef>();

		//public List<WorkTypeDef> workTypeDefs = new List<WorkTypeDef>();

		public List<DamageDef> damageDefs = new List<DamageDef>();

		public List<SkillDef> skillDefs = new List<SkillDef>();

		public List<ThingDef> expItems = new List<ThingDef>();

        public ProgressEXPWorker progressEXPWorker;

    }
}
