using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    public class ProgressTracker_Level : ProgressTracker
    {
        // initalizer- created via activator via SorcerySchema
        public ProgressTracker_Level(Pawn pawn) : base(pawn)
        {

        }

        public ProgressTracker_Level(Pawn pawn, ProgressTrackerDef def) : base(pawn, def)
        {
            Initialize();
        }

        public ProgressTracker_Level(Pawn pawn, SorcerySchemaDef def) : base(pawn, def)
        {
            Initialize();
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void Initialize()
        {
            base.Initialize();
            if(pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff) == null)
                HealthUtility.AdjustSeverity(pawn, def.progressHediff, def.progressHediff.initialSeverity);
            hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff) as Hediff_ProgressLevel;
            hediff.progressTracker = this;
            SetupHediffStage(hediff as Hediff_ProgressLevel);
        }

        public void SetupHediffStage(Hediff_ProgressLevel hediff)
        {
            if(hediff.CurStage != null) hediff.def.stages.Clear();

            HediffStage newStage = new HediffStage() {
                minSeverity = CurrLevel,
                statOffsets = new List<StatModifier>(),
                statFactors = new List<StatModifier>(),
                capMods = new List<PawnCapacityModifier>()
            };
            hediff.curStage = newStage;
        }

        public override void ProgressTrackerTick()
        {
            if(Find.TickManager.TicksGame % 60 == 0)
            {               
                if (def.Workers.EnumerableNullOrEmpty()) return;
                foreach (var worker in def.Workers.Where(x => x.GetType() == typeof(ProgressEXPWorker_Passive) || 
                    x.GetType() == typeof(ProgressEXPWorker_DuringJob)))
                {
                    worker.TryExecute(this);
                }
            }
        }

        public override void AddExperience(float experience)
        {
            float orgSev = CurrLevel;
            bool done = false;
            exp += experience;

            while (!done)
            {
                if(exp > CurrentLevelEXPReq)
                {
                    exp -= CurrentLevelEXPReq;
                    hediff.Severity += 1;
                    NotifyLevelUp(hediff.Severity);
                }
                else done = true;
            }

            if(CurrLevel > orgSev) NotifyTotalLevelUp(orgSev);
        }

        public override void ForceLevelUp()
        {
            if (hediff == null) return;
            hediff.Severity += 1;
            NotifyLevelUp(hediff.Severity);
        }

        public override void NotifyLevelUp(float sev)
        {
            ProgressLevelModifier factor = def.getLevelFactor(sev);
            if (factor != null)
            {
                AdjustModifiers(factor);
                AdjustAbilities(factor);
                AdjustHediffs(factor);
                points += factor.pointGain;
                ApplyOptions(factor);
            }

            ProgressLevelModifier special = def.getLevelSpecific(sev);
            if (special != null)
            {
                AdjustModifiers(special);
                AdjustAbilities(special);
                AdjustHediffs(special);
                points += special.pointGain;
                ApplyOptions(special);
            }

            hediff.curStage = RefreshCurStage();
        }

        public override HediffStage RefreshCurStage()
        {

            HediffStage stage = new HediffStage()
            {
                statOffsets = CreateStatModifiers(statOffsetsTotal).ToList(),
                statFactors = CreateStatModifiers(statFactorsTotal).ToList(),
                capMods = CreateCapModifiers(capModsTotal).ToList()
            };

            return stage;
        }

        public override void NotifyTotalLevelUp(float orgSev)
        {
            Find.LetterStack.ReceiveLetter(def.progressLevelUpKey.Translate(pawn.Name.ToStringShort),
                def.progressLevelUpDescKey.Translate(orgSev.ToString(), CurrLevel.ToString()), LetterDefOf.NeutralEvent, null);
        }

        public override float CurrProgress
        {
            get
            {
                return exp / CurrentLevelEXPReq;
            }
        }

        public override float CurrentLevelEXPReq
        {
            get
            {
                return def.baseEXP * Mathf.Pow(def.scaling, CurrLevel - 1f);
            }
        }

    }
}
