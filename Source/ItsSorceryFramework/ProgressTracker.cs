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
        }

        public ProgressTracker(Pawn pawn, SorcerySchemaDef def)
        {
            this.pawn = pawn;
            this.def = def.progressTrackerDef;
            this.sorcerySchemaDef = def;
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
        }

        public virtual void Initialize()
        {

        }

        public virtual void ProgressTrackerTick()
        {

        }

        public virtual void AddExperience(float experience)
        {

        }

        public virtual void ForceLevelUp()
        {

        }

        public virtual void NotifyLevelUp(float sev)
        {

        }

        public virtual void NotifyTotalLevelUp(float orgSev)
        {
            Find.LetterStack.ReceiveLetter("Level up.",
                "This pawn has leveled up.", LetterDefOf.NeutralEvent, null);
        }

        public bool Maxed
        {
            get
            {
                return (CurrLevel) >= hediff.def.maxSeverity;
            }
        }

        public int CurrLevel
        {
            get
            {
                return (int) hediff.Severity;
            }
        }

        public virtual float CurrProgress
        {
            get
            {
                return exp / CurrentLevelEXPReq;
            }
            
        }

        public virtual float CurrentLevelEXPReq
        {
            get
            {
                return def.baseEXP;
            }
        }

        public virtual void AdjustModifiers(List<StatModifier> offsets = null, List<StatModifier> factorOffsets = null,
            List<PawnCapacityModifier> capMods = null)
        {

        }

        public virtual HediffStage RefreshCurStage()
        {
            return new HediffStage();
        }

        // if the def has specific level labels, get the one for the current level
        public string CurLevelLabel
        {
            get
            {
                if(cachedCurLevel != CurrLevel || cachedLevelLabel.NullOrEmpty())
                {
                    cachedLevelLabel = GetProgressLevelLabel(CurrLevel);
                }

                return cachedLevelLabel;
            }
        }

        // get the list of level labels and sort by descending level
        public List<ProgressLevelLabel> LevelLabelsDesc
        {
            get
            {
                if(cachedLevelLabels == null)
                {
                    cachedLevelLabels = def.levelLabels?.OrderByDescending(x => x.level).ToList();
                }
                return cachedLevelLabels;
            }
        }

        // grab the level label at a specific level
        public string GetProgressLevelLabel(int level)
        {
            if(def.levelLabels.NullOrEmpty()) return null;

            foreach(var levelLabel in LevelLabelsDesc)
            {
                if (levelLabel.level <= level) return levelLabel.label;
            }
            return null;
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

        private int cachedCurLevel = 0;

        private string cachedLevelLabel;

        private List<ProgressLevelLabel> cachedLevelLabels;
    }
}
