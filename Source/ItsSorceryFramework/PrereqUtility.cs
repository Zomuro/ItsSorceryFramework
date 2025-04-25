using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public static class PrereqUtility
    {
        public static bool PrereqClassesFufilled(HashSet<ProgressTrackerClassDef> priorClassDefs, List<ProgressTrackerClassDef> prereqsClassDefs, 
            LearningNodePrereqMode prereqClassMode, int prereqClassModeMin)
        {
            switch (prereqClassMode)
            {
                case LearningNodePrereqMode.All:
                    // if even a single class def isn't in the history, return false
                    foreach (var prereqClassDef in prereqsClassDefs)
                        if (!priorClassDefs.Contains(prereqClassDef)) return false;

                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (var prereqClassDef in prereqsClassDefs)
                        if (priorClassDefs.Contains(prereqClassDef)) return false;

                    return false;

                case LearningNodePrereqMode.Min:
                    if (prereqClassModeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(prereqClassModeMin, prereqsClassDefs.Count());
                    foreach (var prereqClassDef in prereqsClassDefs)
                    {
                        if (priorClassDefs.Contains(prereqClassDef)) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public static bool PrereqNodeFufilled(LearningNodeRecord learningNodeRecord, List<LearningTreeNodeDef> prereqsNodes, LearningNodePrereqMode mode, int modeMin)
        {
            switch (mode)
            {
                case LearningNodePrereqMode.All:
                    foreach (LearningTreeNodeDef prereq in prereqsNodes)
                    {
                        if (!learningNodeRecord.completion[prereq]) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (LearningTreeNodeDef prereq in prereqsNodes)
                    {
                        if (learningNodeRecord.completion[prereq]) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (modeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(modeMin, prereqsNodes.Count());
                    foreach (LearningTreeNodeDef prereq in prereqsNodes)
                    {
                        if (learningNodeRecord.completion[prereq]) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public static bool PrereqResearchFufilled(List<ResearchProjectDef> prereqsResearchs, LearningNodePrereqMode mode, int modeMin)
        {
            switch (mode)
            {
                case LearningNodePrereqMode.All:
                    foreach (ResearchProjectDef prereq in prereqsResearchs)
                    {
                        if (!prereq.IsFinished) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (ResearchProjectDef prereq in prereqsResearchs)
                    {
                        if (prereq.IsFinished) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (modeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(modeMin, prereqsResearchs.Count());
                    foreach (ResearchProjectDef prereq in prereqsResearchs)
                    {
                        if (prereq.IsFinished) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public static bool PrereqXenotypeFufilled(Pawn pawn, XenotypeDef xenotypeDef)
        {
            // xenotypdef to check is null OR matching xenotype = true
            if (xenotypeDef == null || pawn.genes.Xenotype == xenotypeDef) return true;
            return false;
        }

        public static bool PrereqGenesFufilled(HashSet<GeneDef> pawnGenes, List<GeneDef> prereqsGenes, LearningNodePrereqMode mode, int modeMin)
        {
            switch (mode)
            {
                case LearningNodePrereqMode.All:
                    foreach (GeneDef prereq in prereqsGenes)
                    {
                        if (!pawnGenes.Contains(prereq)) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (GeneDef prereq in prereqsGenes)
                    {
                        if (pawnGenes.Contains(prereq)) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (modeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(modeMin, prereqsGenes.Count());
                    foreach (GeneDef prereq in prereqsGenes)
                    {
                        if (pawnGenes.Contains(prereq)) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public static bool PrereqTraitsFufilled(Pawn pawn, List<TraitRequirement> traitReqs, LearningNodePrereqMode mode, int modeMin)
        {           
            switch (mode)
            {
                case LearningNodePrereqMode.All:
                    foreach (var prereq in traitReqs)
                    {
                        if (!prereq.HasTrait(pawn)) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (var prereq in traitReqs)
                    {
                        if (prereq.HasTrait(pawn)) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (modeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(modeMin, traitReqs.Count());
                    foreach (var prereq in traitReqs)
                    {
                        if (prereq.HasTrait(pawn)) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public static TraitDegreeData GetTraitDegreeData(TraitDef traitDef, int? degree)
        {
            if (degree is null) return traitDef.degreeDatas[0];

            for (int i = 0; i < traitDef.degreeDatas.Count; i++)
            {
                if (traitDef.degreeDatas[i].degree == degree) return traitDef.degreeDatas[i];
            }
            return traitDef.degreeDatas[0];
        }

        public static bool PrereqAgeFufilled(Pawn pawn, int prereqAge, LearningNodeStatPrereqMode mode = LearningNodeStatPrereqMode.GreaterEqual, bool bioAge = true)
        {
            if (prereqAge <= 0) return true; // null case
            int finalAge = bioAge ? pawn.ageTracker.AgeBiologicalYears : pawn.ageTracker.AgeChronologicalYears;

            switch (mode)
            {
                case LearningNodeStatPrereqMode.Equal:
                    if (finalAge == prereqAge) return true;
                    break;

                case LearningNodeStatPrereqMode.NotEqual:
                    if (finalAge != prereqAge) return true;
                    break;

                case LearningNodeStatPrereqMode.Greater:
                    if (finalAge > prereqAge) return true;
                    break;

                case LearningNodeStatPrereqMode.GreaterEqual:
                    if (finalAge >= prereqAge) return true;
                    break;

                case LearningNodeStatPrereqMode.Lesser:
                    if (finalAge < prereqAge) return true;
                    break;

                case LearningNodeStatPrereqMode.LesserEqual:
                    if (finalAge <= prereqAge) return true;
                    break;

                default:
                    break;
            }

            return false;
        }

        public static bool PrereqLevelFufilled(ProgressTracker progressTracker, int prereqLevel, LearningNodeStatPrereqMode mode = LearningNodeStatPrereqMode.GreaterEqual)
        {
            if (prereqLevel <= 0) return true; // null case
            int currLevel = progressTracker.CurrLevel;

            switch (mode)
            {
                case LearningNodeStatPrereqMode.Equal:
                    if (currLevel == prereqLevel) return true;
                    break;

                case LearningNodeStatPrereqMode.NotEqual:
                    if (currLevel != prereqLevel) return true;
                    break;

                case LearningNodeStatPrereqMode.Greater:
                    if (currLevel > prereqLevel) return true;
                    break;

                case LearningNodeStatPrereqMode.GreaterEqual:
                    if (currLevel >= prereqLevel) return true;
                    break;

                case LearningNodeStatPrereqMode.Lesser:
                    if (currLevel < prereqLevel) return true;
                    break;

                case LearningNodeStatPrereqMode.LesserEqual:
                    if (currLevel <= prereqLevel) return true;
                    break;

                default:
                    break;
            }

            return false;
        }

        public static bool PrereqStatFufilled(Pawn pawn, List<NodeStatReqs> prereqsStats)
        {
            if (prereqsStats.NullOrEmpty()) return true;
            foreach (var statReqsCase in prereqsStats)
            {
                foreach (var statMod in statReqsCase.statReqs)
                {
                    if (PrereqFailStatCase(pawn, statMod, statReqsCase.mode)) return false;
                }
            }

            return true;
        }

        public static bool PrereqFailStatCase(Pawn pawn, StatModifier statMod, LearningNodeStatPrereqMode mode)
        {
            switch (mode)
            {
                case LearningNodeStatPrereqMode.Equal:
                    if (pawn.GetStatValue(statMod.stat) != statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.NotEqual:
                    if (pawn.GetStatValue(statMod.stat) == statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.Greater:
                    if (pawn.GetStatValue(statMod.stat) <= statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.GreaterEqual:
                    if (pawn.GetStatValue(statMod.stat) < statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.Lesser:
                    if (pawn.GetStatValue(statMod.stat) >= statMod.value) return true;
                    break;

                case LearningNodeStatPrereqMode.LesserEqual:
                    if (pawn.GetStatValue(statMod.stat) > statMod.value) return true;
                    break;

                default:
                    break;
            }

            return false;
        }

        public static bool PrereqSkillFufilled(Pawn pawn, List<NodeSkillReqs> prereqsSkills)
        {
            if (prereqsSkills.NullOrEmpty()) return true;
            foreach (var skillReqsCase in prereqsSkills)
            {
                foreach (var skillLevel in skillReqsCase.skillReqs)
                {
                    if (PrereqFailSkillCase(pawn, skillLevel.skillDef, skillLevel.ClampedLevel, skillReqsCase.mode)) return false;
                }
            }

            return true;
        }

        public static bool PrereqFailSkillCase(Pawn pawn, SkillDef skillDef, int level, LearningNodeStatPrereqMode mode)
        {
            switch (mode)
            {
                case LearningNodeStatPrereqMode.Equal:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() != level) return true;
                    break;

                case LearningNodeStatPrereqMode.NotEqual:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() == level) return true;
                    break;

                case LearningNodeStatPrereqMode.Greater:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() <= level) return true;
                    break;

                case LearningNodeStatPrereqMode.GreaterEqual:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() < level) return true;
                    break;

                case LearningNodeStatPrereqMode.Lesser:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() >= level) return true;
                    break;

                case LearningNodeStatPrereqMode.LesserEqual:
                    if (pawn.skills.GetSkill(skillDef).GetLevel() > level) return true;
                    break;

                default:
                    break;
            }

            return false;
        }

        public static bool PrereqHediffFufilled(Pawn pawn, Dictionary<HediffDef, float> prereqsHediff)
        {
            Hediff hediff;
            foreach (var pair in prereqsHediff)
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(pair.Key);
                if (hediff == null) return false;
                else if (hediff.Severity < pair.Value) return false;
            }

            return true;
        }

        public static string PrereqsModeNotif(LearningNodePrereqMode mode, int min = 0, int done = 0)
        {
            if (mode == LearningNodePrereqMode.Min && min > 0)
                return " (" + done + "/" + min + ")";
            if (mode == LearningNodePrereqMode.Or)
                return " (" + done + "/1)";
            return "";
        }

        public static string PrereqsStatsModeNotif(LearningNodeStatPrereqMode mode)
        {
            switch (mode)
            {
                case LearningNodeStatPrereqMode.Equal:
                    return " = ";

                case LearningNodeStatPrereqMode.NotEqual:
                    return " != ";

                case LearningNodeStatPrereqMode.Greater:
                    return " > ";

                case LearningNodeStatPrereqMode.GreaterEqual:
                    return " >= ";

                case LearningNodeStatPrereqMode.Lesser:
                    return " < ";

                case LearningNodeStatPrereqMode.LesserEqual:
                    return " <= ";

                default:
                    break;
            }
            return "";
        }

        public static int PrereqsDoneCount<T>(HashSet<T> compPrereqs, List<T> prereqList)
        {
            int prereqCount = 0;
            if (!prereqList.NullOrEmpty()) prereqCount = prereqList.Where(x => compPrereqs.Contains(x)).Count();
            return prereqCount;
        }

        public static void SetPrereqStatusColor(bool compCheck)
        {
            if (compCheck)
            {
                GUI.color = Color.green;
                return;
            }
            GUI.color = ColorLibrary.RedReadable;
        }

    }
}
