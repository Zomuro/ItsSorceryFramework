using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;


namespace ItsSorceryFramework
{
    public class Dialog_ProgressLevelOptions : Dialog_DebugOptionListLister
	{
		public ProgressLevelOption selectedOption;

		private bool focusFilter;

		protected Vector2 scrollPositionOption;

		private float optionViewHeight;

		public ProgressTracker tracker;

		public int setLevel = -1;

		public override Vector2 InitialSize => new Vector2(800, 600);

		public override bool IsDebug => false;

		public Dialog_ProgressLevelOptions(IEnumerable<DebugMenuOption> options, ProgressTracker progressTracker, int level) : base(options)
		{
			tracker = progressTracker;
			closeOnClickedOutside = false;
			forcePause = true;
			closeOnCancel = false;
			doCloseX = Prefs.DevMode; // true if in dev mode; false otherwise
			onlyOneOfTypeAllowed = false;
			setLevel = level;
		}

		protected override int HighlightedIndex
		{
			get
			{
				if (options.NullOrEmpty<DebugMenuOption>()) return base.HighlightedIndex;	
				if (FilterAllows(options[prioritizedHighlightedIndex].label)) return prioritizedHighlightedIndex;
				if (filter.NullOrEmpty()) return -1;
				for (int i = 0; i < options.Count; i++)
				{
					if (FilterAllows(options[i].label))
					{
						currentHighlightIndex = i;
						break;
					}
				}
				return currentHighlightIndex;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			// Setup left half (used for listings + filter)
			Rect leftHalfRect = new Rect(inRect);
			leftHalfRect.width = inRect.width / 2;

			// Filter section (taken from Dialog_DebugOptionListLister)
			GUI.SetNextControlName("FilterOptions");
			if (Event.current.type == EventType.KeyDown && (KeyBindingDefOf.Dev_ToggleDebugSettingsMenu.KeyDownEvent || KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent))
			{
				return;
			}
			filter = Widgets.TextField(new Rect(leftHalfRect.x, leftHalfRect.y, leftHalfRect.width / 2, 30f), filter);
			if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.Repaint) && focusFilter)
			{
				GUI.FocusControl("FilterOptions");
				filter = "";
				focusFilter = false;
			}
			if (Event.current.type == EventType.Layout) totalOptionsHeight = 0f;

			leftHalfRect.yMin += 40f;
			
			// Setup right half rect
			Rect rightHalfRect = new Rect(leftHalfRect);
			rightHalfRect.x = inRect.x + inRect.width / 2;

			// Title for options listing
			Text.Font = GameFont.Medium;
			Rect titleLeftHalfRect = new Rect(leftHalfRect);
			Widgets.LabelCacheHeight(ref titleLeftHalfRect, "ISF_ProgressLevelOptionsLabel".Translate(tracker.schema.def.LabelCap, setLevel));
			leftHalfRect.yMin += titleLeftHalfRect.height;
			Text.Font = GameFont.Small;

			// Setup right half (label + description + changes in stats and abilities + confirmation)
			float allOptionsHeight = totalOptionsHeight;
			if (allOptionsHeight < leftHalfRect.height) allOptionsHeight = leftHalfRect.height;

			curX = 0f;
			curY = leftHalfRect.y;

			Rect allOptionsRect = new Rect(leftHalfRect.x, leftHalfRect.y, leftHalfRect.width - 20f, allOptionsHeight);
			float columnWidth = (leftHalfRect.width - 20f);
			Widgets.BeginScrollView(leftHalfRect, ref scrollPosition, allOptionsRect, true);		
			DoListingItems(allOptionsRect, columnWidth);
			Widgets.EndScrollView();

			// seperator line between left and right
			Widgets.DrawLine(new Vector2(rightHalfRect.x, rightHalfRect.y), new Vector2(rightHalfRect.x, rightHalfRect.yMax), Color.grey, 1);

			// dividing up right side into view of options + confirmation area
			Rect selectDetailRect = new Rect(rightHalfRect.ContractedBy(10f));

			// draw background menu area
			Widgets.DrawMenuSection(selectDetailRect);

			// Plotting out info on options
			// skip it all if no option is selected
			if (selectedOption != null)
            {
				// show information on option effect
				Rect detailRect = new Rect(selectDetailRect.x, selectDetailRect.y, selectDetailRect.width, selectDetailRect.height * 4f / 5f).ContractedBy(10f);
				DrawOptionDetails(detailRect);

				// draw confirmation button
				Rect confirmRect = new Rect(selectDetailRect.x + selectDetailRect.width/4f, selectDetailRect.y + selectDetailRect.height * 17f/20f, 
					selectDetailRect.width / 2f, selectDetailRect.width / 10f);
				
				//Rect confirmButtonRect = new Rect(confirmRect.x + confirmRect.width /4, confirmRect.y + confirmRect.height / 4,  confirmRect.width / 2, confirmRect.height / 2);
				if(Widgets.ButtonText(confirmRect, "ISF_ProgressLevelOptionsConfirm".Translate())) // if hit w/ a selected option, adjust the following
                {
					tracker.AdjustModifiers(selectedOption);
					tracker.AdjustAbilities(selectedOption);
					tracker.AdjustHediffs(selectedOption);
					tracker.points += selectedOption.pointGain;
					tracker.Hediff.cachedCurStage = tracker.RefreshCurStage();

					if (Prefs.DevMode && ItsSorceryUtility.settings.ShowItsSorceryDebug)
					{
						Log.Message($"{tracker.schema.def.defName}.{tracker.def.defName}:" +
						$"\nProgressTracker offets: {tracker.statOffsetsTotal.ToStringSafeEnumerable()}" +
						$"\nProgressTracker factors: {tracker.statFactorsTotal.ToStringSafeEnumerable()}" +
						$"\nProgressTracker cap mods: {tracker.capModsTotal.ToStringSafeEnumerable()}" +
						$"\nHediff ProgressTracker offets: {tracker.Hediff.Schema.progressTracker.statOffsetsTotal.ToStringSafeEnumerable()}" +
						$"\nHediff ProgressTracker factors: {tracker.Hediff.Schema.progressTracker.statFactorsTotal.ToStringSafeEnumerable()}" +
						$"\nHediff ProgressTracker cap mods: {tracker.Hediff.Schema.progressTracker.capModsTotal.ToStringSafeEnumerable()}");
					}

					Close();
				}
			}
            else
            {
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(selectDetailRect, "ISF_ProgressLevelOptionsNone".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
			}
		}

