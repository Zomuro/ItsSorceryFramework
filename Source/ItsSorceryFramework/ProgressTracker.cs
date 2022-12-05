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
    public class ProgressTracker
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
            Scribe_Deep.Look(ref hediff, "hediff");
            Scribe_Values.Look(ref exp, "exp", 0f);
            Scribe_Values.Look(ref points, "points", 0);
            Scribe_Collections.Look(ref statOffsetsTotal, "statOffsetsTotal", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref statFactorsTotal, "statFactorsTotal", LookMode.Def, LookMode.Value);
        }

        public virtual void Initialize()
        {

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

        public virtual void adjustTotalStatMods(Dictionary<StatDef, float> stats, List<StatModifier> statMods)
        {
            if (statMods.NullOrEmpty()) return;
        }

        // for later
        public virtual void adjustTotalCapMods(List<PawnCapacityModifier> capModsTotal, List<PawnCapacityModifier> capMods)
        {
            if (capMods.NullOrEmpty()) return;

        }

        public virtual IEnumerable<StatModifier> createStatModifiers(Dictionary<StatDef, float> stats)
        {
            yield break;
        }

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

        public Pawn pawn;

        public ProgressTrackerDef def;

        public SorcerySchemaDef sorcerySchemaDef;

        public Hediff_ProgressLevel hediff;

        public Dictionary<StatDef, float> statOffsetsTotal = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> statFactorsTotal = new Dictionary<StatDef, float>();

        public List<PawnCapacityModifier> capModsTotal = new List<PawnCapacityModifier>();

        public float exp = 0f;

        public int points = 0;

        


    }
}
