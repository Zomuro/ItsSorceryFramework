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
        public LearningTracker_Tree(Pawn pawn) : base(pawn)
        {

        }

        public LearningTracker_Tree(Pawn pawn, LearningTrackerDef def) : base(pawn, def)
        {

        }

        public LearningTracker_Tree(Pawn pawn, LearningTrackerDef def, SorcerySchemaDef schemaDef) : base(pawn, def, schemaDef)
        {

        }

        public List<LearningTreeNodeDef> AllRelativeNodes
        {
            /// <summary>
            /// Gets all relative nodes; learning nodes only relevant to the currently viewed LearningTracker_Tree.
            /// </summary>

            get
            {
                if (cachedAllNodes == null)
                {
                    /*cachedAllNodes = new List<LearningTreeNodeDef>(from def in DefDatabase<LearningTreeNodeDef>.AllDefsListForReading
                                                                   where def.learningTrackerDef == this.def
                                                                   select def);*/

                    cachedAllNodes = new List<LearningTreeNodeDef>(from def in Schema.learningNodeRecord.AllNodes
                                                                   where def.learningTrackerDef == this.def
                                                                   select def);

                    /*foreach (LearningTreeNodeDef node in cachedAllNodes)
                    {
                        if (!Schema.nodeTracker.completion.Keys.Contains(node)) Schema.nodeTracker.completion[node] = false;
                    }*/

                    
                }

                return cachedAllNodes;
            }
        }

        public void RefreshRelativeNodes()
        {
            /// <summary>
            /// Empties cached relative nodes, allowing the displayed nodes to refresh properly.
            /// </summary>

            cachedAllNodes = null;
        }

        public Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> ExclusiveNodes 
        {
            /// <summary>
            /// Creates a list of learning nodes that are incompatible with each other, so that only one of the nodes within a pairing can be completed.
            /// </summary>

            get
            {
                if(cacheExclusive == null)
                {
                    Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> exclusive = new Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>>();
                    foreach(LearningTreeNodeDef node in AllRelativeNodes)
                    {
                        if (!exclusive.ContainsKey(node)) exclusive[node] = node.exclusiveNodes.Distinct().ToList();

                        foreach(LearningTreeNodeDef conflict in node.exclusiveNodes)
                        {
                            if (!exclusive.ContainsKey(conflict)) exclusive[conflict] = new List<LearningTreeNodeDef>() { node };
                            else exclusive[conflict].AddDistinct(node);
                        }
                    }

                    cacheExclusive = exclusive;
                }

                return cacheExclusive;
            }           
        }

        public void RefreshExclusiveNodes()
        {
            /// <summary>
            /// Empties cached exclusive nodes, allowing the system to recalculate them.
            /// </summary>

            cacheExclusive = null;
        }

        public Vector2 ViewSize
        {
            get
            {
                if (cachedViewSize == null || cachedViewSize == Vector2.zero) cachedViewSize = FindViewSize();

                return cachedViewSize;
            }
        }

        public SorcerySchema Schema
        {
            get
            {
                if (cachedSchema == null) cachedSchema = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef);

                return cachedSchema;
            }
        }

        public float PointUsePercent
        {
            get
            {
                ProgressTracker progress = Schema.progressTracker;
                if (progress.points == 0) return 0;
                return (float) (progress.points - progress.usedPoints) / progress.points;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value);

        }

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

                Rect contentRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += DrawContentSource(contentRect, selectedNode);
                coordY += 3f;
                leftScrollViewHeight = coordY;
                Widgets.EndScrollView();

                ProgressTracker progress = Schema.progressTracker;
                Rect confirmButton = new Rect(0f, outRect.yMax + 10f + this.leftViewDebugHeight, rect.width, this.leftStartAreaHeight);
                string reason = "";
                if (!Schema.learningNodeRecord.Schema.learningNodeRecord.completion[selectedNode] && PrereqFufilled(selectedNode) && PrereqResearchFufilled(selectedNode) &&
                    PrereqStatFufilled(selectedNode) && PrereqHediffFufilled(selectedNode) && ExclusiveNodeFufilled(selectedNode) &&
                    selectedNode.pointReq + progress.usedPoints <= progress.points) 
                {
                    if (Widgets.ButtonText(confirmButton, "ISF_SkillPointUse".Translate(selectedNode.pointReq, 
                        progress.def.skillPointLabelKey.Translate())))
                    {
                        Schema.learningNodeRecord.completion[selectedNode] = true;
                        CompletionAbilities(selectedNode);
                        CompletionHediffs(selectedNode);
                        CompletionModifiers(selectedNode);
                        Schema.progressTracker.usedPoints += selectedNode.pointReq;
                    }
                }
                else
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (Schema.learningNodeRecord.completion[selectedNode]) reason = "Completed.";
                    else if (!ExclusiveNodeFufilled(selectedNode)) reason = "Conflicts with another node.";
                    else
                    {
                        reason = "Locked:";

                        if (selectedNode.pointReq + progress.usedPoints > progress.points) reason += "\nNot enough "+ 
                                schemaDef.progressTrackerDef.skillPointLabelKey.Translate() +".";

                        if (!PrereqFufilled(selectedNode)) reason += "\nPrior nodes not completed.";

                        if (!PrereqResearchFufilled(selectedNode)) reason += "\nResearch requirements not completed.";

                        if(!PrereqStatFufilled(selectedNode)) reason += "\nStat requirements not fufilled.";

                        if (!PrereqSkillFufilled(selectedNode)) reason += "\nSkill requirements not fufilled.";

                        if (!PrereqHediffFufilled(selectedNode)) reason += "\nHediff requirements not met.";
                    }

                    this.leftStartAreaHeight = Mathf.Max(Text.CalcHeight(reason, confirmButton.width - 10f) + 10f, 68f);
                    Widgets.DrawHighlight(confirmButton);
                    Widgets.Label(confirmButton.ContractedBy(5f), reason);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                
                Rect pointBar = new Rect(0f, confirmButton.yMax + 10f, rect.width, 35f);
                Widgets.FillableBar(pointBar, PointUsePercent);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(pointBar, (progress.points-progress.usedPoints).ToString("F0") + " / " + Schema.progressTracker.points.ToString("F0"));
                
                Text.Anchor = TextAnchor.UpperLeft;
                this.leftViewDebugHeight = 0f;
                if (Prefs.DevMode && !Schema.learningNodeRecord.completion[selectedNode])
                {
                    Text.Font = GameFont.Tiny;
                    Rect debugButton = new Rect(confirmButton.x, outRect.yMax, 120f, 30f);
                    if (Widgets.ButtonText(debugButton, "Debug: Finish now", true, true, true, null))
                    {
                        Schema.learningNodeRecord.Schema.learningNodeRecord.completion[selectedNode] = true;
                        CompletionAbilities(selectedNode);
                        CompletionHediffs(selectedNode);
                        CompletionModifiers(selectedNode);
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

            Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeCost".Translate(schemaDef.progressTrackerDef.skillPointLabelKey.Translate().CapitalizeFirst(), node.pointReq));
            rect.yMin += rect.height;
            return rect.yMin - yMin;
        }

        private float DrawNodePrereqs(LearningTreeNodeDef node, Rect rect)
        {
            if (node.prereqs.NullOrEmpty() && node.prereqsResearch.NullOrEmpty() && node.prereqsStats.NullOrEmpty() && 
                node.prereqsSkills.NullOrEmpty() && node.prereqsHediff.NullOrEmpty()) return 0f;
            float xMin = rect.xMin;
            float yMin = rect.yMin;

            Tuple<int, int> prereqsDone = PrereqsDone(node);

            if (!node.prereqs.NullOrEmpty()) 
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodePrereqs".Translate() + PrereqsModeNotif(node.prereqMode, node.prereqModeMin, prereqsDone.Item1), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (LearningTreeNodeDef prereq in node.prereqs)
                {
                    SetPrereqStatusColor(Schema.learningNodeRecord.completion[prereq], node);
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    if (Widgets.ButtonInvisible(rect, true))
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera(null);
                        if(node.learningTrackerDef != prereq.learningTrackerDef)
                        {
                            if(Find.WindowStack.currentlyDrawnWindow as Dialog_LearningTabs is Dialog_LearningTabs learningTabs && 
                                learningTabs != null && schemaDef.learningTrackerDefs.Contains(prereq.learningTrackerDef))
                            {
                                learningTabs.curTracker = Schema.learningTrackers.FirstOrDefault(x => x.def == prereq.learningTrackerDef);
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
                Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":" + PrereqsModeNotif(node.prereqResearchMode, node.prereqResearchModeMin, prereqsDone.Item2), true, false);
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

            if (!node.prereqsStats.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeStatReq".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsStatCase in node.prereqsStats)
                {
                    foreach(var statMod in prereqsStatCase.statReqs)
                    {
                        SetPrereqStatusColor(!PrereqFailStatCase(statMod, prereqsStatCase.mode), node);
                        Widgets.LabelCacheHeight(ref rect, statMod.stat.LabelCap + PrereqsStatsModeNotif(prereqsStatCase.mode) +
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
                        SetPrereqStatusColor(!PrereqFailSkillCase(skillLevel.skillDef, skillLevel.ClampedLevel, prereqsSkillCase.mode), node);
                        Widgets.LabelCacheHeight(ref rect, skillLevel.skillDef.LabelCap + PrereqsStatsModeNotif(prereqsSkillCase.mode) +
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
                if(Schema.learningNodeRecord.completion[ex]) GUI.color = ColorLibrary.RedReadable;

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
                if (statDrawEntry.ShouldDisplay)
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            return stringBuilder.ToString();
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
            if (Schema.learningNodeRecord.completion[node])
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

        public bool PrereqFufilled(LearningTreeNodeDef node)
        {
            switch(node.prereqMode){
                case LearningNodePrereqMode.All:
                    foreach (LearningTreeNodeDef prereq in node.prereqs)
                    {
                        if (!Schema.learningNodeRecord.completion[prereq]) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (LearningTreeNodeDef prereq in node.prereqs)
                    {
                        if (Schema.learningNodeRecord.completion[prereq]) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (node.prereqModeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(node.prereqModeMin, node.prereqs.Count());
                    foreach (LearningTreeNodeDef prereq in node.prereqs)
                    {
                        if (Schema.learningNodeRecord.completion[prereq]) count++;
                        if (count >= check) return true;
                    }
                    return false;

                default:
                    break;
            }

            return true;
        }

        public bool PrereqResearchFufilled(LearningTreeNodeDef node)
        {
            switch (node.prereqResearchMode)
            {
                case LearningNodePrereqMode.All:
                    foreach (ResearchProjectDef prereq in node.prereqsResearch)
                    {
                        if (!prereq.IsFinished) return false;
                    }
                    return true;

                case LearningNodePrereqMode.Or:
                    foreach (ResearchProjectDef prereq in node.prereqsResearch)
                    {
                        if (prereq.IsFinished) return true;
                    }
                    return false;

                case LearningNodePrereqMode.Min:
                    if (node.prereqResearchModeMin <= 0) return true;

                    int count = 0;
                    int check = Math.Min(node.prereqResearchModeMin, node.prereqs.Count());
                    foreach (ResearchProjectDef prereq in node.prereqsResearch)
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

        public Tuple<int,int> PrereqsDone(LearningTreeNodeDef node)
        {
            int prereqCount = 0;
            if (!node.prereqs.NullOrEmpty()) prereqCount = node.prereqs.Where(x => Schema.learningNodeRecord.completion[x]).Count();

            int prereqResearchCount = 0;
            if (!node.prereqs.NullOrEmpty()) prereqResearchCount = node.prereqsResearch.Where(x => x.IsFinished).Count();

            return new Tuple<int, int> (prereqCount, prereqResearchCount);
        }

        public string PrereqsModeNotif(LearningNodePrereqMode mode, int min = 0, int done = 0)
        {
            if (mode == LearningNodePrereqMode.Min && min > 0)
                return " (" + done + "/" + min + ")";
            if (mode == LearningNodePrereqMode.Or)
                return " (" + done + "/1)";
            return "";
        }

        public bool PrereqStatFufilled(LearningTreeNodeDef node)
        {
            if (node.prereqsStats.NullOrEmpty()) return true;
            foreach (var statReqsCase in node.prereqsStats)
            {
                foreach(var statMod in statReqsCase.statReqs)
                {
                    if (PrereqFailStatCase(statMod, statReqsCase.mode)) return false;
                }
            }

            return true;
        }

        public bool PrereqFailStatCase(StatModifier statMod, LearningNodeStatPrereqMode mode)
        {
            switch(mode){
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

        public bool PrereqSkillFufilled(LearningTreeNodeDef node)
        {
            if (node.prereqsSkills.NullOrEmpty()) return true;
            foreach (var skillReqsCase in node.prereqsSkills)
            {
                foreach (var skillLevel in skillReqsCase.skillReqs)
                {
                    if (PrereqFailSkillCase(skillLevel.skillDef, skillLevel.ClampedLevel, skillReqsCase.mode)) return false;
                }
            }

            return true;
        }

        public bool PrereqFailSkillCase(SkillDef skillDef, int level, LearningNodeStatPrereqMode mode)
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

        public string PrereqsStatsModeNotif(LearningNodeStatPrereqMode mode)
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

        public bool PrereqHediffFufilled(LearningTreeNodeDef node)
        {
            Hediff hediff;
            foreach (var pair in node.prereqsHediff)
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(pair.Key);
                if (hediff == null) return false;
                else if (hediff.Severity < pair.Value) return false;
            }

            return true;
        }

        public bool ExclusiveNodeFufilled(LearningTreeNodeDef node)
        {
            if (!ExclusiveNodes.ContainsKey(node)) return true;

            foreach(LearningTreeNodeDef ex in ExclusiveNodes[node])
            {
                if (Schema.learningNodeRecord.completion[ex]) return false;
            }

            return true;
        }

        // consider removing this
        public virtual void CompletionAbilities(LearningTreeNodeDef node)
        {
            Pawn_AbilityTracker abilityTracker = pawn.abilities;

            foreach(AbilityDef abilityDef in node.abilityGain)
            {
                abilityTracker.GainAbility(abilityDef);
            }

            foreach (AbilityDef abilityDef in node.abilityRemove)
            {
                abilityTracker.RemoveAbility(abilityDef);
            }
        }

        public virtual void CompletionHediffs(LearningTreeNodeDef node)
        {
            Hediff hediff;
            foreach (NodeHediffProps props in node.hediffAdd)
            {
                hediff = HediffMaker.MakeHediff(props.hediffDef, pawn, null);
                hediff.Severity = props.severity;

                pawn.health.AddHediff(hediff, null, null, null);
            }

            foreach (NodeHediffProps props in node.hediffAdjust)
            {
                HealthUtility.AdjustSeverity(pawn, props.hediffDef, props.severity);
            }

            foreach (HediffDef hediffDef in node.hediffRemove)
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null) pawn.health.RemoveHediff(hediff);
            }
        }

        public virtual void CompletionModifiers(LearningTreeNodeDef node)
        {
            ProgressTracker progressTracker = Schema.progressTracker; // get progresstracker
            progressTracker.AdjustModifiers(node.statOffsets, node.statFactors, node.capMods); // update list of statMods and capMods
            progressTracker.hediff.curStage = progressTracker.RefreshCurStage(); // rebuild hediffstage with adjusted stats & set hediff curstage to it
        }

        public override void DrawRightGUI(Rect rect)
        {
            rect.yMin += 32f;

            Rect outRect = rect.ContractedBy(10f);
            //Rect viewRect = outRect.ContractedBy(10f);
            //Rect groupRect = viewRect.ContractedBy(10f);

            Rect viewRect = new Rect(0f, 0f, ViewSize.x, ViewSize.y);
            viewRect.ContractedBy(10f);
            viewRect.width = ViewSize.x;
            //Log.Message("nodecount: " + allNodes.Count().ToString());
            //Log.Message(viewRect.width.ToString());
            //Log.Message(viewRect.height.ToString());
            Rect groupRect = viewRect.ContractedBy(10f);

            //this.scrollPositioner.ClearInterestRects();
            Widgets.BeginScrollView(outRect, ref this.rightScrollPosition, viewRect, true);
            Widgets.ScrollHorizontal(outRect, ref this.rightScrollPosition, viewRect, 20f);
            Widgets.BeginGroup(groupRect);

            // first pass- draw the lines for the node requirements
            Rect nodeRect;
            foreach (LearningTreeNodeDef node in AllRelativeNodes)
            {
                if (!PrereqFufilled(node) && node.condVisiblePrereq) continue;

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
                if (!PrereqFufilled(node) && node.condVisiblePrereq) continue;

                nodeRect = GetNodeRect(node);

                if (Widgets.CustomButtonText(ref nodeRect, "", SelectionBGColor(node),
                    new Color(0.8f, 0.85f, 1f), SelectionBorderColor(node), false, 1, true, true))
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

            if (Schema.learningNodeRecord.completion[node]) baseCol = TexUI.FinishedResearchColor;

            else if (!ExclusiveNodeFufilled(node)) baseCol = ColorLibrary.BrickRed;

            else if (PrereqFufilled(node) && PrereqResearchFufilled(node)) baseCol = TexUI.AvailResearchColor;

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
                    if(!Schema.learningNodeRecord.completion[node]) return TexUI.DependencyOutlineResearchColor;
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
                if (Schema.learningNodeRecord.completion[prereq])
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
        public LearningTreeNodeDef selectedNode;
        
        private List<LearningTreeNodeDef> cachedAllNodes;

        private Dictionary<LearningTreeNodeDef, List<LearningTreeNodeDef>> cacheExclusive; 

        private SorcerySchema cachedSchema;

        private Vector2 cachedViewSize;

        // public Dictionary<LearningTreeNodeDef, bool> completion = new Dictionary<LearningTreeNodeDef, bool>();

        private ScrollPositioner scrollPositioner = new ScrollPositioner();

        private float leftStartAreaHeight = 68f;

        private float leftViewDebugHeight;

        private Vector2 leftScrollPosition = Vector2.zero;

        private float leftScrollViewHeight;

        private Vector2 rightScrollPosition = Vector2.zero;
    }
}
