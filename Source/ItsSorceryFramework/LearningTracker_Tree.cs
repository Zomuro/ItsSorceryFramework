using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ItsSorceryFramework
{
    public class LearningTracker_Tree : LearningTracker
    {
        public LearningTreeNodeDef selectedNode;

        private List<LearningTreeNodeDef> cachedAllNodes;

        private Vector2 cachedViewSize;

        //private ScrollPositioner scrollPositioner = new ScrollPositioner();

        private float leftStartAreaHeight = 68f;

        private float leftViewDebugHeight;

        private Vector2 leftScrollPosition = Vector2.zero;

        private float leftScrollViewHeight;

        private Vector2 rightScrollPosition = Vector2.zero;

        public LearningTracker_Tree(Pawn pawn) : base(pawn) { }

        public LearningTracker_Tree(Pawn pawn, LearningTrackerDef def, SorcerySchema schema) : base(pawn, def, schema) { }

        public List<LearningTreeNodeDef> AllRelativeNodes
        {
            get
            {
                if (cachedAllNodes == null)
                {
                    cachedAllNodes = new List<LearningTreeNodeDef>(from def in LearningRecord.AllNodes
                                                                   where def.learningTrackerDef == this.def
                                                                   select def);                    
                }

                return cachedAllNodes;
            }
        }

        public void RefreshRelativeNodes() => cachedAllNodes = null;

        public LearningNodeRecord LearningRecord => schema.learningNodeRecord;

        public Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> ExclusiveNodes => LearningRecord.ExclusiveNodes;

        public Vector2 ViewSize
        {
            get
            {
                if (cachedViewSize == null || cachedViewSize == Vector2.zero) cachedViewSize = FindViewSize();

                return cachedViewSize;
            }
        }

        public float PointUsePercent
        {
            get
            {
                ProgressTracker progress = schema.progressTracker;
                if (progress.points == 0) return 0;
                return (float) (progress.points - progress.usedPoints) / progress.points;
            }
        }

        public override void ExposeData() => base.ExposeData();

        public override void DrawLeftGUI(Rect rect)
        {
            float outRectHeight = rect.height - (10f + leftStartAreaHeight) - 45f;

            Widgets.BeginGroup(rect);
            if (this.selectedNode != null)
            {
                Rect outRect = new Rect(0f, 0f, rect.width, outRectHeight - leftViewDebugHeight);
                Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, leftScrollViewHeight);
                Widgets.BeginScrollView(outRect, ref this.leftScrollPosition, viewRect, true);
                
                float coordY = 0f;

                Text.Font = GameFont.Medium;
                GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
                Rect labelRect = new Rect(0f, coordY, viewRect.width, 50f);
                Widgets.LabelCacheHeight(ref labelRect, selectedNode.LabelCap, true, false);
                GenUI.ResetLabelAlign();
                Text.Font = GameFont.Small;
                coordY += labelRect.height;

                Rect descRect = new Rect(0f, coordY, viewRect.width, 0f);
                Widgets.LabelCacheHeight(ref descRect, selectedNode.description, true, false);
                coordY += descRect.height;

                Rect pointRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += DrawPointReq(selectedNode, pointRect);

                Rect prereqRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += DrawNodePrereqs(selectedNode, prereqRect);

                Rect exclusiveRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += DrawExclusive(selectedNode, exclusiveRect);

                Rect hyperlinkRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += DrawHyperlinks(hyperlinkRect, selectedNode);

                Rect statModRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += DrawStatMods(statModRect, selectedNode);

                Rect unlockRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += DrawUnlockedLearningTrackers(unlockRect, selectedNode);

                Rect contentRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += DrawContentSource(contentRect, selectedNode);
                coordY += 3f;
                leftScrollViewHeight = coordY;
                Widgets.EndScrollView();

                ProgressTracker progress = schema.progressTracker;
                Rect confirmButton = new Rect(0f, outRect.yMax + 10f + this.leftViewDebugHeight, rect.width, this.leftStartAreaHeight);
                string reason = "";
                if(!pawn.Faction.IsPlayer || pawn.Faction is null)
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.DrawHighlight(confirmButton);
                    reason = "ISF_LearningNodeNotPlayer".Translate(pawn.Name.ToStringShort);
                    Widgets.Label(confirmButton.ContractedBy(5f), reason);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if (!LearningRecord.completion[selectedNode] && LearningRecord.PrereqFufilled(selectedNode) && LearningRecord.PrereqResearchFufilled(selectedNode) &&
                    LearningRecord.PrereqStatFufilled(selectedNode) && LearningRecord.PrereqHediffFufilled(selectedNode) && LearningRecord.ExclusiveNodeFufilled(selectedNode) &&
                    LearningRecord.PrereqLevelFulfilled(selectedNode) && selectedNode.pointReq + progress.usedPoints <= progress.points) 
                {
                    if (Widgets.ButtonText(confirmButton, "ISF_SkillPointUse".Translate(selectedNode.pointReq, 
                        progress.def.skillPointLabelKey.Translate())))
                    {
                        ProgressDiffLog diffLog = schema.progressTracker.progressDiffLog;
                        ProgressDiffLedger progressDiffLedger = diffLog.PrepNewLedger(schema.progressTracker);
                        ProgressDiffClassLedger progressDiffClassLedger = new ProgressDiffClassLedger();

                        LearningRecord.completion[selectedNode] = true;
                        LearningRecord.CompletionAbilities(selectedNode, ref progressDiffClassLedger);
                        LearningRecord.CompletionHediffs(selectedNode, ref progressDiffClassLedger);
                        LearningRecord.CompletionModifiers(selectedNode, ref progressDiffClassLedger);
                        progressDiffLedger.classDiffLedgers[ISF_DefOf.ISF_Generic_Class] = progressDiffClassLedger;
                        diffLog.AddLedger(progressDiffLedger);

                        LearningRecord.CompletionLearningUnlock(selectedNode);
                        schema.progressTracker.usedPoints += selectedNode.pointReq;

                        foreach (var et in schema.energyTrackers) et.ClearStatCache();
                    }
                }
                else
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (LearningRecord.completion[selectedNode]) reason = "ISF_LearningNodeReasonCompleted".Translate();
                    else if (!LearningRecord.ExclusiveNodeFufilled(selectedNode)) reason = "ISF_LearningNodeReasonExclusive".Translate();
                    else
                    {
                        reason = "ISF_LearningNodeLocked".Translate();

                        if (selectedNode.pointReq + progress.usedPoints > progress.points) reason += "\n" +
                                "ISF_LearningNodeLockedPoints".Translate(schema.progressTracker.def.skillPointLabelKey.Translate());

                        if (!LearningRecord.PrereqFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedNodes".Translate();

                        if (!LearningRecord.PrereqResearchFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedResearch".Translate();

                        if (!LearningRecord.PrereqLevelFulfilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedLevel".Translate();

                        if (!LearningRecord.PrereqStatFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedStat".Translate();

                        if (!LearningRecord.PrereqSkillFufilled(selectedNode)) reason += "\n"+ "ISF_LearningNodeLockedSkill".Translate();

                        if (!LearningRecord.PrereqHediffFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedHediff".Translate();
                    }

                    this.leftStartAreaHeight = Mathf.Max(Text.CalcHeight(reason, confirmButton.width - 10f) + 10f, 68f);
                    Widgets.DrawHighlight(confirmButton);
                    Widgets.Label(confirmButton.ContractedBy(5f), reason);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                
                Rect pointBar = new Rect(0f, confirmButton.yMax + 10f, rect.width, 35f);
                Widgets.FillableBar(pointBar, PointUsePercent);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(pointBar, (progress.points-progress.usedPoints).ToString("F0") + " / " + schema.progressTracker.points.ToString("F0"));
                
                Text.Anchor = TextAnchor.UpperLeft;
                this.leftViewDebugHeight = 0f;
                if (Prefs.DevMode && !LearningRecord.completion[selectedNode])
                {
                    Text.Font = GameFont.Tiny;
                    Rect debugButton = new Rect(confirmButton.x, outRect.yMax, 120f, 30f);
                    if (Widgets.ButtonText(debugButton, "Debug: Finish now", true, true, true, null))
                    {
                        ProgressDiffLog diffLog = schema.progressTracker.progressDiffLog;
                        ProgressDiffLedger progressDiffLedger = diffLog.PrepNewLedger(schema.progressTracker);
                        ProgressDiffClassLedger progressDiffClassLedger = new ProgressDiffClassLedger();

                        LearningRecord.completion[selectedNode] = true;
                        LearningRecord.CompletionAbilities(selectedNode, ref progressDiffClassLedger);
                        LearningRecord.CompletionHediffs(selectedNode, ref progressDiffClassLedger);
                        LearningRecord.CompletionModifiers(selectedNode, ref progressDiffClassLedger);
                        progressDiffLedger.classDiffLedgers[ISF_DefOf.ISF_Generic_Class] = progressDiffClassLedger;
                        diffLog.AddLedger(progressDiffLedger);

                        LearningRecord.CompletionLearningUnlock(selectedNode);
                        
                        foreach (var et in schema.energyTrackers) et.ClearStatCache();
                    }
                    Text.Font = GameFont.Small;
                    this.leftViewDebugHeight = debugButton.height;
                }
                /* modify this for items like techprints required for skill system later
                if (Prefs.DevMode && !this.selectedProject.TechprintRequirementMet)
                {
                    Text.Font = GameFont.Tiny;
                    Rect rect14 = new Rect(rect11.x + 120f, outRect.yMax, 120f, 30f);
                    if (Widgets.ButtonText(rect14, "Debug: Apply techprint", true, true, true, null))
                    {
                        Find.ResearchManager.ApplyTechprint(this.selectedProject, null);
                        SoundDefOf.TechprintApplied.PlayOneShotOnCamera(null);
                    }
                    Text.Font = GameFont.Small;
                    this.leftViewDebugHeight = rect14.height;
                }*/
            }
            Widgets.EndGroup();

        }

        private float DrawPointReq(LearningTreeNodeDef node, Rect rect)
        {
            float yMin = rect.yMin;

            Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeCost".Translate(base.schema.progressTracker.def.skillPointLabelKey.Translate().CapitalizeFirst(), node.pointReq));
            rect.yMin += rect.height;
            return rect.yMin - yMin;
        }

        private float DrawNodePrereqs(LearningTreeNodeDef node, Rect rect)
        {
            if (node.prereqs.NullOrEmpty() && node.prereqsResearch.NullOrEmpty() && node.prereqLevel <= 0 && node.prereqsStats.NullOrEmpty() && 
                node.prereqsSkills.NullOrEmpty() && node.prereqsHediff.NullOrEmpty()) return 0f;
            float xMin = rect.xMin;
            float yMin = rect.yMin;

            Tuple<int, int> prereqsDone = LearningRecord.PrereqsDone(node);

            if (!node.prereqs.NullOrEmpty()) 
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodePrereqs".Translate() + LearningRecord.PrereqsModeNotif(node.prereqMode, node.prereqModeMin, prereqsDone.Item1), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (LearningTreeNodeDef prereq in node.prereqs)
                {
                    SetPrereqStatusColor(LearningRecord.completion[prereq], node);
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    if (Widgets.ButtonInvisible(rect, true))
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera(null);
                        if(node.learningTrackerDef != prereq.learningTrackerDef)
                        {
                            if(Find.WindowStack.currentlyDrawnWindow as Dialog_LearningTabs is Dialog_LearningTabs learningTabs && 
                                learningTabs != null && base.schema.def.learningTrackerDefs.Contains(prereq.learningTrackerDef))
                            {
                                learningTabs.curTracker = schema.learningTrackers.FirstOrDefault(x => x.def == prereq.learningTrackerDef);
                                if (learningTabs.curTracker as LearningTracker_Tree is LearningTracker_Tree treeTracker && treeTracker != null )
                                {
                                    treeTracker.selectedNode = prereq;
                                }
                            }
                        }
                        else 
                        {
                            selectedNode = prereq;
                        }
                        
                    }
                    rect.yMin += rect.height;
                }
                rect.xMin = xMin;
                GUI.color = Color.white;
            }

            if (!node.prereqsResearch.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeResearchPrereqs".Translate() + LearningRecord.PrereqsModeNotif(node.prereqResearchMode, node.prereqResearchModeMin, prereqsDone.Item2), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (ResearchProjectDef prereq in node.prereqsResearch)
                {
                    SetPrereqStatusColor(prereq.IsFinished, node);
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if(node.prereqLevel > 0)
            {
                SetPrereqStatusColor(!LearningRecord.PrereqLevelFulfilled(node), node);
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeLevelReq".Translate(node.prereqLevel), true, false);
                rect.yMin += rect.height;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!node.prereqsStats.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeStatReq".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsStatCase in node.prereqsStats)
                {
                    foreach(var statMod in prereqsStatCase.statReqs)
                    {
                        SetPrereqStatusColor(!LearningRecord.PrereqFailStatCase(statMod, prereqsStatCase.mode), node);
                        Widgets.LabelCacheHeight(ref rect, statMod.stat.LabelCap + LearningRecord.PrereqsStatsModeNotif(prereqsStatCase.mode) +
                            statMod.stat.ValueToString(statMod.value, ToStringNumberSense.Absolute, !statMod.stat.formatString.NullOrEmpty()), true, false);
                        rect.yMin += rect.height;
                    }
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!node.prereqsSkills.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeSkillReq".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsSkillCase in node.prereqsSkills)
                {
                    foreach (var skillLevel in prereqsSkillCase.skillReqs)
                    {
                        SetPrereqStatusColor(!LearningRecord.PrereqFailSkillCase(skillLevel.skillDef, skillLevel.ClampedLevel, prereqsSkillCase.mode), node);
                        Widgets.LabelCacheHeight(ref rect, skillLevel.skillDef.LabelCap + LearningRecord.PrereqsStatsModeNotif(prereqsSkillCase.mode) +
                            skillLevel.ClampedLevel, true, false);
                        rect.yMin += rect.height;
                    }
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!node.prereqsHediff.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeHediffReq".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                Hediff hediff;
                String reqLabel;
                foreach (var prereq in node.prereqsHediff)
                {
                    hediff = pawn.health.hediffSet.GetFirstHediffOfDef(prereq.Key);
                    SetPrereqStatusColor((hediff != null && hediff.Severity >= prereq.Value), node);
                    reqLabel = !prereq.Key.stages.NullOrEmpty() ? 
                        prereq.Key.stages[prereq.Key.StageAtSeverity(prereq.Value)].label : prereq.Value.ToString("F0");
                    Widgets.LabelCacheHeight(ref rect, prereq.Key.LabelCap + " ({0})".Translate(reqLabel), true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            return rect.yMin - yMin;
        }

        private float DrawExclusive(LearningTreeNodeDef node, Rect rect)
        {
            if (ExclusiveNodes[node].NullOrEmpty()) return 0;

            float xMin = rect.xMin;
            float yMin = rect.yMin;

            Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeExclusive".Translate(), true, false);
            rect.yMin += rect.height;
            rect.xMin += 6f;
            foreach (LearningTreeNodeDef ex in ExclusiveNodes[node])
            {
                if(LearningRecord.completion[ex]) GUI.color = ColorLibrary.RedReadable;

                Widgets.LabelCacheHeight(ref rect, ex.LabelCap, true, false);
                rect.yMin += rect.height;
            }
            GUI.color = Color.white;
            rect.xMin = xMin;

            return rect.yMin - yMin;
        }

        private float DrawHyperlinks(Rect rect, LearningTreeNodeDef node)
        {
            List<AbilityDef> abilityGain = node.abilityGain;
            List<AbilityDef> abilityRemove = node.abilityRemove;
            List<NodeHediffProps> hediffAdd = node.hediffAdd;
            List<NodeHediffProps> hediffAdjust = node.hediffAdjust;
            List<HediffDef> hediffRemove = node.hediffRemove;

            if (abilityGain.NullOrEmpty() && abilityRemove.NullOrEmpty() && hediffAdd.NullOrEmpty() && hediffAdjust.NullOrEmpty() &&
                hediffRemove.NullOrEmpty())
            {
                return 0f;
            }

            float yMin = rect.yMin;
            float x = rect.x;
            Dialog_InfoCard.Hyperlink hyperlink;

            if (!abilityGain.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeAbilityGain".Translate(), true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (AbilityDef abilityDef in abilityGain)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(abilityDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!abilityRemove.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeAbilityRemove".Translate(), true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (AbilityDef abilityDef in abilityRemove)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(abilityDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!hediffAdd.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeHediffAdd".Translate(), true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (NodeHediffProps prop in hediffAdd)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    HediffDef hediffDef = prop.hediffDef;
                    string sev;

                    sev = hediffDef.stages.NullOrEmpty() ? prop.severity.ToStringWithSign("F0") :
                        hediffDef.stages[hediffDef.StageAtSeverity(prop.severity)].label;
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, hediffDef.LabelCap + " ({0})".Translate(sev),
                        2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;

                }
                rect.x = x;
            }

            if (!hediffAdjust.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeHediffAdjust".Translate(), true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (NodeHediffProps prop in hediffAdjust)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    HediffDef hediffDef = prop.hediffDef;
                    string sev;

                    sev = prop.severity.ToStringWithSign("F0");
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, hediffDef.LabelCap + " ({0})".Translate(sev),
                        2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!hediffRemove.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeHediffRemove".Translate(), true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (HediffDef hediffDef in hediffRemove)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            return rect.yMin - yMin;
        }

        private float DrawStatMods(Rect rect, LearningTreeNodeDef node)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            String tipString = TipStringExtra(node);
            if (!tipString.NullOrEmpty()) 
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeMods".Translate(), true, false);
                rect.yMin += rect.height;
                Widgets.LabelCacheHeight(ref rect, tipString, true, false);
                rect.yMin += rect.height;
            }

            return rect.yMin - yMin;
        }

        public string TipStringExtra(LearningTreeNodeDef node)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StatDrawEntry statDrawEntry in node.specialDisplayMods()){
                if (statDrawEntry.ShouldDisplay())
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            return stringBuilder.ToString();
        }

        private float DrawUnlockedLearningTrackers(Rect rect, LearningTreeNodeDef node)
        {
            float yMin = rect.yMin;
            float xMin = rect.xMin;

            if (!node.unlocks.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeUnlocks".Translate(), true, false); // label
                rect.yMin += rect.height;
                rect.xMin += 6f;

                foreach (var lt in node.unlocks) // displays learningtrackerdefs that will be unlocked, if not
                {
                    if (!schema.def.learningTrackerDefs.Contains(lt)) continue;

                    Widgets.LabelCacheHeight(ref rect, lt.LabelCap, true, false);
                    if (Widgets.ButtonInvisible(rect, true))
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera(null);
                        if (node.learningTrackerDef != lt)
                        {
                            if (Find.WindowStack.currentlyDrawnWindow as Dialog_LearningTabs is Dialog_LearningTabs learningTabs &&
                                learningTabs != null && base.schema.def.learningTrackerDefs.Contains(lt))
                            {
                                learningTabs.curTracker = schema.learningTrackers.FirstOrDefault(x => x.def == lt);
                                if (learningTabs.curTracker as LearningTracker_Tree is LearningTracker_Tree treeTracker && treeTracker != null)
                                {
                                    treeTracker.selectedNode = null;
                                }
                            }
                        }

                    }
                    rect.yMin += rect.height;
                }
                rect.xMin = xMin;
            }

            return rect.yMin - yMin;
        }

        private float DrawContentSource(Rect rect, LearningTreeNodeDef node) // taken from research tab
        {
            if (node.modContentPack == null || node.modContentPack.IsCoreMod)
            {
                return 0f;
            }
            float yMin = rect.yMin;
            TaggedString taggedString = "Stat_Source_Label".Translate() + ":  " + node.modContentPack.Name;
            Widgets.LabelCacheHeight(ref rect, taggedString.Colorize(Color.grey), true, false);
            ExpansionDef expansionDef = ModLister.AllExpansions.Find((ExpansionDef e) => e.linkedMod == node.modContentPack.PackageId);
            if (expansionDef != null)
            {
                GUI.DrawTexture(new Rect(Text.CalcSize(taggedString).x + 4f, rect.y, 20f, 20f), expansionDef.IconFromStatus);
            }
            return rect.yMax - yMin;
        }

        private void SetPrereqStatusColor(bool compCheck, LearningTreeNodeDef node)
        {
            if (LearningRecord.completion[node])
            {
                return;
            }
            if (compCheck)
            {
                GUI.color = Color.green;
                return;
            }
            GUI.color = ColorLibrary.RedReadable;
        }

        public override void DrawRightGUI(Rect rect)
        {
            if (locked)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Medium;

                Rect lockedRect = new Rect(rect.x, rect.y + rect.height / 3f, rect.width, rect.height);

                Widgets.LabelCacheHeight(ref lockedRect, def.lockedLabel);
                lockedRect.yMin += lockedRect.height;

                Text.Font = GameFont.Small;
                Widgets.LabelCacheHeight(ref lockedRect, def.unlockTip);
                lockedRect.yMin += lockedRect.height;

                if (Prefs.DevMode && Widgets.ButtonText(new Rect(rect.x + rect.width / 2 - 75f, lockedRect.y, 150f, 50f), "Dev mode: unlock"))
                {
                    locked = false;
                }

                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }
            
            rect.yMin += 32f;

            Rect outRect = rect.ContractedBy(10f);
            //Rect viewRect = outRect.ContractedBy(10f);
            //Rect groupRect = viewRect.ContractedBy(10f);

            Rect viewRect = new Rect(0f, 0f, ViewSize.x, ViewSize.y);
            viewRect.ContractedBy(10f);
            viewRect.width = ViewSize.x;
            Rect groupRect = viewRect.ContractedBy(10f);

            //this.scrollPositioner.ClearInterestRects();
            Widgets.BeginScrollView(outRect, ref this.rightScrollPosition, viewRect, true);
            Widgets.ScrollHorizontal(outRect, ref this.rightScrollPosition, viewRect, 20f);
            Widgets.BeginGroup(groupRect);

            // first pass- draw the lines for the node requirements
            Rect nodeRect;
            foreach (LearningTreeNodeDef node in AllRelativeNodes)
            {
                if (!LearningRecord.PrereqFufilled(node) && node.condVisiblePrereq) continue;

                nodeRect = GetNodeRect(node);
                foreach (LearningTreeNodeDef prereq in node.prereqs)
                {
                    if (prereq.learningTrackerDef != node.learningTrackerDef) continue;
                    Tuple<Vector2, Vector2> points = LineEnds(prereq, node, nodeRect);
                    Tuple<Color, float> lineColor = SelectionLineColor(node, prereq);
                    //Widgets.DrawLine(points.Item1, points.Item2, selectionLineColor(node), 2f);
                    Widgets.DrawLine(points.Item1, points.Item2, lineColor.Item1, lineColor.Item2);
                }               
            }

            // second pass- draw the nodes + label
            foreach(LearningTreeNodeDef node in AllRelativeNodes)
            {
                if (!LearningRecord.PrereqFufilled(node) && node.condVisiblePrereq) continue;

                nodeRect = GetNodeRect(node);

                if (Widgets.CustomButtonText(ref nodeRect, "", SelectionBGColor(node),
                    new Color(0.8f, 0.85f, 1f), SelectionBorderColor(node), default, false, 1, true, true))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera(null);
                    this.selectedNode = node;
                }

                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.LabelCacheHeight(ref nodeRect, node.LabelCap, true, false);
                Text.Anchor = TextAnchor.UpperLeft;

                if (Mouse.IsOver(nodeRect))
                {
                    Widgets.DrawLightHighlight(nodeRect);
                    TooltipHandler.TipRegion(nodeRect, node.GetTip());
                }
            }

			Widgets.EndGroup();
            Widgets.EndScrollView();

        }

        private float CoordToPixelsX(float x)
        {
            return x * 190f;
        }

        private float CoordToPixelsY(float y)
        {
            return y * 100f;
        }

        private Rect GetNodeRect(LearningTreeNodeDef nodeDef)
        {
            return new Rect(CoordToPixelsX(nodeDef.coordX), CoordToPixelsY(nodeDef.coordY), 140f, 50f);
        }

        private Tuple<Vector2, Vector2> LineEnds(LearningTreeNodeDef start, LearningTreeNodeDef end, Rect nodeRef)
        {
            Vector2 prereq = new Vector2(CoordToPixelsX(start.coordX), CoordToPixelsY(start.coordY));
            prereq.x += nodeRef.width;
            prereq.y += nodeRef.height/2;
            Vector2 current = new Vector2(CoordToPixelsX(end.coordX), CoordToPixelsY(end.coordY));
            current.y += nodeRef.height/2;

            return new Tuple<Vector2, Vector2>(prereq, current);
        }

        private Vector2 FindViewSize()
        {
            float x = 0f;
            float y = 0f;
            foreach (LearningTreeNodeDef node in AllRelativeNodes)
            { 
                x = Mathf.Max(x, this.CoordToPixelsX(node.coordX) + 140f);
                y = Mathf.Max(y, this.CoordToPixelsY(node.coordY) + 50f);
            }

            return new Vector2(x + 32f, y + 32f);
        }

        private Color SelectionBGColor(LearningTreeNodeDef node)
        {
            Color baseCol = default(Color);

            //Color baseCol2 = TexUI.AvailResearchColor;

            if (LearningRecord.completion[node]) baseCol = TexUI.FinishedResearchColor;

            else if (!LearningRecord.ExclusiveNodeFufilled(node)) baseCol = ColorLibrary.BrickRed;

            else if (LearningRecord.PrereqFufilled(node) && LearningRecord.PrereqResearchFufilled(node)) baseCol = TexUI.AvailResearchColor;

            else baseCol = TexUI.LockedResearchColor;

            // if the node is the selected one, change background to highlight
            if (selectedNode != null && selectedNode == node) return baseCol + TexUI.HighlightBgResearchColor;

            return baseCol;
        }

        private Color SelectionBorderColor(LearningTreeNodeDef node)
        {
            // if the node is the selected one OR a prerequisite, change border to highlight
            if (selectedNode != null)
            {
                if (selectedNode == node) return TexUI.HighlightBorderResearchColor;

                else if (selectedNode.prereqs.NotNullAndContains(node))
                {
                    if(!LearningRecord.completion[node]) return TexUI.DependencyOutlineResearchColor;
                    else return TexUI.HighlightLineResearchColor;
                }

            }

            return TexUI.DefaultBorderResearchColor;
        }

        private Tuple<Color,float> SelectionLineColor(LearningTreeNodeDef node, LearningTreeNodeDef prereq)
        {
            //Color col = default(Color);
            if (selectedNode == node)
            {
                if (LearningRecord.completion[prereq])
                {
                    return new Tuple<Color, float>(TexUI.HighlightLineResearchColor, 3f);
                }

                else 
                {
                    return new Tuple<Color, float>(TexUI.DependencyOutlineResearchColor, 3f);
                } 
            }   
            return new Tuple<Color, float>(TexUI.DefaultLineResearchColor, 2f);
        }
        
    }
}
