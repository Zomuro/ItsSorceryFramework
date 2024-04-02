using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ProgressTracker : IExposable
    {
        public Pawn pawn;

        public ProgressTrackerDef def;

        public SorcerySchema schema;

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

        private List<SorceryDef> cachedSorceryDefs = new List<SorceryDef>();

        //private bool cacheDirtySorceries = true;

        public System.Random rand = new System.Random();

        // initalizer- created via activator via SorcerySchema
        public ProgressTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public ProgressTracker(Pawn pawn, ProgressTrackerDef def, SorcerySchema schema)
        {
            this.pawn = pawn;
            this.def = def;
            this.schema = schema;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref schema, "schema");
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

        public virtual void SetupHediffStage(Hediff_Progress hediff)
        {
            if (hediff.CurStage != null) hediff.def.stages.Clear();

            HediffStage newStage = new HediffStage()
            {
                minSeverity = CurrLevel,
                statOffsets = new List<StatModifier>(),
                statFactors = new List<StatModifier>(),
                capMods = new List<PawnCapacityModifier>()
            };
            hediff.curStage = newStage;
        }

        public virtual void ProgressTrackerTick() { }

        public virtual void AddExperience(float experience) { }

        public virtual void ForceLevelUp() { }

        public virtual void NotifyLevelUp(float sev) { }

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

            if (!pawn.Faction.IsPlayer) // if we try to apply options to a NPC, just choose a random option.
            {
                ProgressLevelOption option = modifier.options.RandomElement();
                AdjustModifiers(option);
                AdjustAbilities(option);
                AdjustHediffs(option);
                points += option.pointGain;
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

        public virtual HediffStage RefreshCurStage() => new HediffStage();

        public virtual void NotifyTotalLevelUp(float orgSev)
        {
            Find.LetterStack.ReceiveLetter("Level up.",
                "This pawn has leveled up.", LetterDefOf.NeutralEvent, null);
        }

        public bool Maxed => (CurrLevel) >= hediff.def.maxSeverity;

        public int CurrLevel => (int)hediff.Severity;

        public virtual float CurrProgress => exp / CurrentLevelEXPReq;

        public virtual float CurrentLevelEXPReq => def.baseEXP;

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
            Widgets.LabelCacheHeight(ref labelRect, schema.def.LabelCap, true, false);
        }

        public virtual void DrawRightGUI(Rect rect) { }

        public virtual float DrawModifiers(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "Modifiers", true, false);
            rect.yMin += rect.height;
            //Text.Font = GameFont.Small;
            rect.x += 22f;

            String tipString = TipStringExtra(hediff.CurStage);
            if (!tipString.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Current:", true, false);
                rect.yMin += rect.height;
                Text.Font = GameFont.Small;

                Widgets.LabelCacheHeight(ref rect, tipString, true, false);
                rect.yMin += rect.height;
            }

            return rect.yMin - yMin;
        }

        public virtual string TipStringExtra(HediffStage stage)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StatDrawEntry statDrawEntry in stage.SpecialDisplayStats())
            {
                if (statDrawEntry.ShouldDisplay)
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            return stringBuilder.ToString();
        }

        public virtual string TipStringExtra(IEnumerable<StatDrawEntry> entries)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StatDrawEntry statDrawEntry in entries)
            {
                if (statDrawEntry.ShouldDisplay)
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            return stringBuilder.ToString();
        }

        public virtual string TipStringExtra(ProgressLevelModifier mods)
        {
            IEnumerable<StatDrawEntry> entries = def.specialDisplayMods(mods);
            if (entries.EnumerableNullOrEmpty()) return null;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (StatDrawEntry statDrawEntry in entries)
            {
                if (statDrawEntry.ShouldDisplay)
                {
                    stringBuilder.AppendInNewLine("  - " + statDrawEntry.LabelCap + ": " + statDrawEntry.ValueString);
                }
            }
            if (mods.pointGain > 0) stringBuilder.AppendInNewLine("  - " + def.skillPointLabelKey.Translate().CapitalizeFirst() + ": " +
                 mods.pointGain);
            return stringBuilder.ToString();
        }

        public virtual float DrawEXPMethods(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Medium;
            Widgets.LabelCacheHeight(ref rect, "Experience", true, false);
            rect.yMin += rect.height;
            //Text.Font = GameFont.Small;
            rect.x += 22f;

            if (schema.progressTracker.def.Workers.EnumerableNullOrEmpty()) return rect.yMin - yMin;
            foreach (ProgressEXPWorker worker in schema.progressTracker.def.Workers)
            {
                rect.yMin += worker.drawWorker(rect);
            }

            return rect.yMin - yMin;
        }

        public virtual bool HyperlinkCheck(ProgressLevelModifier mod)
        {
            if (mod == null) return false;

            if (mod.abilityGain.NullOrEmpty() && mod.abilityRemove.NullOrEmpty() && mod.hediffAdd.NullOrEmpty() &&
                mod.hediffAdjust.NullOrEmpty() && mod.hediffRemove.NullOrEmpty()) return false;

            return true;
        }

        public virtual float DrawHyperlinks(Rect rect, ProgressLevelModifier mod)
        {
            List<AbilityDef> abilityGain = mod.abilityGain;
            List<AbilityDef> abilityRemove = mod.abilityRemove;
            List<NodeHediffProps> hediffAdd = mod.hediffAdd;
            List<NodeHediffProps> hediffAdjust = mod.hediffAdjust;
            List<HediffDef> hediffRemove = mod.hediffRemove;

            if (abilityGain.NullOrEmpty() && abilityRemove.NullOrEmpty() && hediffAdd.NullOrEmpty() && hediffAdjust.NullOrEmpty() &&
                hediffRemove.NullOrEmpty())
            {
                return 0f;
            }

            float yMin = rect.yMin;
            float x = rect.x;
            Dialog_InfoCard.Hyperlink hyperlink;

            if (!abilityGain.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Abilities gained:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (AbilityDef abilityDef in abilityGain)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(abilityDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!abilityRemove.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Abilities removed:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (AbilityDef abilityDef in abilityRemove)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(abilityDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!hediffAdd.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Hediffs added:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (NodeHediffProps prop in hediffAdd)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    HediffDef hediffDef = prop.hediffDef;
                    string sev;

                    sev = hediffDef.stages.NullOrEmpty() ? prop.severity.ToStringWithSign("F0") :
                        hediffDef.stages[hediffDef.StageAtSeverity(prop.severity)].label;
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, hediffDef.LabelCap + " ({0})".Translate(sev),
                        2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;

                }
                rect.x = x;
            }

            if (!hediffAdjust.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Hediff adjustments:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (NodeHediffProps prop in hediffAdjust)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    HediffDef hediffDef = prop.hediffDef;
                    string sev;

                    sev = prop.severity.ToStringWithSign("F0");
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, hediffDef.LabelCap + " ({0})".Translate(sev),
                        2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            if (!hediffRemove.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "Hediffs removed:", true, false);
                rect.yMin += rect.height;
                rect.x += 6f;
                foreach (HediffDef hediffDef in hediffRemove)
                {
                    Rect hyperRect = new Rect(rect.x, rect.yMin, rect.width, 24f);
                    hyperlink = new Dialog_InfoCard.Hyperlink(hediffDef, -1);
                    Widgets.HyperlinkWithIcon(hyperRect, hyperlink, null, 2f, 6f, new Color(0.8f, 0.85f, 1f), false);
                    rect.yMin += 24f;
                }
                rect.x = x;
            }

            return rect.yMin - yMin;
        }

        public virtual float DrawSorceries(Rect rect)
        {
            float yMin = rect.yMin;
            float x = rect.x;

            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(rect);
            Widgets.LabelCacheHeight(ref titleRect, "Sorceries", true, false);

            Rect titleButtonRect = new Rect(titleRect);
            rect.yMin += titleRect.height;
            Text.Font = GameFont.Small;
            titleButtonRect.x = rect.x + rect.width / 3f;
            titleButtonRect.width = rect.width / 6f;
            if (Widgets.ButtonText(titleButtonRect, "Selection")) Find.WindowStack.Add(new Dialog_SorcerySelection(AllSorceries));

            float scale = 50f;
            Color col = Color.white;
            Rect bounds = new Rect(rect.x, rect.y, rect.width - 5f, rect.height);
            Rect sorceriesRect = GenUI.DrawElementStack(bounds, scale, AllSorceryDefs,
                delegate (Rect r, SorceryDef sorceryDef)
                {
                    if (pawn.abilities.GetAbility(sorceryDef) != null) col = Color.white;
                    else col = new Color(0.25f, 0.25f, 0.25f);

                    GUI.DrawTexture(r, BaseContent.ClearTex);
                    if (Mouse.IsOver(r))
                    {
                        Widgets.DrawHighlight(r);
                        Sorcery sorcery = AbilityUtility.MakeAbility(sorceryDef, pawn) as Sorcery;
                        TipSignal tip = new TipSignal(() => sorcery.SorceryTooltip + "\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor),
                            (int)bounds.y * 37);
                        TooltipHandler.TipRegion(r, tip);
                    }
                    if (Widgets.ButtonImage(r, sorceryDef.uiIcon, col, true))
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(sorceryDef, null));
                    }
                }, (SorceryDef sorceryDef) => scale, 4f, 5f, true);

            rect.yMin += sorceriesRect.height;

            return rect.yMin - yMin;
        }

        public List<Sorcery> AllSorceries
        {
            get
            {
                return pawn.abilities.AllAbilitiesForReading.Where(x => x is Sorcery s && s != null && s.Schema.def == schema.def).Select(x => x as Sorcery).ToList();
            }
        }

        public List<SorceryDef> AllSorceryDefs
        {
            get
            {
                if (cachedSorceryDefs.NullOrEmpty())
                {
                    cachedSorceryDefs = (from def in DefDatabase<SorceryDef>.AllDefs
                                         where def.sorcerySchema == schema.def
                                         select def).ToList();
                }

                return cachedSorceryDefs;
            }
        }



        
    }
}
