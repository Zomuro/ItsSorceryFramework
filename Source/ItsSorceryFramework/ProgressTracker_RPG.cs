using System;
using System.Collections.Generic;
using System.Linq;
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
            SetupHediffStage(hediff);
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
                else
                {
                    done = true;
                }
            }

            if(CurrLevel > orgSev)
            {
                NotifyTotalLevelUp(orgSev);
            }
        }

        public override void ForceLevelUp()
        {
            if (hediff == null) return;
            hediff.Severity += 1;
            NotifyLevelUp(hediff.Severity);
        }

        public override void NotifyLevelUp(float sev)
        {
            //HediffStage currStage = hediff.CurStage;

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

        public void ApplyOptions(ProgressLevelModifier modifier)
        {
            int select = Math.Min(modifier.optionChoices, modifier.options.Count);

            if (modifier.options.NullOrEmpty() || select == 0) return; // empty options -> skip rest
            if (modifier.options.Count == 1) // only one option = autoselect that option
            {
                AdjustModifiers(modifier.options[0]);
                AdjustAbilities(modifier.options[0]);
                AdjustHediffs(modifier.options[0]);
                points += modifier.options[0].pointGain;
                return;
            }

            // if there's a proper list of 2+ options for the progresslevelmodifier, create a window for selection.
            List<DebugMenuOption> options;
            if (select < 0 || select > modifier.options.Count) options = LevelOptions(modifier).ToList();
            else options = LevelOptions(modifier).OrderBy(x => rand.Next()).Take(select).ToList();
            Find.WindowStack.Add(new Dialog_ProgressLevelOptions(options, this));
        }

        public IEnumerable<DebugMenuOption> LevelOptions(ProgressLevelModifier modifier)
        {
            foreach(var option in modifier.options)
            {
                yield return new DebugMenuOption(option.label.CapitalizeFirst(), DebugMenuOptionMode.Action, delegate ()
                {
                    Dialog_ProgressLevelOptions optionsDialog = (Find.WindowStack.currentlyDrawnWindow as Dialog_ProgressLevelOptions);
                    if (optionsDialog is null) return;
                    optionsDialog.selectedOption = option;
                });
            }
            yield break;
        }

        public void AdjustModifiers(ProgressLevelModifier modulo)
        {
            AdjustTotalStatMods(statOffsetsTotal, modulo.statOffsets);
            AdjustTotalStatMods(statFactorsTotal, modulo.statFactorOffsets, true);
            AdjustTotalCapMods(capModsTotal, modulo.capMods);
        }

        public void AdjustModifiers(ProgressLevelOption option)
        {
            AdjustTotalStatMods(statOffsetsTotal, option.statOffsets);
            AdjustTotalStatMods(statFactorsTotal, option.statFactorOffsets, true);
            AdjustTotalCapMods(capModsTotal, option.capMods);
        }

        public override void AdjustModifiers(List<StatModifier> offsets = null, List<StatModifier> factorOffsets = null, 
            List<PawnCapacityModifier> capMods = null)
        {
            AdjustTotalStatMods(statOffsetsTotal, offsets);
            AdjustTotalStatMods(statFactorsTotal, factorOffsets, true);
            AdjustTotalCapMods(capModsTotal, capMods);
        }

        public virtual void AdjustTotalStatMods(Dictionary<StatDef, float> stats, List<StatModifier> statMods, bool factor = false)
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

        public virtual void AdjustTotalCapMods(Dictionary<PawnCapacityDef, float> caps, List<PawnCapacityModifier> capMods)
        {
            if (capMods.NullOrEmpty()) return;

            foreach (PawnCapacityModifier capMod in capMods)
            {
                if (caps.Keys.Contains(capMod.capacity))
                {
                    caps[capMod.capacity] += capMod.offset;
                    continue;
                }

                caps[capMod.capacity] = capMod.offset;
            }
        }

        public virtual IEnumerable<StatModifier> CreateStatModifiers(Dictionary<StatDef, float> stats)
        {
            foreach (var pair in stats) yield return new StatModifier() { stat = pair.Key, value = pair.Value };

            yield break;
        }

        public virtual IEnumerable<PawnCapacityModifier> CreateCapModifiers(Dictionary<PawnCapacityDef, float> caps)
        {
            foreach (var pair in caps) yield return new PawnCapacityModifier() { capacity = pair.Key, offset = pair.Value };

            yield break;
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

        public virtual void AdjustAbilities(ProgressLevelModifier modifier)
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

        public virtual void AdjustAbilities(ProgressLevelOption option)
        {
            Pawn_AbilityTracker abilityTracker = pawn.abilities;

            foreach (AbilityDef abilityDef in option.abilityGain)
            {
                abilityTracker.GainAbility(abilityDef);
            }

            foreach (AbilityDef abilityDef in option.abilityRemove)
            {
                abilityTracker.RemoveAbility(abilityDef);
            }
        }

        public virtual void AdjustHediffs(ProgressLevelModifier modifier)
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

        public virtual void AdjustHediffs(ProgressLevelOption option)
        {
            Hediff hediff;
            foreach (NodeHediffProps props in option.hediffAdd)
            {
                hediff = HediffMaker.MakeHediff(props.hediffDef, pawn, null);
                hediff.Severity = props.severity;

                pawn.health.AddHediff(hediff, null, null, null);
            }

            foreach (NodeHediffProps props in option.hediffAdjust)
            {
                HealthUtility.AdjustSeverity(pawn, props.hediffDef, props.severity);
            }

            foreach (HediffDef hediffDef in option.hediffRemove)
            {
                hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null) pawn.health.RemoveHediff(hediff);
            }
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

        public System.Random rand = new System.Random();


    }
}
