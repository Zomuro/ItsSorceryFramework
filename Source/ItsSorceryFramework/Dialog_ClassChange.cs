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
            float outRectHeight = rect.height - rightDebugHeightAdj - rightConfirmHeightAdj; // account for debug & confirm buttons
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
            Rect confirmButton = new Rect(0, outRectHeight + rightDebugHeightAdj, outRect.width, rightConfirmHeight);
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
                string failReason = "ISF_ClassChangeLocked".Translate() + "\n" + reason;
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

            foreach (var et in progressTracker.schema.energyTrackers) et.ClearStatCache();

            currClassChangeOption = null;
            progressTracker.CleanClassChangeOpps();
        }

        private float rightConfirmHeightAdj => rightConfirmHeight + 10f;

        private float rightDebugHeightAdj => rightDebugHeight == 0 ? rightDebugHeight : rightDebugHeight + 10f;

        private float DrawClassPrereqs(Rect rect, ProgressTrackerClassDef classDef)
        {
            if (classDef.prereqsClassDefs.NullOrEmpty() && classDef.prereqsResearch.NullOrEmpty() && classDef.prereqLevel <= 0 && classDef.prereqsStats.NullOrEmpty() &&
                classDef.prereqsSkills.NullOrEmpty() && classDef.prereqsHediff.NullOrEmpty()) return 0f;
            float xMin = rect.xMin;
            float yMin = rect.yMin;

            LearningNodeRecord LearningRecord = progressTracker.schema.learningNodeRecord; // reuse old methods here
            Tuple<int, int> prereqsDone = PrereqsDone(classDef);
            ProgressDiffLog diffLog = progressTracker.progressDiffLog;

            if (!classDef.prereqsClassDefs.NullOrEmpty()) // show completed prior classes
            {
                HashSet<ProgressTrackerClassDef> classesDone = progressTracker.progressDiffLog.GetClassSet;
                Widgets.LabelCacheHeight(ref rect, "ISF_ClassChangePrereqs".Translate() + LearningRecord.PrereqsModeNotif(classDef.prereqClassMode, classDef.prereqClassModeMin, prereqsDone.Item1), true, false);
                rect.yMin += rect.height;
                rect.xMin += 6f;
                foreach (ProgressTrackerClassDef prereqClass in classDef.prereqsClassDefs)
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
                SetPrereqStatusColor(diffLog.PrereqLevelFulfilled(progressTracker, classDef));
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
