using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class EnergyTracker : IExposable
    {
        // initalizer- created via activator via SorcerySchema
        public EnergyTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public EnergyTracker(Pawn pawn, EnergyTrackerDef def) 
        {
            this.pawn = pawn;
            this.def = def;
            this.sorcerySchemaDef = null;

            // maybe put initalize gizmo here idunno
        }

        public EnergyTracker(Pawn pawn, SorcerySchemaDef def)
        {
            this.pawn = pawn;
            this.def = def.energyTrackerDef;
            this.sorcerySchemaDef = def;

            // maybe put initalize gizmo here idunno
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref sorcerySchemaDef, "sorcerySchemaDef");
            Scribe_Values.Look<float>(ref this.currentEnergy, "currentEnergy", 0f, false);
        }

        public virtual float MaxEnergy
        {
            get
            {
                return 50f;//this.pawn.GetStatValue(def.energyMaxStatDef, true);
            }
        }

        public virtual float MinEnergy
        {
            get
            {
                return 0f; //this.pawn.GetStatValue(def.energyMinStatDef, true);
            }
        }

        public virtual float OverMaxEnergy
        {
            get
            {
                return 100;//this.pawn.GetStatValue(def.energyOverMaxStatDef, true);
            }
        }

        public virtual float EnergyRecoveryRate
        {
            get
            {
                return 5f; //this.pawn.GetStatValue(def.energyRecoveryStatDef, true);
            }
        }

        public virtual float EnergyCostFactor
        {
            get
            {
                return 1f; // this.pawn.GetStatValue(def.energyCostFactorStatDef, true);
            }
        }

        public virtual float OverBarRecoveryFactor
        {
            get
            {
                return 0.5f;
            }
        }

        public virtual float UnderBarRecoveryFactor
        {
            get
            {
                return 0.5f;
            }
        }

        public virtual int TurnTicks
        {
            get
            {
                return 60;
            }
        }

        public virtual void EnergyTrackerTick()
        {
            
        }

        public virtual float EnergyRelativeValue
        {
            get
            {
                return this.EnergyToRelativeValue();
            }
        }


        public virtual float EnergyToRelativeValue(float energyCost = 0)
        {
            return 0f;
        }

        public virtual bool WouldReachLimitEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            return false;
        }

        public virtual bool TryAlterEnergy(float energyCost, SorceryDef sorceryDef = null, Sorcery sorcery = null)
        {
            currentEnergy = Math.Max(0f, currentEnergy - energyCost);
            return true;
        }

        public virtual void EmptyEnergy()
        {
            this.currentEnergy = 0f;
        }

        public virtual void ApplyHediffSeverity(float newSev)
        {

        }

        public virtual string DisableCommandReason()
        {
            return "ISF_CommandDisableReasonBase";
        }

        public virtual void DrawOnGUI(Rect rect)
        {

        }

        public virtual void HightlightEnergyCost(Rect rec)
        {

        }

        public void SchemaViewBox(Rect rect)
        {
            // sets up outline of the sorcery schema in the itab
            Widgets.DrawBoxSolidWithOutline(rect, new Color(), Color.grey, 1);

            // information button- shows important info about the sorcery schema
            SorcerySchemaDef tempSchemaDef = sorcerySchemaDef;
            tempSchemaDef.TempPawn = pawn;
            sorcerySchemaDef.TempPawn = pawn;

            Widgets.InfoCardButton(rect.x + 5, rect.y + 5, tempSchemaDef);
            LearningTrackerButton(rect.x + 5 + 24, rect.y + 5, pawn, sorcerySchemaDef);
            tempSchemaDef.ClearCachedData();

            // shows the label of the sorcery schema in the itab
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, sorcerySchemaDef.LabelCap.ToString());
        }

        public virtual void DrawEnergyBar(Rect rect)
        {

        }

        public bool LearningTrackerButton(float x, float y, Pawn pawn, SorcerySchemaDef schemaDef)
        {
            if (cachedLearningTrackers.NullOrEmpty())
            {
                cachedLearningTrackers = SorcerySchemaUtility.FindSorcerySchema(pawn, schemaDef).learningTrackers;
            }
            if (cachedLearningTrackers.NullOrEmpty()) return false;
            Rect rect = new Rect(x, y, 24f, 24f);
            MouseoverSounds.DoRegion(rect);
            TooltipHandler.TipRegionByKey(rect, "DefInfoTip");
            //bool result = Widgets.ButtonImage(rect, TexButton.Info, GUI.color, true);
            UIHighlighter.HighlightOpportunity(rect, "InfoCard");

            if (Widgets.ButtonImage(rect, TexButton.IconBook, GUI.color, true))
            {
                Find.TickManager.Pause();
                Find.WindowStack.Add(new Dialog_LearningTabs(cachedLearningTrackers));
                return true;
            }
            return false;
        }

        // used to detect if two values are on different "bars"
        // min bar: less than 0% energy
        // max bar: greater than 0%, less than 100%
        // overmax bar: greater than 100%
        public int findFloor(float relVal, bool decrease = true)
        {
            if (!decrease)
            {
                if (relVal < 0) return -1;
                else if (relVal < 1) return 0;
            }
            else
            {
                if (relVal <= 0) return -1;
                else if (relVal <= 1) return 0;
            }
            return 1;
        }

        // normalizes relative values for use in highlighting sorcery costs
        // see findFloor(relVal, decrease) for structure
        public float normVal(float relVal, bool decrease = true)
        {
            if (!decrease)
            {
                if (relVal < 0) return relVal + 1;
                else if (relVal < 1) return relVal;
            }
            else
            {
                if (relVal <= 0) return relVal + 1;
                else if (relVal <= 1) return relVal;
            }
            return relVal - 1;
        }

        public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            yield break;
        }

        public virtual string TopRightLabel(SorceryDef sorceryDef)
        {
            return "";
        }

        public override string ToString()
        {
            return "Energy class: "+ this.GetType().Name.ToString();
        }

        public Pawn pawn;

        public EnergyTrackerDef def;

        public SorcerySchemaDef sorcerySchemaDef;

        public List<LearningTracker> cachedLearningTrackers = new List<LearningTracker>();

        public float currentEnergy;

        public bool limitLocked = true;
    }
}
