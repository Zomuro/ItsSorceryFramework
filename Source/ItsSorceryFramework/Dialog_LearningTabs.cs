﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class Dialog_LearningTabs : Window
    {
		public LearningTracker CurTracker
		{
			get
			{
				return curTracker;
			}
			set
			{
				if (value == curTracker)
				{
					return;
				}
				curTracker = value;
			}
		}

		public LearningTabRecord CurTabRecord
		{
			get
			{
				foreach (LearningTabRecord tab in tabs)
				{
					if (tab.tracker == CurTracker)
					{
						return tab;
					}
				}
				return null;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(UI.screenWidth, UI.screenHeight * 3f / 4f);
			}
		}

		public Dialog_LearningTabs(List<LearningTracker> trackers)
		{
			learningTrackers = trackers;
			forcePause = true;
			resizeable = true;
			draggable = true;
			closeOnClickedOutside = true;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.tabs.Clear();
			foreach (LearningTracker lt in learningTrackers)
			{
				this.tabs.Add(new LearningTabRecord(lt, lt.def.LabelCap, delegate ()
				{
					this.CurTracker = lt;
				}, () => this.CurTracker == lt));
			}

			if (this.CurTracker == null) curTracker = tabs[0].tracker;		
		}

		public override void PostOpen()
		{
			base.PostOpen();
		}

		public override void DoWindowContents(Rect inRect)
        {
			//this.windowRect.width = (float)UI.screenWidth;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			float width = Mathf.Max(200f, inRect.width * 0.22f);
			Rect leftOutRect = new Rect(0f, 0f, width, inRect.height - 24f - 10f);
			Rect searchRect = new Rect(0f, leftOutRect.yMax + 10f, width, 24f);
			Rect rightOutRect = new Rect(leftOutRect.xMax + 10f, 0f, inRect.width - leftOutRect.width - 10f, inRect.height);
			DrawLeftRect(leftOutRect);
			DrawRightRect(rightOutRect);
		}

		private void DrawLeftRect(Rect leftOutRect)
		{
			if (CurTabRecord != null) CurTabRecord.tracker.DrawLeftGUI(leftOutRect);
		}

		private void DrawRightRect(Rect rightOutRect)
		{
			rightOutRect.yMin += 32f;
			Widgets.DrawMenuSection(rightOutRect);
			TabDrawer.DrawTabs<LearningTabRecord>(rightOutRect, this.tabs, 200f);
			if(CurTabRecord != null) CurTabRecord.tracker.DrawRightGUI(rightOutRect);
			
		}

		public LearningTracker curTracker;

		public List<LearningTracker> learningTrackers;

		public List<LearningTabRecord> tabs = new List<LearningTabRecord>();

		public class LearningTabRecord : TabRecord
		{
			public LearningTabRecord(LearningTracker tracker, string label, Action clickedAction, Func<bool> selected) : base(label, clickedAction, selected)
			{
				this.tracker = tracker;
			}

			public readonly LearningTracker tracker;
		}
	}
}
