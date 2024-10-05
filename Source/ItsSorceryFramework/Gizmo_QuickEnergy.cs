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
    [StaticConstructorOnStartup]
    public class Gizmo_QuickEnergy : Gizmo
    {
		public Pawn_SorcerySchemaTracker tracker;

		public List<EnergyTracker> energyTrackers = new List<EnergyTracker>();

		private float gizmoHeight = 75f;

		private const float gizmoBaseHeight = 75f;

		public Gizmo_QuickEnergy(Pawn_SorcerySchemaTracker tracker)
		{
			this.tracker = tracker;
			Order = -95f;
		}

		public void UpdateGizmo()
		{
			List<EnergyTracker> updated_list = new List<EnergyTracker>();
			foreach (var e in tracker.quickEnergyEntries){
				updated_list.Add(SorcerySchemaUtility.GetEnergyTracker(tracker, e.sorcerySchemaDef, e.energyTrackerDef));
			}

			energyTrackers = updated_list;
		}


		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			if (tracker is null) return new GizmoResult(GizmoState.Clear);

			// if the energyTrackers are empty, and the energy entries are not, update gizmo
			if (energyTrackers.NullOrEmpty() && !tracker.quickEnergyEntries.NullOrEmpty()) UpdateGizmo();

			// draw background rect
			float finalHeight = 0f;

			//Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), gizmoHeight);
			
			// create adjusted rect
			float heightAdj = Math.Max(gizmoBaseHeight, gizmoHeight);
			float yAdj = Math.Max(gizmoHeight - gizmoBaseHeight, 0);
			Rect rect = new Rect(topLeft.x, topLeft.y - yAdj, GetWidth(maxWidth), heightAdj);
			Widgets.DrawWindowBackground(rect);

			// draw energy bars within confines of the rect.
			float barHeight = 0;
			foreach(var et in energyTrackers)
            {
				barHeight = et.DrawOnGUI(ref rect, false);
				rect.y += barHeight;
				finalHeight += barHeight;
			}
			gizmoHeight = finalHeight + 10f;


			//Widgets.Label(rect, "{0}/5".Translate(tracker.quickEnergyEntries.Count));

			return new GizmoResult(GizmoState.Clear);
		}

		public override float GetWidth(float maxWidth) => 350f;

	}
}
