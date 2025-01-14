using LudeonTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public static class ProgressTrackerUtility
    {
        public static System.Random rand = new System.Random();

        public static void ApplyOptions(ref ProgressTracker progressTracker, ProgressLevelModifier modifier, ref List<Window> windows)
        {
            int select = Math.Min(modifier.optionChoices, modifier.options.Count);

            if (modifier.options.NullOrEmpty() || select == 0) return; // empty options -> skip rest
            if (modifier.options.Count == 1) // only one option = autoselect that option
            {
                AdjustModifiers(ref progressTracker, modifier.options[0]);
                AdjustAbilities(ref progressTracker, modifier.options[0]);
                AdjustHediffs(ref progressTracker, modifier.options[0]);
                progressTracker.points += modifier.options[0].pointGain;
                return;
            }

            if (!progressTracker.pawn.Faction.IsPlayer) // if we try to apply options to a NPC, just choose a random option.
            {
                ProgressLevelOption option = modifier.options.RandomElement();
                AdjustModifiers(ref progressTracker, option);
                AdjustAbilities(ref progressTracker, option);
                AdjustHediffs(ref progressTracker, option);
                progressTracker.points += option.pointGain;
                return;
            }

            // if there's a proper list of 2+ options for the progresslevelmodifier, create a window for selection.
            List<DebugMenuOption> options;
            if (select < 0 || select > modifier.options.Count) options = progressTracker.LevelOptions(modifier).ToList();
            else options = progressTracker.LevelOptions(modifier).OrderBy(x => rand.Next()).Take(select).ToList();
            windows.Add(new Dialog_ProgressLevelOptions(options, progressTracker, progressTracker.CurrLevel, progressTracker.currClassDef));
        }

        public static void AdjustModifiers(ref ProgressTracker progressTracker, ProgressLevelModifier modulo)
        {
            AdjustTotalStatMods(ref progressTracker.statOffsetsTotal, modulo.statOffsets);
            AdjustTotalStatMods(ref progressTracker.statFactorsTotal, modulo.statFactorOffsets, true);
            AdjustTotalCapMods(ref progressTracker.capModsTotal, modulo.capMods);
        }

        public static void AdjustModifiers(ref ProgressTracker progressTracker, ProgressLevelOption option)
        {
            AdjustTotalStatMods(ref progressTracker.statOffsetsTotal, option.statOffsets);
            AdjustTotalStatMods(ref progressTracker.statFactorsTotal, option.statFactorOffsets, true);
            AdjustTotalCapMods(ref progressTracker.capModsTotal, option.capMods);
        }

        public static void AdjustModifiers(ref ProgressTracker progressTracker, List<StatModifier> offsets = null, List<StatModifier> factorOffsets = null,
            List<PawnCapacityModifier> capMods = null)
        {
            AdjustTotalStatMods(ref progressTracker.statOffsetsTotal, offsets);
            AdjustTotalStatMods(ref progressTracker.statFactorsTotal, factorOffsets, true);
            AdjustTotalCapMods(ref progressTracker.capModsTotal, capMods);
        }

        public static void AdjustTotalStatMods(ref Dictionary<StatDef, float> stats, List<StatModifier> statMods, bool factor = false)
        {
            if (statMods.NullOrEmpty()) return;

            foreach (StatModifier statMod in statMods)
            {
                if (stats.Keys.Contains(statMod.stat))
                {
                    stats[statMod.stat] += statMod.value;
                    continue;
                }

                if (!factor) stats[statMod.stat] = statMod.value;
                else stats[statMod.stat] = statMod.value + 1f;
            }
        }

        public static void AdjustTotalCapMods(ref Dictionary<PawnCapacityDef, float> caps, List<PawnCapacityModifier> capMods)
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

        public static IEnumerable<StatModifier> CreateStatModifiers(Dictionary<StatDef, float> stats)
        {
            foreach (var pair in stats) yield return new StatModifier() { stat = pair.Key, value = pair.Value };

            yield break;
        }

        public static IEnumerable<PawnCapacityModifier> CreateCapModifiers(Dictionary<PawnCapacityDef, float> caps)
        {
            foreach (var pair in caps) yield return new PawnCapacityModifier() { capacity = pair.Key, offset = pair.Value };

            yield break;
        }

        public static void AdjustAbilities(ref ProgressTracker progressTracker, ProgressLevelModifier modifier)
        {
            Pawn_AbilityTracker abilityTracker = progressTracker.pawn.abilities;

            foreach (AbilityDef abilityDef in modifier.abilityGain)
            {
                abilityTracker.GainAbility(abilityDef);
            }

            foreach (AbilityDef abilityDef in modifier.abilityRemove)
            {
                abilityTracker.RemoveAbility(abilityDef);
            }
        }

        public static void AdjustAbilities(ref ProgressTracker progressTracker, ProgressLevelOption option)
        {
            Pawn_AbilityTracker abilityTracker = progressTracker.pawn.abilities;

            foreach (AbilityDef abilityDef in option.abilityGain)
            {
                abilityTracker.GainAbility(abilityDef);
            }

            foreach (AbilityDef abilityDef in option.abilityRemove)
            {
                abilityTracker.RemoveAbility(abilityDef);
            }
        }

        public static void AdjustHediffs(ref ProgressTracker progressTracker, ProgressLevelModifier modifier)
        {
            Hediff hediff;
            foreach (NodeHediffProps props in modifier.hediffAdd)
            {
                hediff = HediffMaker.MakeHediff(props.hediffDef, progressTracker.pawn, null);
                hediff.Severity = props.severity;

                progressTracker.pawn.health.AddHediff(hediff, null, null, null);
            }

            foreach (NodeHediffProps props in modifier.hediffAdjust)
            {
                HealthUtility.AdjustSeverity(progressTracker.pawn, props.hediffDef, props.severity);
            }

            foreach (HediffDef hediffDef in modifier.hediffRemove)
            {
                hediff = progressTracker.pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null) progressTracker.pawn.health.RemoveHediff(hediff);
            }
        }

        public static void AdjustHediffs(ref ProgressTracker progressTracker, ProgressLevelOption option)
        {
            Hediff hediff;
            foreach (NodeHediffProps props in option.hediffAdd)
            {
                hediff = HediffMaker.MakeHediff(props.hediffDef, progressTracker.pawn, null);
                hediff.Severity = props.severity;

                progressTracker.pawn.health.AddHediff(hediff, null, null, null);
            }

            foreach (NodeHediffProps props in option.hediffAdjust)
            {
                HealthUtility.AdjustSeverity(progressTracker.pawn, props.hediffDef, props.severity);
            }

            foreach (HediffDef hediffDef in option.hediffRemove)
            {
                hediff = progressTracker.pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (hediff != null) progressTracker.pawn.health.RemoveHediff(hediff);
            }
        }

        /*public static ProgressDiffLedger CreateNextLedgerBase(ProgressTracker progressTracker)
        {
            ProgressDiffLedger lastLedger = progressTracker.progressLedgers.LastOrDefault();
            if (lastLedger is null)
            {
                return new ProgressDiffLedger(0, progressTracker.CurrLevel, progressTracker.currClass, new Dictionary<string, ProgressDiffClassLedger>() { 
                    { "", new ProgressDiffClassLedger("")} 
                });
            }

            return new ProgressDiffLedger(lastLedger);
        }*/

        /*public static ProgressTrackerClassLedger UpdateClassLedger(ProgressTracker progressTracker, ProgressTrackerLedger ledger)
        {
            ProgressTrackerLedger lastLedger = progressTracker.progressLedgers.LastOrDefault();
            if (lastLedger is null)
            {
                return new ProgressTrackerLedger(0, progressTracker.CurrLevel, progressTracker.currClass, new Dictionary<string, ProgressTrackerClassLedger>() {
                    { "", new ProgressTrackerClassLedger("")}
                });
            }

            return new ProgressTrackerLedger(lastLedger);
        }*/
    }
}
