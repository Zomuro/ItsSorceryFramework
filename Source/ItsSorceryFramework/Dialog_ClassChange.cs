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
    public class Dialog_ClassChange : Window
    {
		List<ProgressTrackerClassDef> allClassDefs;

		ProgressTracker progressTracker;

		ProgressTrackerClassDef currClassDef;

        private Vector2 cachedViewSize;

        private ScrollPositioner scrollPositioner = new ScrollPositioner();

        private float leftStartAreaHeight = 68f;

        private float leftViewDebugHeight;

        private Vector2 leftScrollPosition = Vector2.zero;

        private float leftScrollViewHeight;

        public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(600, 400);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return false;
			}
		}

		public Dialog_ClassChange(ProgressTracker progressTracker, List<ProgressTrackerClassDef> classDefs) : base()
		{
			this.progressTracker = progressTracker;
			allClassDefs = classDefs;
			currClassDef = allClassDefs.NullOrEmpty() ? null : allClassDefs[0];
			closeOnClickedOutside = true;
			forcePause = true;
			closeOnCancel = true;
			doCloseX = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(inRect);

			
		}

		public virtual void DrawLeftGUI(Rect rect)
        {
			Rect listingRect = rect.ContractedBy(5f);

			Listing_Standard listing = new Listing_Standard();
			listing.Begin(listingRect);

			// for all class
			foreach(var c in allClassDefs)
            {
				// creates button to select class; if class selected changes curr class info displayed
				if(listing.ButtonDebug(c.label, currClassDef != null && c == currClassDef))
                {
					currClassDef = c;
                }
            }

			listing.End();

        }

        public virtual void DrawRightGUI(Rect rect)
        {
            float outRectHeight = rect.height - (10f + leftStartAreaHeight) - 45f;

            Widgets.BeginGroup(rect);

            if (currClassDef is null)
            {
                Widgets.EndGroup();
                return;
            }

            Rect outRect = new Rect(0f, 0f, rect.width, outRectHeight - leftViewDebugHeight);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 20f, leftScrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.leftScrollPosition, viewRect, true);

            float coordY = 0f;

            // class label
            Text.Font = GameFont.Medium;
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            Rect labelRect = new Rect(0f, coordY, viewRect.width, 50f);
            Widgets.LabelCacheHeight(ref labelRect, currClassDef.LabelCap, true, false);
            coordY += labelRect.height;

            // class desc
            GenUI.ResetLabelAlign();
            Text.Font = GameFont.Small;
            Rect descRect = new Rect(0f, coordY, viewRect.width, 0f);
            Widgets.LabelCacheHeight(ref descRect, currClassDef.description, true, false);
            coordY += descRect.height;

            /*Rect pointRect = new Rect(0f, coordY, viewRect.width, 500f);
            coordY += DrawPointReq(selectedNode, pointRect);*/

            // draw reqs
            Rect prereqRect = new Rect(0f, coordY, viewRect.width, 500f);
            coordY += DrawClassPrereqs(prereqRect, currClassDef);

            /*Rect exclusiveRect = new Rect(0f, coordY, viewRect.width, 500f);
            coordY += DrawExclusive(selectedNode, exclusiveRect);

            Rect hyperlinkRect = new Rect(0f, coordY, viewRect.width, 500f);
            coordY += DrawHyperlinks(hyperlinkRect, selectedNode);

            Rect statModRect = new Rect(0f, coordY, viewRect.width, 500f);
            coordY += DrawStatMods(statModRect, selectedNode);*/

            Rect unlockRect = new Rect(0f, coordY, viewRect.width, 500f);
            coordY += DrawUnlockedLearningTrackers(unlockRect, currClassDef);

            Rect contentRect = new Rect(0f, coordY, viewRect.width, 500f);
            coordY += DrawContentSource(contentRect, currClassDef);
            coordY += 3f;
            leftScrollViewHeight = coordY;
            Widgets.EndScrollView();

            Pawn pawn = progressTracker.pawn;
            Rect confirmButton = new Rect(0f, outRect.yMax + 10f + this.leftViewDebugHeight, rect.width, this.leftStartAreaHeight);
            string reason = "";
            if (!pawn.Faction.IsPlayer || pawn.Faction is null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.DrawHighlight(confirmButton);
                reason = "ISF_ClassChangeNotPlayer".Translate(pawn.Name.ToStringShort);
                Widgets.Label(confirmButton.ContractedBy(5f), reason);
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else if (progressTracker.progressDiffLog.ValidateClassChange(progressTracker, currClassDef, out reason))
            {
                if (Widgets.ButtonText(confirmButton, "ISF_ClassChangeConfirm".Translate()))
                {
                    LearningNodeRecord LearningRecord = progressTracker.schema.learningNodeRecord;

                    ProgressDiffLog diffLog = progressTracker.progressDiffLog;
                    ProgressDiffLedger progressDiffLedger = diffLog.PrepNewLedger(progressTracker);
                    ProgressDiffClassLedger progressDiffClassLedger = new ProgressDiffClassLedger();


                    progressDiffLedger.classDiffLedgers[progressTracker.currClassDef] = progressDiffClassLedger;
                    diffLog.AddLedger(progressDiffLedger);

                    CompletionLearningUnlock(currClassDef);

                    foreach (var et in progressTracker.schema.energyTrackers) et.ClearStatCache();
                }
            }




            /*else if (!LearningRecord.completion[selectedNode] && LearningRecord.PrereqFufilled(selectedNode) && LearningRecord.PrereqResearchFufilled(selectedNode) &&
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
                    progressDiffLedger.classDiffLedgers[schema.progressTracker.currClassDef] = progressDiffClassLedger;
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
                    reason = "ISF_ClassChangeLocked".Translate();

                    *//*if (selectedNode.pointReq + progress.usedPoints > progress.points) reason += "\n" +
                            "ISF_LearningNodeLockedPoints".Translate(schema.progressTracker.def.skillPointLabelKey.Translate());*//*

                    if (!PrereqFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedNodes".Translate();

                    if (!PrereqResearchFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedResearch".Translate();

                    if (!PrereqLevelFulfilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedLevel".Translate();

                    if (!PrereqStatFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedStat".Translate();

                    if (!PrereqSkillFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedSkill".Translate();

                    if (!PrereqHediffFufilled(selectedNode)) reason += "\n" + "ISF_LearningNodeLockedHediff".Translate();
                }

                this.leftStartAreaHeight = Mathf.Max(Text.CalcHeight(reason, confirmButton.width - 10f) + 10f, 68f);
                Widgets.DrawHighlight(confirmButton);
                Widgets.Label(confirmButton.ContractedBy(5f), reason);
                Text.Anchor = TextAnchor.UpperLeft;
            }*/

            Widgets.EndGroup();
        }

        private float DrawClassPrereqs(Rect rect, ProgressTrackerClassDef classDef)
        {
            if (classDef.prereqClasses.NullOrEmpty() && classDef.prereqsResearch.NullOrEmpty() && classDef.prereqLevel <= 0 && classDef.prereqsStats.NullOrEmpty() &&
                classDef.prereqsSkills.NullOrEmpty() && classDef.prereqsHediff.NullOrEmpty()) return 0f;
            float xMin = rect.xMin;
            float yMin = rect.yMin;

            LearningNodeRecord LearningRecord = progressTracker.schema.learningNodeRecord; // reuse old methods here
            Tuple<int, int> prereqsDone = PrereqsDone(classDef);
            ProgressDiffLog diffLog = progressTracker.progressDiffLog;

            if (!classDef.prereqClasses.NullOrEmpty()) // show completed prior classes
            {
                HashSet<ProgressTrackerClassDef> classesDone = progressTracker.progressDiffLog.GetClassSet;
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangePrereqs".Translate() + LearningRecord.PrereqsModeNotif(classDef.prereqClassMode, classDef.prereqClassModeMin, prereqsDone.Item1), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (ProgressTrackerClassDef prereqClass in classDef.prereqClasses)
                {
                    SetPrereqStatusColor(classesDone.Contains(prereqClass));
                    Widgets.LabelCacheHeight(ref rect, prereqClass.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                rect.xMin = xMin;
                GUI.color = Color.white;
            }

            if (!classDef.prereqsResearch.NullOrEmpty()) // show completed research
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangeResearchPrereqs".Translate() + 
                    ":" + LearningRecord.PrereqsModeNotif(classDef.prereqResearchMode, classDef.prereqResearchModeMin, prereqsDone.Item2), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (ResearchProjectDef prereq in classDef.prereqsResearch)
                {
                    SetPrereqStatusColor(prereq.IsFinished);
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (classDef.prereqLevel > 0) // show completed level req
            {
                SetPrereqStatusColor(diffLog.PrereqLevelFulfilled(progressTracker, classDef)); //!LearningRecord.PrereqLevelFulfilled(node)
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangeLevelReq".Translate(classDef.prereqLevel), true, false);
                rect.yMin += rect.height;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqsStats.NullOrEmpty()) // show completed stat req
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangeStatReq".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsStatCase in classDef.prereqsStats)
                {
                    foreach (var statMod in prereqsStatCase.statReqs)
                    {
                        SetPrereqStatusColor(!LearningRecord.PrereqFailStatCase(statMod, prereqsStatCase.mode));
                        Widgets.LabelCacheHeight(ref rect, statMod.stat.LabelCap + LearningRecord.PrereqsStatsModeNotif(prereqsStatCase.mode) +
                            statMod.stat.ValueToString(statMod.value, ToStringNumberSense.Absolute, !statMod.stat.formatString.NullOrEmpty()), true, false);
                        rect.yMin += rect.height;
                    }
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqsSkills.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangeSkillReq".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsSkillCase in classDef.prereqsSkills)
                {
                    foreach (var skillLevel in prereqsSkillCase.skillReqs)
                    {
                        SetPrereqStatusColor(!LearningRecord.PrereqFailSkillCase(skillLevel.skillDef, skillLevel.ClampedLevel, prereqsSkillCase.mode));
                        Widgets.LabelCacheHeight(ref rect, skillLevel.skillDef.LabelCap + LearningRecord.PrereqsStatsModeNotif(prereqsSkillCase.mode) +
                            skillLevel.ClampedLevel, true, false);
                        rect.yMin += rect.height;
                    }
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqsHediff.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangeHediffReq".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                Hediff hediff;
                String reqLabel;
                foreach (var prereq in classDef.prereqsHediff)
                {
                    hediff = progressTracker.pawn.health.hediffSet.GetFirstHediffOfDef(prereq.Key);
                    SetPrereqStatusColor((hediff != null && hediff.Severity >= prereq.Value));
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
            foreach (StatDrawEntry statDrawEntry in node.specialDisplayMods())
            {
                if (statDrawEntry.ShouldDisplay())
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            return stringBuilder.ToString();
        }

        private float DrawUnlockedLearningTrackers(Rect rect, ProgressTrackerClassDef classDef)
        {
            float yMin = rect.yMin;
            float xMin = rect.xMin;

            if (!classDef.unlocks.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangeUnlocks".Translate(), true, false); // label
                rect.yMin += rect.height;
                rect.xMin += 6f;

                foreach (var ltDef in classDef.unlocks) // displays learningtrackerdefs that will be unlocked, if not
                {
                    if (!progressTracker.schema.def.learningTrackerDefs.Contains(ltDef)) continue;

                    Widgets.LabelCacheHeight(ref rect, ltDef.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                rect.xMin = xMin;
            }

            return rect.yMin - yMin;
        }

        private float DrawContentSource(Rect rect, ProgressTrackerClassDef classDef) // taken from research tab
        {
            if (classDef.modContentPack == null || classDef.modContentPack.IsCoreMod)
            {
                return 0f;
            }
            float yMin = rect.yMin;
            TaggedString taggedString = "Stat_Source_Label".Translate() + ":  " + classDef.modContentPack.Name;
            Widgets.LabelCacheHeight(ref rect, taggedString.Colorize(Color.grey), true, false);
            ExpansionDef expansionDef = ModLister.AllExpansions.Find((ExpansionDef e) => e.linkedMod == classDef.modContentPack.PackageId);
            if (expansionDef != null)
            {
                GUI.DrawTexture(new Rect(Text.CalcSize(taggedString).x + 4f, rect.y, 20f, 20f), expansionDef.IconFromStatus);
            }
            return rect.yMax - yMin;
        }

        private Tuple<int, int> PrereqsDone(ProgressTrackerClassDef classDef)
        {
            HashSet<ProgressTrackerClassDef> classesDone = progressTracker.progressDiffLog.GetClassSet;

            int prereqCount = 0;
            if (!classDef.prereqsClassDefs.NullOrEmpty()) prereqCount = classDef.prereqsClassDefs.Where(x => classesDone.Contains(x)).Count();

            int prereqResearchCount = 0;
            if (!classDef.prereqsResearch.NullOrEmpty()) prereqResearchCount = classDef.prereqsResearch.Where(x => x.IsFinished).Count();

            return new Tuple<int, int>(prereqCount, prereqResearchCount);
        }

        private void SetPrereqStatusColor(bool compCheck)
        {
            if (compCheck)
            {
                GUI.color = Color.green;
                return;
            }
            GUI.color = ColorLibrary.RedReadable;
        }

        private void CompletionLearningUnlock(ProgressTrackerClassDef classDef)
        {
            if (classDef.unlocks.NullOrEmpty()) return;

            foreach (var lt in progressTracker.schema.learningTrackers)
            {
                if (classDef.unlocks.Contains(lt.def)) lt.locked = false;
            }
        }
    }
}
