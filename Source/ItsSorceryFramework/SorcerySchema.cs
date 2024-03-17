using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ItsSorceryFramework
{
    public class SorcerySchema : IExposable, ILoadReferenceable
    {
        public SorcerySchema(Pawn pawn)
        {
            this.pawn = pawn;
            DetermineHasLimits();
            DetermineHasTurns();
        }

        public SorcerySchema(Pawn pawn, SorcerySchemaDef def)
        {
            this.pawn = pawn;
            this.def = def;
            InitializeTrackers(); // setup energy, learning, and progress trackers
            InitializeNodeCompletion(); // setup record of learning nodes
            DetermineHasLimits();
            DetermineHasTurns();
        }

        public string GetUniqueLoadID() => "SorcerySchema_" + loadID;

        // not the thing that makes me happy, but gotta do this
        public void DetermineHasLimits()
        {
            foreach(var et in energyTrackers)
            {
                if (et.HasLimit) 
                {
                    hasLimits = true;
                    return;
                }
            }
        }

        // same boat
        public void DetermineHasTurns()
        {
            foreach (var et in energyTrackers)
            {
                if (et.HasTurn)
                {
                    hasTurns = true;
                    return;
                }
            }
        }

        public virtual void InitializeTrackers()
        {
            foreach (EnergyTrackerDef etDef in def.energyTrackerDefs)
            {
                energyTrackers.Add(Activator.CreateInstance(etDef.energyTrackerClass,
                new object[] { pawn, etDef, def }) as EnergyTracker);
            }

            foreach (LearningTrackerDef ltDef in def.learningTrackerDefs)
            {
                learningTrackers.Add(Activator.CreateInstance(ltDef.learningTrackerClass,
                    new object[] { pawn, ltDef, def }) as LearningTracker);
            }

            progressTracker = Activator.CreateInstance(def.progressTrackerDef.progressTrackerClass,
                new object[] { pawn, def }) as ProgressTracker;
        }

        public virtual void InitializeNodeCompletion()
        {
            learningNodeRecord = new LearningNodeRecord(pawn, def); //testing initalization of node completion and saving
        }

        public Dictionary<LearningTreeNodeDef, bool> NodeCompletion
        {
            get
            {
                if(learningNodeRecord is null)
                {
                    learningNodeRecord = new LearningNodeRecord(pawn, def);
                }
                return learningNodeRecord.completion;
            }
        }

        public virtual void SchemaTick()
        {
            if (!energyTrackers.NullOrEmpty()) 
            {
                foreach(var et in energyTrackers) et.EnergyTrackerTick();
            }

            if(progressTracker != null)
            {
                progressTracker.ProgressTrackerTick();
            }            
        }

        public virtual float DrawOnGUI(ref Rect rect)
        {
            // get original rect
            Rect orgRect = new Rect(rect);
            
            float coordY = 0;
            float etHeight = 0;

            // draws info, learningtracker buttons + schema title
            coordY += SchemaViewBox(ref rect);
            float buttonRefPoint = rect.x + rect.width - 5 - 24;
            if (hasLimits) // draws limit toggle button
            {
                LimitButton(buttonRefPoint, rect.y + 5);
                buttonRefPoint -= 24;
            }  
            if (hasTurns) TurnButton(buttonRefPoint, rect.y + 5); // draws turn pause toggle button if schema should be pausable

            // temporary rect for plotting energytrackers
            Rect tempRect = new Rect(rect);
            tempRect.y += coordY;

            // draw out all energytrackers
            foreach (var et in energyTrackers)
            {
                etHeight = et.DrawOnGUI(ref tempRect);
                coordY += etHeight;
                tempRect.y += etHeight;
            }

            coordY += 10;
            rect.height = coordY;
            // draw outline of the entire rectangle when it's all done
            DrawOutline(rect, Color.grey, 1);
            // reset rect
            rect.height = orgRect.height;
            // return accumulated height
            return coordY;
        }

        public float SchemaViewBox(ref Rect rect)
        {
            //float yMin = rect.yMin;
            float coordY = 0f;

            // information button- shows important info about the sorcery schema
            def.TempPawn = pawn;

            Widgets.InfoCardButton(rect.x + 5, rect.y + 5, def);
            LearningTrackerButton(rect.x + 5 + 24, rect.y + 5);
            FavButton(rect.x + 5 + 48, rect.y + 5);

            def.ClearCachedData();

            // shows the label of the sorcery schema in the itab
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;

            Rect titleRect = new Rect(rect.x + rect.width / 5, rect.y, rect.width * 3 / 5, 50f);
            Widgets.LabelCacheHeight(ref titleRect, def.LabelCap.ToString());
            coordY += titleRect.height;

            Text.Anchor = TextAnchor.UpperLeft;

            return coordY;
        }

        public bool LearningTrackerButton(float x, float y)
        {
            if (learningTrackers.NullOrEmpty()) return false;
            Rect rect = new Rect(x, y, 24f, 24f);
            MouseoverSounds.DoRegion(rect);
            TooltipHandler.TipRegionByKey(rect, "ISF_ButtonLearningTrackers");

            if (Widgets.ButtonImage(rect, TexButton.ToggleLog, GUI.color, true))
            {
                Find.TickManager.Pause();
                Find.WindowStack.Add(new Dialog_LearningTabs(learningTrackers));
                return true;
            }
            return false;
        }

        public bool FavButton(float x, float y)
        {
            Rect rect = new Rect(x, y, 24f, 24f);
            MouseoverSounds.DoRegion(rect);
            TooltipHandler.TipRegionByKey(rect, "ISF_ButtonFav");

            if (Widgets.ButtonImage(rect, favorited ? GizmoTextureUtility.StarFull : GizmoTextureUtility.StarEmpty, GUI.color, true))
            {
                favorited = !favorited;
                return true;
            }
            return false;
        }

        public bool LimitButton(float x, float y)
        {
            Rect rect = new Rect(x, y, 24f, 24f);
            MouseoverSounds.DoRegion(rect);
            TooltipHandler.TipRegionByKey(rect, "ISF_ButtonLimit");

            if (Widgets.ButtonImage(rect, limitLocked ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex, GUI.color, true))
            {
                limitLocked = !limitLocked;
                return true;
            }
            return false;
        }

        public bool TurnButton(float x, float y)
        {
            Rect rect = new Rect(x, y, 24f, 24f);
            MouseoverSounds.DoRegion(rect);
            TooltipHandler.TipRegionByKey(rect, "ISF_ButtonTurnTimer");

            if (Widgets.ButtonImage(rect, turnTimerOn ? TexButton.SpeedButtonTextures[0] : TexButton.SpeedButtonTextures[1], GUI.color, true))
            {
                turnTimerOn = !turnTimerOn;
                return true;
            }
            return false;
        }

        public void DrawOutline(Rect rect, Color outColor, int outThick = 1, Texture2D lineTex = null)
        {
            Color color = GUI.color;
            GUI.color = outColor;
            Widgets.DrawBox(rect, outThick, lineTex);
            GUI.color = color;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");

            Scribe_Collections.Look(ref energyTrackers, "energyTrackers", LookMode.Deep, new object[] { pawn });
            Scribe_Collections.Look(ref learningTrackers, "learningTrackers", LookMode.Deep, new object[] { pawn });
            Scribe_Deep.Look(ref progressTracker, "progressTracker", new object[] { pawn });
            Scribe_Deep.Look(ref learningNodeRecord, "nodeTracker", new object[] { pawn });
            Scribe_Values.Look(ref favorited, "favorited", false);

            Scribe_Values.Look(ref hasLimits, "hasLimits", false);
            Scribe_Values.Look(ref hasTurns, "hasTurns", false);
            Scribe_Values.Look(ref limitLocked, "limitLocked", true, false);
            Scribe_Values.Look(ref turnTimerOn, "turnTimerOn", true, false);

        }

        public Pawn pawn;

        public SorcerySchemaDef def;

        public List<EnergyTracker> energyTrackers = new List<EnergyTracker>();

        public List<LearningTracker> learningTrackers = new List<LearningTracker>();

        public LearningNodeRecord learningNodeRecord;

        public ProgressTracker progressTracker;

        public bool favorited = false;

        public bool hasLimits = false;

        public bool hasTurns = false;

        public bool limitLocked = true;

        public bool turnTimerOn = true;

        public int loadID = -1;

    }
}
