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
    public class ProgressTracker : IExposable
    {
        // initalizer- created via activator via SorcerySchema
        public ProgressTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public ProgressTracker(Pawn pawn, ProgressTrackerDef def)
        {
            this.pawn = pawn;
            this.def = def;
            this.sorcerySchemaDef = null;
            //Initialize();
        }

        public ProgressTracker(Pawn pawn, SorcerySchemaDef def)
        {
            this.pawn = pawn;
            this.def = def.progressTrackerDef;
            this.sorcerySchemaDef = def;
            //Initialize();
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref sorcerySchemaDef, "sorcerySchemaDef");
            Scribe_References.Look(ref hediff, "hediff");
            Scribe_Values.Look(ref exp, "exp", 0f);
            Scribe_Values.Look(ref usedPoints, "usedPoints", 0);
            Scribe_Values.Look(ref points, "points", 0);
            Scribe_Collections.Look(ref statOffsetsTotal, "statOffsetsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref statFactorsTotal, "statFactorsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref capModsTotal, "capModsTotal", LookMode.Def, LookMode.Value);


            // probably will have to just cache this

            // comp saving stuff
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                this.InitializeComps();
            }
            if (this.comps != null)
            {
                foreach(ProgressComp comp in comps)
                {
                    comp.PostExposeData();
                }
            }

        }

        public virtual void Initialize()
        {
            InitializeComps();
        }

        public void InitializeComps() // taken from ThingWithComps
        {
            if (this.def.progressComps.Any<ProgressCompProperties>())
            {
                this.comps = new List<ProgressComp>();
                foreach(ProgressCompProperties prop in this.def.progressComps)
                {
                    ProgressComp progressComp = null;
                    try
                    {
                        progressComp = (ProgressComp)Activator.CreateInstance(prop.compClass);
                        progressComp.parent = this;
                        this.comps.Add(progressComp);
                        progressComp.Initialize(prop);
                    }
                    catch (Exception arg)
                    {
                        Log.Error("Could not instantiate or initialize a ProgressComp: " + arg);
                        this.comps.Remove(progressComp);
                    }
                }
            }
        }

        public PC GetProgressComp<PC>() where PC : ProgressComp // enables ability to get ProgressComps
        {
            if (this.comps != null)
            {
                foreach(ProgressComp comp in comps)
                {
                    PC pc = comp as PC;
                    if (pc != null) return pc;
                }
            }
            return default(PC);
        }

        public virtual void addExperience(float experience)
        {

        }

        public virtual void forceLevelUp()
        {

        }

        public virtual void notifyLevelUp(float sev)
        {

        }

        /*public virtual void adjustTotalStatMods(Dictionary<StatDef, float> stats, List<StatModifier> statMods, bool factor = false)
        {
            if (statMods.NullOrEmpty()) return;
        }

        // for later
        public virtual void adjustTotalCapMods(Dictionary<PawnCapacityDef, float> caps, List<PawnCapacityModifier> capMods)
        {
            if (capMods.NullOrEmpty()) return;

        }

        public virtual IEnumerable<StatModifier> createStatModifiers(Dictionary<StatDef, float> stats)
        {
            yield break;
        }*/

        public virtual void notifyTotalLevelUp(float orgSev)
        {
            Find.LetterStack.ReceiveLetter("Level up.",
                "This pawn has leveled up.", LetterDefOf.NeutralEvent, null);
        }

        public int currLevel
        {
            get
            {
                return (int) hediff.Severity;
            }
        }

        public virtual float currProgress
        {
            get
            {
                return exp / currentLevelEXPReq;
            }
            
        }

        public virtual float currentLevelEXPReq
        {
            get
            {
                return def.baseEXP;
            }
        }

        public virtual void adjustModifiers(List<StatModifier> offsets = null, List<StatModifier> factorOffsets = null,
            List<PawnCapacityModifier> capMods = null)
        {

        }

        public virtual HediffStage refreshCurStage()
        {
            return new HediffStage();
        }

        public Pawn pawn;

        public ProgressTrackerDef def;

        public SorcerySchemaDef sorcerySchemaDef;

        public Hediff_ProgressLevel hediff;

        public Dictionary<StatDef, float> statOffsetsTotal = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> statFactorsTotal = new Dictionary<StatDef, float>();

        public Dictionary<PawnCapacityDef, float> capModsTotal = new Dictionary<PawnCapacityDef, float>();

        public float exp = 0f;

        public int usedPoints = 0;

        public int points = 0;

        private List<ProgressComp> comps;

        


    }
}
