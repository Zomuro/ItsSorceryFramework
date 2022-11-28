using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningTreeNodeDef : Def
    {
		public float ViewX
		{
			get
			{
				return this.coordX;
			}
		}

		public float ViewY
		{
			get
			{
				return this.coordY;
			}
		}

		public List<LearningTreeNodeDef> prereqs = new List<LearningTreeNodeDef>();

		public List<ResearchProjectDef> prereqsResearch = new List<ResearchProjectDef>();

		public LearningTrackerDef learningTracker;

		public float coordX = 0;

		public float coordY = 0;

		public int level;

		public bool condVisiblePrereq = true;

		List<LearningTreeNodeDef> exclusiveNodes = new List<LearningTreeNodeDef>();

		//public AbilityDef nodeAbilityGain;

		public List<AbilityDef> abilityGain = new List<AbilityDef>();

		public List<AbilityDef> abilityRemove = new List<AbilityDef>();

		//public AbilityDef nodeAbilityRemove;

		public HediffDef nodeHediff;

		public Dictionary<HediffDef, float> hediffAdjust = new Dictionary<HediffDef, float>();

		public List<HediffDef> hediffRemove = new List<HediffDef>();

		public float nodeHediffSev;

	}
}
