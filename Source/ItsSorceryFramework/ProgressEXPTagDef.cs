using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressEXPTagDef : Def
    {
		public Type workerClass = typeof(ProgressEXPWorker);

        public float expFactor = 1f;

        public float fixedEXP = 0f;

		public List<EXPJobSet> jobSets = new List<EXPJobSet>();

		public List<DamageDef> damageDefs = new List<DamageDef>();

		public List<SkillDef> skillDefs = new List<SkillDef>();

		public List<EXPConsumableItems> expItems = new List<EXPConsumableItems>();

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
