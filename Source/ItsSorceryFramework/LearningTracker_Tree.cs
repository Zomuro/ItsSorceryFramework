using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

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

        public List<LearningTreeNodeDef> allNodes
        {
            get
            {
                if (cachedAllNodes == null)
                {
                    cachedAllNodes = new List<LearningTreeNodeDef>(from def in DefDatabase<LearningTreeNodeDef>.AllDefsListForReading
                                                                   where def.learningTracker == this.def
                                                                   select def);

                    foreach (LearningTreeNodeDef node in cachedAllNodes)
                    {
                        if (!completion.Keys.Contains(node)) completion[node] = false;
                    }
                }

                return cachedAllNodes;
            }
        }

        public Vector2 ViewSize
        {
            get
            {
                if (cachedViewSize == null || cachedViewSize == Vector2.zero) cachedViewSize = findViewSize();

                return cachedViewSize;
            }
        }

        public SorcerySchema schema
        {
            get
            {
                if (cachedSchema == null) cachedSchema = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef);

                return cachedSchema;
            }
        }

        public float pointUsePercent
        {
            get
            {
                ProgressTracker progress = schema.progressTracker;
                if (progress.points == 0) return 0;
                return (float) (progress.points - progress.usedPoints) / progress.points;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Collections.Look(ref nodes, "nodes", LookMode.Def);
            Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value);

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
                Widgets.LabelCacheHeight(ref labelRect, this.selectedNode.LabelCap, true, false);
                GenUI.ResetLabelAlign();
                Text.Font = GameFont.Small;
                coordY += labelRect.height;

                Rect descRect = new Rect(0f, coordY, viewRect.width, 0f);
                Widgets.LabelCacheHeight(ref descRect, this.selectedNode.description, true, false);
                coordY += descRect.height;

                Rect prereqRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += this.drawNodePrereqs(this.selectedNode, prereqRect);
                Rect hyperlinkRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += this.drawHyperlinks(hyperlinkRect, selectedNode);

                Rect contentRect = new Rect(0f, coordY, viewRect.width, 500f);
                coordY += this.drawContentSource(contentRect, selectedNode);
                coordY += 3f;
                this.leftScrollViewHeight = coordY;
                Widgets.EndScrollView();
                leftViewDebugHeight = 0f;

                ProgressTracker progress = schema.progressTracker;
                Rect confirmButton = new Rect(0f, outRect.yMax + 10f + this.leftViewDebugHeight, rect.width, this.leftStartAreaHeight);
                string reason = "";
                if (!completion[selectedNode] && prereqFufilled(selectedNode) && prereqResearchFufilled(selectedNode) &&
                    selectedNode.pointReq + progress.usedPoints <= progress.points) 
                {
                    if (Widgets.ButtonText(confirmButton, "complete: " + selectedNode.pointReq))
                    {
                        completion[selectedNode] = true;
                        completionAbilities(selectedNode);
                        //progress.usedPoints += selectedNode.pointReq;
                        schema.progressTracker.usedPoints += selectedNode.pointReq;
                    }
                }
                else
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    if (completion[selectedNode]) reason = "Completed.";
                    else
                    {
                        reason = "Locked:";

                        if (selectedNode.pointReq + progress.usedPoints > progress.points) reason += "\nNot enough skill points.";

                        if (!prereqFufilled(selectedNode)) reason += "\nPrior nodes not completed.";

                        if (!prereqResearchFufilled(selectedNode)) reason += "\nResearch requirements not completed.";

                        // if for exlusive nodes check
                    }

                    this.leftStartAreaHeight = Mathf.Max(Text.CalcHeight(reason, confirmButton.width - 10f) + 10f, 68f);
                    Widgets.DrawHighlight(confirmButton);
                    Widgets.Label(confirmButton.ContractedBy(5f), reason);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                
                Rect pointBar = new Rect(0f, confirmButton.yMax + 10f, rect.width, 35f);
                Widgets.FillableBar(pointBar, pointUsePercent);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(pointBar, (progress.points-progress.usedPoints).ToString("F0") + " / " + schema.progressTracker.points.ToString("F0"));
                
                /*Text.Anchor = TextAnchor.UpperLeft;
                this.leftViewDebugHeight = 0f;
                if (Prefs.DevMode && this.selectedProject != Find.ResearchManager.currentProj && !this.selectedProject.IsFinished)
                {
                    Text.Font = GameFont.Tiny;
                    Rect rect13 = new Rect(rect11.x, outRect.yMax, 120f, 30f);
                    if (Widgets.ButtonText(rect13, "Debug: Finish now", true, true, true, null))
                    {
                        Find.ResearchManager.currentProj = this.selectedProject;
                        Find.ResearchManager.FinishProject(this.selectedProject, false, null, true);
                    }
                    Text.Font = GameFont.Small;
                    this.leftViewDebugHeight = rect13.height;
                }
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

        private float drawNodePrereqs(LearningTreeNodeDef node, Rect rect)
        {
            if (node.prereqs.NullOrEmpty())
            {
                return 0f;
            }
            float xMin = rect.xMin;
            float yMin = rect.yMin;

            Widgets.LabelCacheHeight(ref rect, "LearningNodePrerequisites_ISF".Translate() + ":", true, false);
            rect.yMin += rect.height;
            rect.xMin += 6f;
            foreach (LearningTreeNodeDef prereq in node.prereqs)
            {
                this.setPrereqStatusColor(completion[prereq], node);
                Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                rect.yMin += rect.height;
            }
            rect.xMin = xMin;
            GUI.color = Color.white;

            Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":", true, false);
            rect.yMin += rect.height;
            rect.xMin += 6f;
            foreach (ResearchProjectDef prereq in node.prereqsResearch)
            {
                this.setPrereqStatusColor(prereq.IsFinished, node);
                Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                rect.yMin += rect.height;
            }
            GUI.color = Color.white;
            rect.xMin = xMin;
            return rect.yMin - yMin;
        }

        private float drawHyperlinks(Rect rect, LearningTreeNodeDef node)
        {
            List<AbilityDef> abilityGain = node.abilityGain;
            List<AbilityDef> abilityRemove = node.abilityRemove;
            Dictionary<HediffDef, float> hediffAdjust = node.hediffAdjust;
            List<HediffDef> hediffRemove = node.hediffRemove;

            if (abilityGain.NullOrEmpty() && abilityRemove.NullOrEmpty() && hediffAdjust.NullOrEmpty() && hediffRemove.NullOrEmpty())
            {
                return 0f;
            }

            float yMin = rect.yMin;
            float x = rect.x;
            Dialog_InfoCard.Hyperlink hyperlink;

            if (!abilityGain.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Abilities gained:", true, false);
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
                Widgets.LabelCacheHeight(ref rect, "Abilities removed:", true, false);
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

            if (!hediffAdjust.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Hediffs added:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (var pair in hediffAdjust)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    HediffDef hediffDef = pair.Key;
                    string stageName = hediffDef.stages[hediffDef.StageAtSeverity(pair.Value)].label;
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, hediffDef.LabelCap + "({0})".Translate(stageName), 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!hediffRemove.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Hediffs removed:", true, false);
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

        // yeah i just copied this from MainTabWindow_Research
        private float drawContentSource(Rect rect, LearningTreeNodeDef node)
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

        private void setPrereqStatusColor(bool compCheck, LearningTreeNodeDef node)
        {
            if (completion[node])
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

        public bool prereqFufilled(LearningTreeNodeDef node)
        {
            foreach(LearningTreeNodeDef prereq in node.prereqs)
            {
                if (!completion[prereq]) return false;
            }

            return true;
        }

        public bool prereqResearchFufilled(LearningTreeNodeDef node)
        {
            foreach (ResearchProjectDef prereq in node.prereqsResearch)
            {
                if (!prereq.IsFinished) return false;
            }

            return true;
        }

        public virtual void completionAbilities(LearningTreeNodeDef node)
        {
            Pawn_AbilityTracker abilityTracker = this.pawn.abilities;

            foreach(AbilityDef abilityDef in node.abilityGain)
            {
                abilityTracker.GainAbility(abilityDef);
            }

            foreach (AbilityDef abilityDef in node.abilityRemove)
            {
                abilityTracker.RemoveAbility(abilityDef);
            }
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
            Log.Message("nodecount: " + allNodes.Count().ToString());
            Log.Message(viewRect.width.ToString());
            Log.Message(viewRect.height.ToString());
            Rect groupRect = viewRect.ContractedBy(10f);

            //this.scrollPositioner.ClearInterestRects();
            Widgets.BeginScrollView(outRect, ref this.rightScrollPosition, viewRect, true);
            Widgets.ScrollHorizontal(outRect, ref this.rightScrollPosition, viewRect, 20f);
            Widgets.BeginGroup(groupRect);
            
            foreach (LearningTreeNodeDef node in allNodes)
            {
                Rect nodeRect = getNodeRect(node);
                if (Widgets.CustomButtonText(ref nodeRect, "", selectionBGColor(node),
                    new Color(0.8f, 0.85f, 1f), selectionBorderColor(node), false, 1, true, true))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera(null);
                    this.selectedNode = node;
                }

                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.LabelCacheHeight(ref nodeRect, node.LabelCap, true, false);
                Text.Anchor = TextAnchor.UpperLeft;

                foreach (LearningTreeNodeDef prereq in node.prereqs)
                {
                    Tuple<Vector2, Vector2> points = lineEnds(prereq, node, nodeRect);
                    Widgets.DrawLine(points.Item1, points.Item2, selectionLineColor(node), 2f);
                }

            }

			Widgets.EndGroup();
            Widgets.EndScrollView();
            //this.scrollPositioner.ScrollHorizontally(ref this.rightScrollPosition, outRect.size);

            //Widgets.EndScrollView();
            //this.scrollPositioner.ScrollHorizontally(ref this.rightScrollPosition, outRect.size);

        }

        private float CoordToPixelsX(float x)
        {
            return x * 190f;
        }

        private float CoordToPixelsY(float y)
        {
            return y * 100f;
        }

        private Rect getNodeRect(LearningTreeNodeDef nodeDef)
        {
            return new Rect(CoordToPixelsX(nodeDef.coordX), CoordToPixelsY(nodeDef.coordY), 140f, 50f);
        }

        private Tuple<Vector2, Vector2> lineEnds(LearningTreeNodeDef start, LearningTreeNodeDef end, Rect nodeRef)
        {
            Vector2 prereq = new Vector2(CoordToPixelsX(start.coordX), CoordToPixelsY(start.coordY));
            prereq.x += nodeRef.width;
            prereq.y += nodeRef.height/2;
            Vector2 current = new Vector2(CoordToPixelsX(end.coordX), CoordToPixelsY(end.coordY));
            current.y += nodeRef.height/2;

            return new Tuple<Vector2, Vector2>(prereq, current);
        }

        private Vector2 findViewSize()
        {
            float x = 0f;
            float y = 0f;
            foreach (LearningTreeNodeDef node in allNodes)
            { 
                x = Mathf.Max(x, this.CoordToPixelsX(node.coordX) + 140f);
                y = Mathf.Max(y, this.CoordToPixelsY(node.coordY) + 50f);
            }

            return new Vector2(x + 32f, y + 32f);
        }

        private Color selectionBGColor(LearningTreeNodeDef node)
        {
            Color baseCol = TexUI.AvailResearchColor;

            if (completion[node]) baseCol = TexUI.FinishedResearchColor;

            // if the node is the selected one, change background to highlight
            if (selectedNode != null && selectedNode == node) return baseCol + TexUI.HighlightBgResearchColor;

            return baseCol;
        }

        private Color selectionBorderColor(LearningTreeNodeDef node)
        {
            // if the node is the selected one OR a prerequisite, change border to highlight
            if (selectedNode != null)
            {
                if (selectedNode == node) return TexUI.HighlightBorderResearchColor;

                else if (selectedNode.prereqs.NotNullAndContains(node))
                {
                    if(!completion[node]) return TexUI.DependencyOutlineResearchColor;
                    else return TexUI.HighlightLineResearchColor;
                }

            }

            return TexUI.DefaultBorderResearchColor;
        }

        private Color selectionLineColor(LearningTreeNodeDef node)
        {
            if (selectedNode == node) return TexUI.HighlightLineResearchColor;

            return TexUI.DefaultLineResearchColor;
        }

        public List<LearningTreeNodeDef> cachedAllNodes;

        public SorcerySchema cachedSchema;

        public Vector2 cachedViewSize;

        public LearningTreeNodeDef selectedNode;

        //public Color colorSelected = TexUI.DefaultBorderResearchColor;

        public Dictionary<LearningTreeNodeDef, bool> completion = new Dictionary<LearningTreeNodeDef, bool>();

        private ScrollPositioner scrollPositioner = new ScrollPositioner();

        private float leftStartAreaHeight = 68f;

        private float leftViewDebugHeight;

        private Vector2 leftScrollPosition = Vector2.zero;

        private float leftScrollViewHeight;

        private Vector2 rightScrollPosition = Vector2.zero;
    }
}
