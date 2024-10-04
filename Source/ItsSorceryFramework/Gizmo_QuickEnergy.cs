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
/*			foreach(var et in tracker.quickEnergyTrackers)
            {
				et.DrawEnergyBar
            }*/
			
			if(tracker is null) return new GizmoResult(GizmoState.Clear);

			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Widgets.DrawWindowBackground(rect);
			Widgets.Label(rect, "{0}/5".Translate(tracker.quickEnergyEntries.Count));

			return new GizmoResult(GizmoState.Clear);
		}

		public override float GetWidth(float maxWidth) => 212f;

	}
}
