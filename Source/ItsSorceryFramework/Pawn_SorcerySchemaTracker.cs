using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class Pawn_SorcerySchemaTracker : IExposable
    {
        public Pawn pawn;

        public List<SorcerySchema> sorcerySchemas = new List<SorcerySchema>();

        public List<GizmoEntry_QuickEnergy> quickEnergyEntries = new List<GizmoEntry_QuickEnergy>();

        private Gizmo gizmo;

        private Window window;

        public Pawn_SorcerySchemaTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void SchemaTrackerTick()
        {
            if (sorcerySchemas.NullOrEmpty()) return;

            foreach (SorcerySchema schema in sorcerySchemas)
            {
                schema.SchemaTick();
            }
        }

        public void UpdateGizmo()
        {
            // only visually update the gizmo/window when the window is open
            if(window != null) (window as Dialog_QuickEnergy).UpdateEnergy();
        }

        public bool ShowGizmo() => true; // always show gizmo

        public Gizmo GetGizmo()
        {
            if (gizmo == null)
            {
                gizmo = new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/ISF_QuickEnergy", true), // get custom texture
                    defaultLabel = "ISF_CommandQuickEnergyViewLabel".Translate(),
                    defaultDesc = "ISF_CommandQuickEnergyViewDesc".Translate(),
                    action = delegate ()
                    {
                        if (window is null || !Find.WindowStack.IsOpen(typeof(Dialog_QuickEnergy)))
                        {
                            window = new Dialog_QuickEnergy(this); // create window
                            Find.WindowStack.Add(window); // add to window stack
                        }
                        else
                        {
                            Find.WindowStack.TryRemove(window); // remove window from stack
                            window = null; // set linked window to null
                        }
                    }
                };
            }

            return gizmo;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Collections.Look(ref sorcerySchemas, "sorcerySchemas", LookMode.Deep, new object[]
            {
                pawn
            });
            Scribe_Collections.Look(ref quickEnergyEntries, "quickEnergyEntries", LookMode.Deep, new object[] { });

            // save this concept for later - prevent a schema from being learned if another is there
            // maybe even anti psycast option
            //Scribe_Collections.Look(ref incompatibleSchemas, "incompatibleSchemas", LookMode.Deep, LookMode.Deep);
        }
    }

}


