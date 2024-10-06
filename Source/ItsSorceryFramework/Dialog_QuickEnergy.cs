using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace ItsSorceryFramework
{
	[StaticConstructorOnStartup]
	public class Dialog_QuickEnergy : Window
	{

		/*public List<Sorcery> allSorcery;

		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;*/

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

            if (energyTrackers.NullOrEmpty())
            {
				windowRect.height = windowBaseHeight;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(inRect, "ISF_CommandQuickEnergyGizmoNoTrackers".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				return;
			}

			// draw background rect
			float finalHeight = 0f;
			float heightAdj = Math.Max(windowBaseHeight, windowHeight);
			//float yAdj = Math.Max(windowHeight - windowBaseHeight, 0);
			Rect rect = new Rect(inRect.x, inRect.y, inRect.width, heightAdj);
			Widgets.DrawWindowBackground(rect);

			// draw energy bars within confines of the rect.
			float barHeight = 0;
			foreach (var et in energyTrackers)
			{
				barHeight = et.DrawOnGUI(ref rect, false);
				rect.y += barHeight;
				finalHeight += barHeight;
			}
			windowHeight = finalHeight + 10f; //10f for buffer, -1 for boundary.

			windowRect.height = windowHeight;

		}

	}
}