		// shows the stat change + hediff and ability + skill point changes for each result
		public void DrawOptionDetails(Rect rect)
        {
			Widgets.BeginGroup(rect);
			Rect baseRect = new Rect(0, 0, rect.width, rect.height);
			float coordY = 0f;
			Rect optionRectView = new Rect(baseRect.x, baseRect.y, baseRect.width - 20f, optionViewHeight);
			Widgets.BeginScrollView(baseRect, ref scrollPositionOption, optionRectView, true);

			// Title for option
			Text.Font = GameFont.Medium;
			Rect optionTitleRect = new Rect(optionRectView);
			Widgets.LabelCacheHeight(ref optionTitleRect, selectedOption.label.CapitalizeFirst(), true, false);
			coordY += optionTitleRect.height;
			Text.Font = GameFont.Small;

			// display option stats
			Rect optionModRect = new Rect(optionRectView.x, optionRectView.y + coordY, optionRectView.width, 500f);
			coordY += DrawStatMods(optionModRect, selectedOption);

			// display option hyperlinks (abilities, hediffs)
			Rect optionHyperlinkRect = new Rect(optionRectView.x, optionRectView.y + coordY, optionRectView.width, 500f);
			coordY += DrawHyperlinks(optionHyperlinkRect, selectedOption);

            // display description
            if (!selectedOption.description.NullOrEmpty())
            {
				Rect optionDescRect = new Rect(optionRectView.x, optionRectView.y + coordY, optionRectView.width, 500f);
				Widgets.LabelCacheHeight(ref optionDescRect, selectedOption.description, true, false);
				coordY += optionDescRect.height;
			}

			optionViewHeight = coordY;
			Widgets.EndScrollView();
			Widgets.EndGroup();
        }

		public float DrawStatMods(Rect rect, ProgressLevelOption option)
		{
			float yMin = rect.yMin;

			String tipString = TipStringExtra(option);
			if (!tipString.NullOrEmpty())
			{
				Widgets.LabelCacheHeight(ref rect, "ISF_LearningNodeMods".Translate(), true, false);
				rect.yMin += rect.height;
				Widgets.LabelCacheHeight(ref rect, tipString, true, false);
				rect.yMin += rect.height;
			}

			return rect.yMin - yMin;
		}

		public string TipStringExtra(ProgressLevelOption option)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (StatDrawEntry statDrawEntry in option.SpecialDisplayMods())
			{
				if (statDrawEntry.ShouldDisplay())
				{
					stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
				}
			}
			if (option.pointGain > 0) stringBuilder.AppendInNewLine("  - " + tracker.def.skillPointLabelKey.Translate().CapitalizeFirst() + ": " +
				 option.pointGain);
			return stringBuilder.ToString();
		}

		public float DrawHyperlinks(Rect rect, ProgressLevelOption option)
		{
			List<AbilityDef> abilityGain = option.abilityGain;
			List<AbilityDef> abilityRemove = option.abilityRemove;
			List<NodeHediffProps> hediffAdd = option.hediffAdd;
			List<NodeHediffProps> hediffAdjust = option.hediffAdjust;
			List<HediffDef> hediffRemove = option.hediffRemove;

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

		// adjusted so that clicking on an option does not close the dialog window
		protected override void DoListingItems(Rect inRect, float columnWidth)
		{
			if (KeyBindingDefOf.Dev_ChangeSelectedDebugAction.IsDownEvent) ChangeHighlightedOption();
			int highlightedIndex = HighlightedIndex;
			for (int i = 0; i < options.Count; i++)
			{
				DebugMenuOption debugMenuOption = options[i];
				bool highlight = highlightedIndex == i;
				if (debugMenuOption.mode == DebugMenuOptionMode.Action) DebugActionOption(debugMenuOption.label, columnWidth, debugMenuOption.method, highlight);
				if (debugMenuOption.mode == DebugMenuOptionMode.Tool) DebugToolMap(debugMenuOption.label, columnWidth, debugMenuOption.method, highlight);
			}
		}

		protected bool DebugActionOption(string label, float columnWidth, Action action, bool highlight)
		{
			bool result = false;
			if (!base.FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (this.ButtonDebug(label, columnWidth, highlight))
			{
				action();
				result = true;
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += 24f;
			}
			return result;
		}

		
	}
}
