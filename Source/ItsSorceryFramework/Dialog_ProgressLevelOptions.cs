using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;


namespace ItsSorceryFramework
{
	public class Dialog_ProgressLevelOptions : Dialog_DebugOptionListLister
	{
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(800, 600);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return false;
			}
		}

		public Dialog_ProgressLevelOptions(IEnumerable<DebugMenuOption> options, ProgressTracker_RPG progressTracker) : base(options)
		{
			//selectableCache = options.ToList();
			/*options = SelectableIncidents;*/
			tracker = progressTracker;
			closeOnClickedOutside = false;
			forcePause = true;
			closeOnCancel = false;
			doCloseX = false;
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
			filter = Widgets.TextField(new Rect(leftHalfRect.x, leftHalfRect.y, leftHalfRect.width, 30f), this.filter);
			if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.Repaint) && this.focusFilter)
			{
				GUI.FocusControl("FilterOptions");
				filter = "";
				focusFilter = false;
			}
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight = 0f;
			}

			// Setup right half (label + description + changes in stats and abilities + confirmation)
			leftHalfRect.yMin += 40f;
			Rect rightHalfRect = new Rect(leftHalfRect);
			rightHalfRect.xMin += rightHalfRect.width;

			// Title for options listing
			Text.Font = GameFont.Medium;
			Rect optionsTitleRect = leftHalfRect.ContractedBy(5f);
			optionsTitleRect.x += 5;
			Widgets.Label(optionsTitleRect, "Options");
			Text.Font = GameFont.Small;

			// Listing that shows all the options
			Rect optionsRect = new Rect(leftHalfRect).ContractedBy(10f);
			optionsRect.yMin += 30f;
			float allOptionsHeight = totalOptionsHeight;
			if (allOptionsHeight < optionsRect.height)
			{
				allOptionsHeight = optionsRect.height;
			}
			Rect allOptionsRect = new Rect(optionsRect.x, optionsRect.y, optionsRect.width - 16f, allOptionsHeight);
			Widgets.BeginScrollView(optionsRect, ref scrollPosition, allOptionsRect, true);
			listing = new Listing_Standard(inRect, () => scrollPosition);
			listing.ColumnWidth = (optionsRect.width - 16f);
			listing.Begin(allOptionsRect);
			DoListingItems();
			listing.End();
			Widgets.EndScrollView();

			// Plotting out info on options
			if (selectedOption is null) return;

			// reuse code from learning tracker tree to include info on stat, hediff, and ability changes

		}

		protected override void DoListingItems()
		{
			if (KeyBindingDefOf.Dev_ChangeSelectedDebugAction.IsDownEvent)
			{
				this.ChangeHighlightedOption();
			}
			int highlightedIndex = this.HighlightedIndex;
			for (int i = 0; i < this.options.Count; i++)
			{
				DebugMenuOption debugMenuOption = this.options[i];
				bool highlight = highlightedIndex == i;
				if (debugMenuOption.mode == DebugMenuOptionMode.Action)
				{
					DebugAction(debugMenuOption.label, debugMenuOption.method, highlight);
				}
				if (debugMenuOption.mode == DebugMenuOptionMode.Tool)
				{
					base.DebugToolMap(debugMenuOption.label, debugMenuOption.method, highlight);
				}
			}
		}

		private ProgressLevelOption selectedOption;

		private bool focusFilter;

		protected Vector2 scrollPositionOptions;

		private float optionHeight;

		protected Vector2 scrollPositionRight;

		private float infoHeight;

		public ProgressTracker_RPG tracker;
	}
}
