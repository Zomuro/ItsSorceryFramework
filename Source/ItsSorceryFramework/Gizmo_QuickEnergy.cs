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
		public Gizmo_QuickEnergy()
		{
			
		}


		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			
			return new GizmoResult(GizmoState.Clear);
		}

		public override float GetWidth(float maxWidth) => 212f;

	}
}
