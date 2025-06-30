using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class Dialog_ClassChange : Window
    {
        //public List<ProgressLinkedClassMap> classChangeOptions;

        public ProgressTracker progressTracker;

        public ProgressLinkedClassMap currClassChangeOption;

        public bool debugShowClassOptions = false;

        private float leftScrollViewHeight;

        private Vector2 leftScrollPosition = Vector2.zero;

        //private ScrollPositioner scrollPositioner = new ScrollPositioner();

        private float rightConfirmHeight = 50f;

        private float rightDebugHeight;

        private Vector2 rightScrollPosition = Vector2.zero;

        private float rightScrollViewHeight;

        public override Vector2 InitialSize => new Vector2(800, 600);

        public override bool IsDebug => false;

        public List<ProgressLinkedClassMap> AllClassChangeOptions => Prefs.DevMode && debugShowClassOptions ? progressTracker.currClassDef.linkedClasses : progressTracker.classChangeOpps; // classChangeOptions;

        public Dialog_ClassChange(ProgressTracker progressTracker) : base() //, List<ProgressLinkedClassMap> classChangeOptions
		{
			this.progressTracker = progressTracker;
			//this.classChangeOptions = classChangeOptions;
            currClassChangeOption = progressTracker.classChangeOpps.NullOrEmpty() ? null : progressTracker.classChangeOpps[0];
			closeOnClickedOutside = true;
			forcePause = true;
			closeOnCancel = true;
			doCloseX = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
            Rect leftRect = new Rect(inRect.x, inRect.y, inRect.width / 3f, inRect.height);
            Widgets.DrawMenuSection(leftRect);

            Rect rightRect = new Rect(leftRect.xMax, inRect.y, inRect.width - leftRect.width, inRect.height);

            DrawLeftGUI(leftRect.ContractedBy(10f)); // draw all class change options
            DrawRightGUI(rightRect.ContractedBy(10f)); // draw information on class change requirements and enable the class change
        }

		public virtual void DrawLeftGUI(Rect rect)
        {
            // create outer bounds of most info
            float coordY = 0f;
            Rect outRect = new Rect(rect.x, rect.y, rect.width, rect.height - 30f); // - 20

            // class label
            Text.Font = GameFont.Medium;
            GenUI.SetLabelAlign(TextAnchor.UpperCenter);
            Rect labelRect = new Rect(outRect.x, outRect.y + coordY, outRect.width, 50f);
            Widgets.LabelCacheHeight(ref labelRect, "ISF_ClassChangeClassChoicesLabel".Translate(), true, false);
            coordY += labelRect.height;
            GenUI.ResetLabelAlign();
            Text.Font = GameFont.Small;

            // create listing outer and view rects
            Rect listingRectOut = new Rect(outRect.x, outRect.y + coordY, outRect.width, outRect.height);
            listingRectOut.yMax = outRect.yMax;
            listingRectOut = listingRectOut.ContractedBy(10f);

            Rect listingRectView = new Rect(listingRectOut);
            listingRectView.width -= 16f;
            listingRectView.height = leftScrollViewHeight;

            // begin scroll view of the listing
            Widgets.BeginScrollView(listingRectOut, ref leftScrollPosition, listingRectView, true);

            // create listing for showing all class change options & start listing
            Listing_Standard listing = new Listing_Standard(rect, () => leftScrollPosition);
            listing.ColumnWidth = listingRectView.width;
            listing.Begin(listingRectOut);

            if (AllClassChangeOptions.NullOrEmpty()) listing.Label("ISF_ClassChangeNoChoices".Translate());

            // for all classes, create debug button for class change option
            foreach (var c in AllClassChangeOptions)
            {
                // creates button to select class; if class selected changes curr class info displayed
                if (listing.ButtonDebug(c.classDef.label, currClassChangeOption != null && c == currClassChangeOption))
                {
                    currClassChangeOption = c;
                }
            }
            
            // end listing and get proper height of all the options for the scroll view
            listing.End();
            leftScrollViewHeight = listing.CurHeight;
            Widgets.EndScrollView();

            // finally, add a dev mode toggle to force show all class change options.
            if (Prefs.DevMode)
            {
                Text.Font = GameFont.Tiny;
                Rect debugButton = new Rect(outRect.x, outRect.yMax, outRect.width, 30f);
                bool orgCheckboxBool = debugShowClassOptions;
                Widgets.CheckboxLabeled(debugButton, "Debug: all class options", ref debugShowClassOptions);
                Text.Font = GameFont.Small;

                // if we toggle between debug and not - set the current class change option to null to prevent access to debug
                if (orgCheckboxBool != debugShowClassOptions) currClassChangeOption = null;
            }

        }

        public virtual void DrawRightGUI(Rect rect)
        {
            // sets all coordinates to be relative to the input rect
            Widgets.BeginGroup(rect);

            // end this function early if there is no class option to display
            if (currClassChangeOption is null)
            {
                Widgets.EndGroup();
                return;
            }

            // define bounding and view rect of scroll window
            float outRectHeight = rect.height - RightDebugHeightAdj - RightConfirmHeightAdj; // account for debug & confirm buttons
            Rect outRect = new Rect(0,0, rect.width, outRectHeight); 
            Rect viewRect = new Rect(outRect.x, outRect.y, outRect.width - 20f, rightScrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.rightScrollPosition, viewRect, true);

            float coordY = 0f; // coord Y to keep track of info y coord

            // class label
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Medium;
            Rect labelRect = new Rect(0, coordY, viewRect.width, 50f);
            Widgets.LabelCacheHeight(ref labelRect, currClassChangeOption.classDef.LabelCap, true, false);
            coordY += labelRect.height;

            // class desc
            Text.Font = GameFont.Small;
            if (!currClassChangeOption.classDef.description.NullOrEmpty()) // null description => just don't show it
            {
                Rect descRect = new Rect(0, coordY, viewRect.width, 0f);
                Widgets.LabelCacheHeight(ref descRect, currClassChangeOption.classDef.description, true, false);
                coordY += descRect.height;
            }
            
            // draw reqs
            Rect prereqRect = new Rect(0, coordY, viewRect.width, 500f);
            coordY += DrawClassPrereqs(prereqRect, currClassChangeOption.classDef);

            // draw unlocks
            Rect unlockRect = new Rect(0, coordY, viewRect.width, 500f);
            coordY += DrawUnlockedLearningTrackers(unlockRect, currClassChangeOption.classDef);

            // draw content class is from
            Rect contentRect = new Rect(0, coordY, viewRect.width, 500f);
            coordY += DrawContentSource(contentRect, currClassChangeOption.classDef);
            coordY += 5f;
            rightScrollViewHeight = coordY;
            Widgets.EndScrollView();

            // get pawn for later + confirm button sizing
            Pawn pawn = progressTracker.pawn;
            Rect confirmButton = new Rect(0, outRectHeight + RightDebugHeightAdj, outRect.width, rightConfirmHeight);
            Text.Anchor = TextAnchor.UpperLeft;

            // draw debug button if in debug mode
            rightDebugHeight = 0f;
            if (Prefs.DevMode)
            {
                Text.Font = GameFont.Tiny;
                Rect debugButton = new Rect(0, outRect.yMax, 120f, 30f);
                debugButton.y += 10f;

                if (Widgets.ButtonText(debugButton, "Debug: Finish now", true, true, true, null))
                {
                    CompleteClassChange();

                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    Close(true);
                    return;
                }
                Text.Font = GameFont.Small;
                rightDebugHeight = debugButton.height;
            }

            // draw confirm button / locked button
            confirmButton.y += 10f;
            bool validChange = progressTracker.progressDiffLog.ValidateClassChange(progressTracker, currClassChangeOption.classDef, out string reason);
            Text.Anchor = TextAnchor.MiddleCenter; // force label to middle center for confirm button / fail

            if (!pawn.Faction.IsPlayer || pawn.Faction is null)
            {
                rightConfirmHeight = 50f;
                Widgets.DrawHighlight(confirmButton);
                reason = "ISF_ClassChangeNotPlayer".Translate(pawn.Name.ToStringShort);
                Widgets.Label(confirmButton.ContractedBy(5f), reason);
            }
            else if (validChange)
            {
                rightConfirmHeight = 50f;
                if (Widgets.ButtonText(confirmButton, "ISF_ClassChangeConfirm".Translate()))
                {
                    CompleteClassChange();

                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    Close(true);
                    return;
                }
            }
            else
            {
                string failReason = "ISF_GeneralDialogLocked".Translate() + "\n" + reason;
                rightConfirmHeight = Mathf.Max(Text.CalcHeight(failReason, confirmButton.width - 10f) + 10f, 50f);
                Widgets.DrawHighlight(confirmButton);
                Widgets.Label(confirmButton.ContractedBy(5f), failReason);
            }

            // revert back to default for consistency
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            Widgets.EndGroup();
        }

        public void CompleteClassChange()
        {
            ProgressDiffLog diffLog = progressTracker.progressDiffLog;
            diffLog.AdjustClass(progressTracker, currClassChangeOption.classDef, currClassChangeOption.levelReset, currClassChangeOption.benefitReset);
            CompletionLearningUnlock(currClassChangeOption.classDef);
            progressTracker.ResetLevelLabel();

            foreach (var et in progressTracker.schema.energyTrackers) et.ForceClearEnergyStatCaches();

            currClassChangeOption = null;
            progressTracker.CleanClassChangeOpps();
        }

        private float RightConfirmHeightAdj => rightConfirmHeight + 10f;

        private float RightDebugHeightAdj => rightDebugHeight == 0 ? rightDebugHeight : rightDebugHeight + 10f;

        private float DrawClassPrereqs(Rect rect, ProgressTrackerClassDef classDef)
        {
            // account for prohibit and normal prereqs
            if (classDef.prereqClassesProhibit.NullOrEmpty() && classDef.prereqNodesProhibit.NullOrEmpty()
                && classDef.prereqResearchProhibit.NullOrEmpty() && classDef.prereqGenesProhibit.NullOrEmpty()
                && classDef.prereqTraitsProhibit.NullOrEmpty() && classDef.prereqXenotypeProhibit is null
                && classDef.prereqLevelProhibit <= 0 && classDef.prereqAgeProhibit <= 0
                && classDef.prereqStatsProhibit.NullOrEmpty() && classDef.prereqSkillsProhibit.NullOrEmpty()
                && classDef.prereqHediffsProhibit.NullOrEmpty() &&
                classDef.prereqClasses.NullOrEmpty() && classDef.prereqNodes.NullOrEmpty() 
                && classDef.prereqResearch.NullOrEmpty() && classDef.prereqGenes.NullOrEmpty() 
                && classDef.prereqTraits.NullOrEmpty() && classDef.prereqXenotype is null 
                && classDef.prereqLevel <= 0 && classDef.prereqAge <= 0
                && classDef.prereqStats.NullOrEmpty() && classDef.prereqSkills.NullOrEmpty() 
                && classDef.prereqHediffs.NullOrEmpty()) return 0f;
            float xMin = rect.xMin;
            float yMin = rect.yMin;

            int doneCount = -1;
            ProgressDiffLog diffLog = progressTracker.progressDiffLog;

            // PROHIBIT prereqs

            if (!classDef.prereqClassesProhibit.NullOrEmpty()) // show completed prior classes
            {
                HashSet<ProgressTrackerClassDef> classesDone = progressTracker.progressDiffLog.GetClassSet;
                doneCount = PrereqUtility.PrereqsDoneCount(classesDone, classDef.prereqClassesProhibit);
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangePrereqsProhibit".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqClassModeProhibit, classDef.prereqClassModeMinProhibit, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqClassesProhibit)
                {
                    PrereqUtility.SetPrereqStatusColor(!classesDone.Contains(prereq));
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                rect.xMin = xMin;
                GUI.color = Color.white;
            }

            if (!classDef.prereqNodesProhibit.NullOrEmpty())
            {
                LearningNodeRecord LearningRecord = progressTracker.schema.learningNodeRecord; // reuse old methods here
                HashSet<LearningTreeNodeDef> nodesDone = LearningRecord.completion.Where(x => x.Value == true).Select(x => x.Key).ToHashSet();
                doneCount = PrereqUtility.PrereqsDoneCount(nodesDone, classDef.prereqNodesProhibit);
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqNodeProhibit".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqNodeModeProhibit, classDef.prereqNodeModeMinProhibit, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqNodesProhibit)
                {
                    PrereqUtility.SetPrereqStatusColor(!LearningRecord.completion[prereq]);
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                rect.xMin = xMin;
                GUI.color = Color.white;
            }

            if (!classDef.prereqResearchProhibit.NullOrEmpty()) // show completed research
            {
                doneCount = classDef.prereqResearch.Where(x => x.PrerequisitesCompleted).Count();
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqResearchProhibit".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqResearchModeProhibit, classDef.prereqResearchModeMinProhibit, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqResearchProhibit)
                {
                    PrereqUtility.SetPrereqStatusColor(!prereq.IsFinished);
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqGenesProhibit.NullOrEmpty()) // show completed genes
            {
                HashSet<GeneDef> genesDone = progressTracker.pawn.genes.GenesListForReading.Select(x => x.def).ToHashSet();
                doneCount = PrereqUtility.PrereqsDoneCount(genesDone, classDef.prereqGenesProhibit);
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqGeneProhibit".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqGeneModeProhibit, classDef.prereqGeneModeMinProhibit, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqGenesProhibit)
                {
                    PrereqUtility.SetPrereqStatusColor(!genesDone.Contains(prereq));
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqTraitsProhibit.NullOrEmpty()) // show completed genes
            {
                doneCount = classDef.prereqTraitsProhibit.Where(x => x.HasTrait(progressTracker.pawn)).Count();
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqTraitProhibit".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqTraitModeProhibit, classDef.prereqTraitModeMinProhibit, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqTraitsProhibit)
                {
                    PrereqUtility.SetPrereqStatusColor(!prereq.HasTrait(progressTracker.pawn));
                    Widgets.LabelCacheHeight(ref rect, PrereqUtility.GetTraitDegreeData(prereq.def, prereq.degree).LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (classDef.prereqXenotypeProhibit != null) // show completed xenotype req
            {
                PrereqUtility.SetPrereqStatusColor(!diffLog.PrereqXenotypeFufilled(progressTracker, classDef));
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqXenotypeProhibit".Translate(classDef.prereqXenotypeProhibit), true, false);
                rect.yMin += rect.height;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (classDef.prereqLevelProhibit > 0) // show completed level req
            {
                PrereqUtility.SetPrereqStatusColor(!diffLog.PrereqLevelFulfilled(progressTracker, classDef));
                string levelComp = "level" + PrereqUtility.PrereqsStatsModeNotif(classDef.prereqLevelModeProhibit) + classDef.prereqLevelProhibit.ToString();
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqLevelProhibit".Translate(levelComp), true, false);
                rect.yMin += rect.height;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (classDef.prereqAgeProhibit > 0) // show completed age req
            {
                PrereqUtility.SetPrereqStatusColor(!diffLog.PrereqAgeFulfilled(progressTracker, classDef));
                string ageCheck = classDef.prereqCheckBioAgeProhibit ? "ISF_GeneralDialogPrereqAgeBioProhibit".Translate() : "ISF_GeneralDialogPrereqAgeChronProhibit".Translate();
                string ageComp = ageCheck + PrereqUtility.PrereqsStatsModeNotif(classDef.prereqAgeModeProhibit) + classDef.prereqAgeProhibit.ToString();
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqAgeProhibit".Translate(ageComp), true, false);
                rect.yMin += rect.height;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqStatsProhibit.NullOrEmpty()) // show completed stat req
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqStatProhibit".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsStatCase in classDef.prereqStatsProhibit)
                {
                    foreach (var statMod in prereqsStatCase.statReqs)
                    {
                        PrereqUtility.SetPrereqStatusColor(PrereqUtility.PrereqFailStatCase(progressTracker.pawn, statMod, prereqsStatCase.mode));
                        Widgets.LabelCacheHeight(ref rect, statMod.stat.LabelCap + PrereqUtility.PrereqsStatsModeNotif(prereqsStatCase.mode) +
                            statMod.stat.ValueToString(statMod.value, ToStringNumberSense.Absolute, !statMod.stat.formatString.NullOrEmpty()), true, false);
                        rect.yMin += rect.height;
                    }
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqSkillsProhibit.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqSkillProhibit".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsSkillCase in classDef.prereqSkillsProhibit)
                {
                    foreach (var skillLevel in prereqsSkillCase.skillReqs)
                    {
                        PrereqUtility.SetPrereqStatusColor(PrereqUtility.PrereqFailSkillCase(progressTracker.pawn, skillLevel.skillDef, skillLevel.ClampedLevel, prereqsSkillCase.mode));
                        Widgets.LabelCacheHeight(ref rect, skillLevel.skillDef.LabelCap + PrereqUtility.PrereqsStatsModeNotif(prereqsSkillCase.mode) +
                            skillLevel.ClampedLevel, true, false);
                        rect.yMin += rect.height;
                    }
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqHediffsProhibit.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqHediffProhibit".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                Hediff hediff;
                String reqLabel;
                foreach (var prereq in classDef.prereqHediffsProhibit)
                {
                    hediff = progressTracker.pawn.health.hediffSet.GetFirstHediffOfDef(prereq.Key);
                    PrereqUtility.SetPrereqStatusColor(!(hediff != null && hediff.Severity >= prereq.Value));
                    reqLabel = !prereq.Key.stages.NullOrEmpty() ?
                        prereq.Key.stages[prereq.Key.StageAtSeverity(prereq.Value)].label : prereq.Value.ToString("F0");
                    Widgets.LabelCacheHeight(ref rect, prereq.Key.LabelCap + " ({0})".Translate(reqLabel), true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            // NORMAL prereqs

            if (!classDef.prereqClasses.NullOrEmpty()) // show completed prior classes
            {
                HashSet<ProgressTrackerClassDef> classesDone = progressTracker.progressDiffLog.GetClassSet;
                doneCount = PrereqUtility.PrereqsDoneCount(classesDone, classDef.prereqClasses);
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangePrereqs".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqClassMode, classDef.prereqClassModeMin, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqClasses)
                {
                    PrereqUtility.SetPrereqStatusColor(classesDone.Contains(prereq));
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                rect.xMin = xMin;
                GUI.color = Color.white;
            }

            if (!classDef.prereqNodes.NullOrEmpty())
            {
                LearningNodeRecord LearningRecord = progressTracker.schema.learningNodeRecord; // reuse old methods here
                HashSet<LearningTreeNodeDef> nodesDone = LearningRecord.completion.Where(x => x.Value == true).Select(x => x.Key).ToHashSet();
                doneCount = PrereqUtility.PrereqsDoneCount(nodesDone, classDef.prereqNodes);
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqNode".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqNodeMode, classDef.prereqNodeModeMin, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqNodes)
                {
                    PrereqUtility.SetPrereqStatusColor(LearningRecord.completion[prereq]);
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                rect.xMin = xMin;
                GUI.color = Color.white;
            }

            if (!classDef.prereqResearch.NullOrEmpty()) // show completed research
            {
                doneCount = classDef.prereqResearch.Where(x => x.PrerequisitesCompleted).Count();
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqResearch".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqResearchMode, classDef.prereqResearchModeMin, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqResearch)
                {
                    PrereqUtility.SetPrereqStatusColor(prereq.IsFinished);
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqGenes.NullOrEmpty()) // show completed genes
            {
                HashSet<GeneDef> genesDone = progressTracker.pawn.genes.GenesListForReading.Select(x => x.def).ToHashSet();
                doneCount = PrereqUtility.PrereqsDoneCount(genesDone, classDef.prereqGenes);
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqGene".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqGeneMode, classDef.prereqGeneModeMin, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqGenes)
                {
                    PrereqUtility.SetPrereqStatusColor(genesDone.Contains(prereq));
                    Widgets.LabelCacheHeight(ref rect, prereq.LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqTraits.NullOrEmpty()) // show completed genes
            {               
                doneCount = classDef.prereqTraits.Where(x => x.HasTrait(progressTracker.pawn)).Count();
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqTrait".Translate() + PrereqUtility.PrereqsModeNotif(classDef.prereqTraitMode, classDef.prereqTraitModeMin, doneCount), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereq in classDef.prereqTraits)
                {
                    PrereqUtility.SetPrereqStatusColor(prereq.HasTrait(progressTracker.pawn));
                    Widgets.LabelCacheHeight(ref rect, PrereqUtility.GetTraitDegreeData(prereq.def, prereq.degree).LabelCap, true, false);
                    rect.yMin += rect.height;
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (classDef.prereqXenotype != null) // show completed xenotype req
            {
                PrereqUtility.SetPrereqStatusColor(diffLog.PrereqXenotypeFufilled(progressTracker, classDef));
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqXenotype".Translate(classDef.prereqXenotype), true, false);
                rect.yMin += rect.height;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (classDef.prereqLevel > 0) // show completed level req
            {
                PrereqUtility.SetPrereqStatusColor(diffLog.PrereqLevelFulfilled(progressTracker, classDef));
                string levelComp = "level" + PrereqUtility.PrereqsStatsModeNotif(classDef.prereqLevelMode) + classDef.prereqLevel.ToString();
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqLevel".Translate(levelComp), true, false);
                rect.yMin += rect.height;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (classDef.prereqAge > 0) // show completed age req
            {
                PrereqUtility.SetPrereqStatusColor(diffLog.PrereqAgeFulfilled(progressTracker, classDef));
                string ageCheck = classDef.prereqCheckBioAge ? "ISF_GeneralDialogPrereqAgeBio".Translate() : "ISF_GeneralDialogPrereqAgeChron".Translate();
                string ageComp = ageCheck + PrereqUtility.PrereqsStatsModeNotif(classDef.prereqAgeMode) + classDef.prereqAge.ToString();
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqAge".Translate(ageComp), true, false);
                rect.yMin += rect.height;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqStats.NullOrEmpty()) // show completed stat req
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqStat".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsStatCase in classDef.prereqStats)
                {
                    foreach (var statMod in prereqsStatCase.statReqs)
                    {
                        PrereqUtility.SetPrereqStatusColor(!PrereqUtility.PrereqFailStatCase(progressTracker.pawn, statMod, prereqsStatCase.mode));
                        Widgets.LabelCacheHeight(ref rect, statMod.stat.LabelCap + PrereqUtility.PrereqsStatsModeNotif(prereqsStatCase.mode) +
                            statMod.stat.ValueToString(statMod.value, ToStringNumberSense.Absolute, !statMod.stat.formatString.NullOrEmpty()), true, false);
                        rect.yMin += rect.height;
                    }
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqSkills.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqSkill".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (var prereqsSkillCase in classDef.prereqSkills)
                {
                    foreach (var skillLevel in prereqsSkillCase.skillReqs)
                    {
                        PrereqUtility.SetPrereqStatusColor(!PrereqUtility.PrereqFailSkillCase(progressTracker.pawn, skillLevel.skillDef, skillLevel.ClampedLevel, prereqsSkillCase.mode));
                        Widgets.LabelCacheHeight(ref rect, skillLevel.skillDef.LabelCap + PrereqUtility.PrereqsStatsModeNotif(prereqsSkillCase.mode) +
                            skillLevel.ClampedLevel, true, false);
                        rect.yMin += rect.height;
                    }
                }
                GUI.color = Color.white;
                rect.xMin = xMin;
            }

            if (!classDef.prereqHediffs.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "ISF_GeneralDialogPrereqHediff".Translate(), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                Hediff hediff;
                String reqLabel;
                foreach (var prereq in classDef.prereqHediffs)
                {
                    hediff = progressTracker.pawn.health.hediffSet.GetFirstHediffOfDef(prereq.Key);
                    PrereqUtility.SetPrereqStatusColor((hediff != null && hediff.Severity >= prereq.Value));
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

        public string TipStringExtra(LearningTreeNodeDef node)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StatDrawEntry statDrawEntry in node.SpecialDisplayMods())
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
