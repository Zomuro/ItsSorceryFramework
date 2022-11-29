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

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Collections.Look(ref nodes, "nodes", LookMode.Def);
            Scribe_Collections.Look(ref completion, "completion", LookMode.Def, LookMode.Value);

        }

        public override void DrawLeftGUI(Rect rect)
        {
            /*if (selectedNode != null)
            {
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Medium;
                Widgets.Label(rect, selectedNode.LabelCap);

                rect.yMin += 32f;
                Rect desc = new Rect(rect);
                desc.yMax = rect.yMax - 50f;
                Text.Font = GameFont.Small;
                Widgets.Label(desc, selectedNode.description);

                Text.Anchor = TextAnchor.MiddleCenter;
                if (!completion[selectedNode] && prereqFufilled(selectedNode) && 
                    Widgets.ButtonText(new Rect(rect.x, rect.yMax - 50f, 140, 50), "complete"))
                {
                    completion[selectedNode] = true;
                    completionAbilities(selectedNode);
                }
                Text.Anchor = TextAnchor.UpperLeft;
            }*/

            float outRectHeight = rect.height - (10f + leftStartAreaHeight) - 45f;

            Widgets.BeginGroup(rect);
            if (this.selectedNode != null)
            {
                Rect outRect = new Rect(0f, 0f, rect.width, outRectHeight - leftViewDebugHeight);
                Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, leftScrollViewHeight);
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

                /*Rect rect11 = new Rect(0f, outRect.yMax + 10f + this.leftViewDebugHeight, rect.width, this.leftStartAreaHeight);
                if (this.selectedProject.CanStartNow && this.selectedProject != Find.ResearchManager.currentProj)
                {
                    this.leftStartAreaHeight = 68f;
                    if (Widgets.ButtonText(rect11, "Research".Translate(), true, true, true, null))
                    {
                        this.AttemptBeginResearch(this.selectedProject);
                    }
                }
                else
                {
                    string text2;
                    if (this.selectedProject.IsFinished)
                    {
                        text2 = "Finished".Translate();
                        Text.Anchor = TextAnchor.MiddleCenter;
                    }
                    else if (this.selectedProject == Find.ResearchManager.currentProj)
                    {
                        text2 = "InProgress".Translate();
                        Text.Anchor = TextAnchor.MiddleCenter;
                    }
                    else
                    {
                        text2 = "";
                        if (!this.selectedProject.PrerequisitesCompleted)
                        {
                            text2 += "\n  " + "PrerequisitesNotCompleted".Translate();
                        }
                        if (!this.selectedProject.TechprintRequirementMet)
                        {
                            text2 += "\n  " + "InsufficientTechprintsApplied".Translate(this.selectedProject.TechprintsApplied, this.selectedProject.TechprintCount);
                        }
                        if (!this.selectedProject.PlayerHasAnyAppropriateResearchBench)
                        {
                            text2 += "\n  " + "MissingRequiredResearchFacilities".Translate();
                        }
                        if (!this.selectedProject.PlayerMechanitorRequirementMet)
                        {
                            text2 += "\n  " + "MissingRequiredMechanitor".Translate();
                        }
                        if (!this.selectedProject.StudiedThingsRequirementsMet)
                        {
                            text2 = text2 + "\n" + (from t in this.selectedProject.requiredStudied
                                                    select "NotStudied".Translate(t.LabelCap).ToString()).ToLineList("  ", false);
                        }
                        if (text2.NullOrEmpty())
                        {
                            Log.ErrorOnce("Research " + this.selectedProject.defName + " locked but no reasons given", this.selectedProject.GetHashCode() ^ 100543441);
                        }
                        text2 = "Locked".Translate() + ":" + text2;
                    }
                    this.leftStartAreaHeight = Mathf.Max(Text.CalcHeight(text2, rect11.width - 10f) + 10f, 68f);
                    Widgets.DrawHighlight(rect11);
                    Widgets.Label(rect11.ContractedBy(5f), text2);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                Rect rect12 = new Rect(0f, rect11.yMax + 10f, rect.width, 35f);
                Widgets.FillableBar(rect12, this.selectedProject.ProgressPercent, MainTabWindow_Research.ResearchBarFillTex, MainTabWindow_Research.ResearchBarBGTex, true);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect12, this.selectedProject.ProgressApparent.ToString("F0") + " / " + this.selectedProject.CostApparent.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
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

        /*public virtual void completionAbilities(LearningTreeNodeDef node)
        {
            Pawn_AbilityTracker abilityTracker = this.pawn.abilities;

            if (node.nodeAbilityGain != null) abilityTracker.GainAbility(node.nodeAbilityGain);
            if (node.nodeAbilityRemove != null) abilityTracker.RemoveAbility(node.nodeAbilityRemove);
        }*/

        public override void DrawRightGUI(Rect rect)
        {
            rect.yMin += 32f;

            Rect outRect = rect.ContractedBy(10f);
            Rect viewRect = outRect.ContractedBy(10f);
            Rect groupRect = viewRect.ContractedBy(10f);


            //Widgets.ScrollHorizontal(outRect, ref this.rightScrollPosition, viewRect, 20f);
			//Widgets.BeginScrollView(outRect, ref this.rightScrollPosition, viewRect, true);
			Widgets.BeginGroup(groupRect);
            
            foreach (LearningTreeNodeDef ltnDef in allNodes)
            {
                Rect nodeRect = getNodeRect(ltnDef);

                /*Color BG = selectionBGColor(ltnDef);
                Color border = selectionBorderColor(ltnDef);*/

                if (Widgets.CustomButtonText(ref nodeRect, "", selectionBGColor(ltnDef),
                    new Color(0.8f, 0.85f, 1f), selectionBorderColor(ltnDef), false, 1, true, true))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera(null);
                    this.selectedNode = ltnDef;
                }
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(nodeRect, ltnDef.LabelCap);
                Text.Anchor = TextAnchor.UpperLeft;

                foreach (LearningTreeNodeDef prereq in ltnDef.prereqs)
                {
                    Tuple<Vector2, Vector2> points = lineEnds(prereq, ltnDef, nodeRect);
                    Widgets.DrawLine(points.Item1, points.Item2, selectionLineColor(ltnDef), 2f);
                }

            }

			Widgets.EndGroup();
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
