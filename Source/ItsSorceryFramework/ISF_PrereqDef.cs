using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public abstract class ISF_PrereqDef : Def
    {
        // common prereq fields used in node and progress tracker class defs

        public LearningNodePrereqMode prereqNodeMode = LearningNodePrereqMode.All;

        public int prereqNodeModeMin = 1;

        public LearningNodePrereqMode prereqResearchMode = LearningNodePrereqMode.All;

        public int prereqResearchModeMin = 1;

        [MayRequireBiotech]
        public LearningNodePrereqMode prereqGeneMode = LearningNodePrereqMode.All;

        [MayRequireBiotech]
        public int prereqGeneModeMin = 1;

        public LearningNodePrereqMode prereqTraitMode = LearningNodePrereqMode.All;

        public int prereqTraitModeMin = 1;

        public LearningNodeStatPrereqMode prereqLevelMode = LearningNodeStatPrereqMode.GreaterEqual;

        public int prereqLevel = 0;

        public LearningNodeStatPrereqMode prereqAgeMode = LearningNodeStatPrereqMode.GreaterEqual;

        public int prereqAge = 0;

        public bool prereqCheckBioAge = false;

        [MayRequireBiotech]
        public XenotypeDef prereqXenotype = null;

        public List<LearningTreeNodeDef> prereqNodes = new List<LearningTreeNodeDef>();

        public List<ResearchProjectDef> prereqResearch = new List<ResearchProjectDef>();

        [MayRequireBiotech]
        public List<GeneDef> prereqGenes = new List<GeneDef>();

        [XmlInheritanceAllowDuplicateNodes]
        public List<TraitRequirement> prereqTraits = new List<TraitRequirement>();

        public Dictionary<HediffDef, float> prereqHediffs = new Dictionary<HediffDef, float>();

        public List<NodeStatReqs> prereqStats = new List<NodeStatReqs>();

        public List<NodeSkillReqs> prereqSkills = new List<NodeSkillReqs>();
    }

    public class NodeStatReqs
    {
        public List<StatModifier> statReqs;

        public LearningNodeStatPrereqMode mode = LearningNodeStatPrereqMode.Greater;
    }

    public class NodeSkillReqs
    {
        public List<SkillLevel> skillReqs;

        public LearningNodeStatPrereqMode mode = LearningNodeStatPrereqMode.Greater;
    }

    public class SkillLevel
    {
        public SkillDef skillDef;

        public int level = 1;

        public int ClampedLevel
        {
            get
            {
                return Mathf.Clamp(level, 1, 20);
            }
        }

    }
}
