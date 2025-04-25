using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace ItsSorceryFramework
{
    [StaticConstructorOnStartup]
	public class Dialog_QuickEnergy : Window
	{
		public Pawn_SorcerySchemaTracker tracker;

		public List<EnergyTracker> energyTrackers = new List<EnergyTracker>();

		private float windowHeight = 75f;

		private const float windowBaseHeight = 75f;

		public override Vector2 InitialSize => new Vector2(300, windowBaseHeight);

		protected override float Margin => 0f; // no margin - built into the bars themselves.

		public override bool IsDebug => false;

		public Dialog_QuickEnergy(Pawn_SorcerySchemaTracker tracker) : base()
		{
			this.tracker = tracker;
			draggable = true;
			drawShadow = false;
			closeOnClickedOutside = false;
			forcePause = false;
			closeOnCancel = false;
			preventCameraMotion = false;
			doCloseX = true; // set to false
		}

		public override void PostOpen()
		{
			base.PostOpen(); // make window opening sounds
			UpdateEnergy(); // get references to relevant energytrackers
		}

		public void UpdateEnergy()
		{
			List<EnergyTracker> updated_list = new List<EnergyTracker>();
			foreach (var e in tracker.quickEnergyEntries)
			{
				updated_list.Add(SorcerySchemaUtility.GetEnergyTracker(tracker, e.sorcerySchemaDef, e.energyTrackerDef));
			}

			energyTrackers = updated_list;
		}

		public override void DoWindowContents(Rect inRect)
		{
			// hide window if relevant pawn is not selected
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (tracker is null || pawn is null || pawn != tracker.pawn)
            {
				windowRect.height = 0f;
				return;
			}

			// no energy trackers? window should inform user they should select energy
            if (energyTrackers.NullOrEmpty())
            {
				windowRect.height = windowBaseHeight;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(inRect, "ISF_CommandQuickEnergyViewNoTrackers".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				return;
			}

			// draw background rect
			float totalBarHeight = 0f;
			float heightAdj = Math.Max(windowBaseHeight, windowHeight);
			Rect rect = new Rect(inRect.x, inRect.y, inRect.width, heightAdj);

			// make room for close x
			rect.y += 10;
			float topBuffer = 15f;
			float bottomBuffer = 10f;

			// draw energy bars within confines of the rect.
			float barHeight = 0;
			foreach (var et in energyTrackers)
			{
				barHeight = et.DrawOnGUI(ref rect, false);
				rect.y += barHeight;
				totalBarHeight += barHeight;
			}
			windowHeight = topBuffer + totalBarHeight + bottomBuffer; //15f for top buffer, total bar height for bars, 10f for bottom buffer.
			windowRect.height = windowHeight; // set to final window height

		}

	}
}
