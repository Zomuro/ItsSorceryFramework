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
    public class ProgressTracker_RPG : ProgressTracker
    {
        // initalizer- created via activator via SorcerySchema
        public ProgressTracker_RPG(Pawn pawn) : base(pawn)
        {

        }

        public ProgressTracker_RPG(Pawn pawn, ProgressTrackerDef def) : base(pawn, def)
        {
            Initialize();
        }

        public ProgressTracker_RPG(Pawn pawn, SorcerySchemaDef def) : base(pawn, def)
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
            setupHediffStage(hediff);
        }

        public void setupHediffStage(Hediff_ProgressLevel hediff)
        {
            if(hediff.CurStage != null) hediff.def.stages.Clear();

            HediffStage newStage = new HediffStage() {
                minSeverity = currLevel,
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
                if (def.Workers.NullOrEmpty()) return;
                foreach (var worker in def.Workers)
                {
                    if (worker.GetType() == typeof(ProgressEXPWorker_Passive)) worker.TryExecute(this);
                    else if (worker.GetType() == typeof(ProgressEXPWorker_DuringJob)) worker.TryExecute(this);
                }
            }

            
        }

        public override void addExperience(float experience)
        {
            float orgSev = currLevel;
            bool done = false;
            exp += experience;

            // maybe put this into exp workers
            /*MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, 
                experience.ToStringByStyle(ToStringStyle.FloatMaxTwo, ToStringNumberSense.Offset) + " EXP");*/

            while (!done)
            {
                if(exp > currentLevelEXPReq)
                {
                    exp -= currentLevelEXPReq;
                    hediff.Severity += 1;
                    notifyLevelUp(hediff.Severity);
                }
                else
                {
                    done = true;
                }
            }

            if(currLevel > orgSev)
            {
                notifyTotalLevelUp(orgSev);
            }
        }

        public override void forceLevelUp()
        {
            if (hediff == null) return;
            hediff.Severity += 1;
            notifyLevelUp(hediff.Severity);
        }

        public override void notifyLevelUp(float sev)
        {
            HediffStage currStage = hediff.CurStage;
            //bool check = false;

            ProgressLevelModifier factor = def.getLevelFactor(sev);
            if (factor != null)
            {
                adjustModifiers(factor);
                adjustAbilities(factor);
                adjustHediffs(factor);
                points += factor.pointGain;
            }

            ProgressLevelModifier special = def.getLevelSpecific(sev);
            if (special != null)
            {
                adjustModifiers(factor);
                adjustAbilities(factor);
                adjustHediffs(factor);
                points += factor.pointGain;
            }

            hediff.curStage = refreshCurStage();
        }

        public void adjustModifiers(ProgressLevelModifier modulo)
        {
            adjustTotalStatMods(statOffsetsTotal, modulo.statOffsets);
            adjustTotalStatMods(statFactorsTotal, modulo.statFactorOffsets, true);
            adjustTotalCapMods(capModsTotal, modulo.capMods);
        }

        public override void adjustModifiers(List<StatModifier> offsets = null, List<StatModifier> factorOffsets = null, 
            List<PawnCapacityModifier> capMods = null)
        {
            adjustTotalStatMods(statOffsetsTotal, offsets);
            adjustTotalStatMods(statFactorsTotal, factorOffsets, true);
            adjustTotalCapMods(capModsTotal, capMods);
        }

        public virtual void adjustTotalStatMods(Dictionary<StatDef, float> stats, List<StatModifier> statMods, bool factor = false)
        {
            if (statMods.NullOrEmpty()) return;
            
            foreach(StatModifier statMod in statMods)
            {
                if (stats.Keys.Contains(statMod.stat))
                {
                    stats[statMod.stat] += statMod.value;
                    continue;
                }

                if(!factor) stats[statMod.stat] = statMod.value;
                else stats[statMod.stat] = statMod.value + 1f;
            }
        }

        public virtual void adjustTotalCapMods(Dictionary<PawnCapacityDef, float> caps, List<PawnCapacityModifier> capMods)
        {
            if (capMods.NullOrEmpty()) return;

            foreach (PawnCapacityModifier capMod in capMods)
            {
                if (caps.Keys.Contains(capMod.capacity))
                {
                    caps[capMod.capacity] += capMod.offset;
                    continue;
                }

                caps[capMod.capacity] = capMod.offset != null ? capMod.offset : 0f;
            }
        }

        public virtual IEnumerable<StatModifier> createStatModifiers(Dictionary<StatDef, float> stats)
        {
            foreach (var pair in stats) yield return new StatModifier() { stat = pair.Key, value = pair.Value };

            yield break;
        }

        public virtual IEnumerable<PawnCapacityModifier> createCapModifiers(Dictionary<PawnCapacityDef, float> caps)
        {
            foreach (var pair in caps) yield return new PawnCapacityModifier() { capacity = pair.Key, offset = pair.Value };

            yield break;
        }

        public override HediffStage refreshCurStage()
        {

            HediffStage stage = new HediffStage()
            {
                statOffsets = createStatModifiers(statOffsetsTotal).ToList(),
                statFactors = createStatModifiers(statFactorsTotal).ToList(),
                capMods = createCapModifiers(capModsTotal).ToList()
            };

            return stage;
        }

        public virtual void adjustAbilities(ProgressLevelModifier modifier)
        {
            Pawn_AbilityTracker abilityTracker = this.pawn.abilities;

            foreach (AbilityDef abilityDef in modifier.abilityGain)
            {
                abilityTracker.GainAbility(abilityDef);
            }

            foreach (AbilityDef abilityDef in modifier.abilityRemove)
            {
                abilityTracker.RemoveAbility(abilityDef);
            }
        }

        public virtual void adjustHediffs(ProgressLevelModifier modifier)
        {
            Hediff hediff;
            foreach (NodeHediffProps props in modifier.hediffAdd)
            {
                hediff = HediffMaker.MakeHediff(props.hediffDef, pawn, null);
                hediff.Severity = props.severity;

                pawn.health.AddHediff(hediff, null, null, null);
            }

            foreach (NodeHediffProps props in modifier.hediffAdjust)
            {
                HealthUtility.AdjustSeverity(pawn, props.hediffDef, props.severity);
            }

            foreach (HediffDef hediffDef in modifier.hediffRemove)
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null) pawn.health.RemoveHediff(hediff);
            }
        }

        public override void notifyTotalLevelUp(float orgSev)
        {
            Find.LetterStack.ReceiveLetter("ISF_LevelUp".Translate(pawn.Name),
                "ISF_LevelUpMessage".Translate(orgSev.ToString(), currLevel.ToString()), LetterDefOf.NeutralEvent, null);
        }

        public override float currProgress
        {
            get
            {
                return exp / currentLevelEXPReq;
            }
        }

        public override float currentLevelEXPReq
        {
            get
            {
                return def.baseEXP * Mathf.Pow(def.scaling, currLevel - 1f);
            }
        }      


    }
}
