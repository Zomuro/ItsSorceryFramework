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

        public virtual void ApplyOptions(ProgressLevelModifier modifier)
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

        public virtual IEnumerable<DebugMenuOption> LevelOptions(ProgressLevelModifier modifier)
        {
            foreach (var option in modifier.options)
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

        public virtual void AdjustModifiers(ProgressLevelModifier modulo)
        {
            AdjustTotalStatMods(statOffsetsTotal, modulo.statOffsets);
            AdjustTotalStatMods(statFactorsTotal, modulo.statFactorOffsets, true);
            AdjustTotalCapMods(capModsTotal, modulo.capMods);
        }

        public virtual void AdjustModifiers(ProgressLevelOption option)
        {
            AdjustTotalStatMods(statOffsetsTotal, option.statOffsets);
            AdjustTotalStatMods(statFactorsTotal, option.statFactorOffsets, true);
            AdjustTotalCapMods(capModsTotal, option.capMods);
        }

        public virtual void AdjustModifiers(List<StatModifier> offsets = null, List<StatModifier> factorOffsets = null,
            List<PawnCapacityModifier> capMods = null)
        {
            AdjustTotalStatMods(statOffsetsTotal, offsets);
            AdjustTotalStatMods(statFactorsTotal, factorOffsets, true);
            AdjustTotalCapMods(capModsTotal, capMods);
        }

        public virtual void AdjustTotalStatMods(Dictionary<StatDef, float> stats, List<StatModifier> statMods, bool factor = false)
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

        public virtual HediffStage RefreshCurStage()
        {
            return new HediffStage();
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

        public virtual void DrawLeftGUI(Rect rect)
        {
            Rect labelRect = new Rect(0f, 0, rect.width, 50f);
            Widgets.LabelCacheHeight(ref labelRect, sorcerySchemaDef.LabelCap, true, false);
        }

        public virtual void DrawRightGUI(Rect rect)
        {

        }

        public Pawn pawn;

        public ProgressTrackerDef def;

        public SorcerySchemaDef sorcerySchemaDef;

        public Hediff_Progress hediff;

        public Dictionary<StatDef, float> statOffsetsTotal = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> statFactorsTotal = new Dictionary<StatDef, float>();

        public Dictionary<PawnCapacityDef, float> capModsTotal = new Dictionary<PawnCapacityDef, float>();

        public float exp = 0f;

        public int usedPoints = 0;

        public int points = 0;

        private int cachedCurLevel = 0;

        private string cachedLevelLabel;

        private List<ProgressLevelLabel> cachedLevelLabels;

        public System.Random rand = new System.Random();
    }
}
