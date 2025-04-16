using System;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ItsSorcerySettings : ModSettings
    {
        // ITab //

        // General
        public int GeneralStatCacheTicks = 60;

        // SorcerySchema
        public bool SchemaShowEnergyBar = true;

        public bool SchemaShowSkillPoints = false;

        // EnergyTracker
        public int EnergyStatCacheTicks = 60;

        // LearningTracker

        // ProgressTracker
        public bool ProgressShowXPMotes = true;

        public int ProgressViewProspectsNum = 5;

        // Presets and Debug
        public bool ShowItsSorceryDebug = false;

        public override void ExposeData()
        {
            // ITab
            Scribe_Values.Look(ref GeneralStatCacheTicks, "GeneralStatCacheTicks", 300);

            // SorcerySchema
            Scribe_Values.Look(ref SchemaShowEnergyBar, "SchemaShowEnergyBar", true);
            Scribe_Values.Look(ref SchemaShowSkillPoints, "SchemaShowSkillPoints", false);

            // EnergyTracker
            Scribe_Values.Look(ref EnergyStatCacheTicks, "EnergyStatCacheTicks", 300);

            // LearningTracker

            // ProgressTracker
            Scribe_Values.Look(ref ProgressShowXPMotes, "ProgressShowXPMotes", true);
            Scribe_Values.Look(ref ProgressViewProspectsNum, "ProgressShowProspectsNum", 5);

            // ProgressTracker
            Scribe_Values.Look(ref ShowItsSorceryDebug, "ShowItsSorceryDebug", false);

            base.ExposeData();
        }
    }

    public class ItsSorceryMod : Mod
    {
        public ItsSorcerySettings settings;

        private static List<TabRecord> tabsList = new List<TabRecord>();

        private Tab tab;

        private Vector2 scrollPosition;

        private float leftHalfViewHeight;

        private const int MinStatCacheTicks = 60;

        private const int MaxStatCacheTicks = 600;

        private const int MinProspectLevels = 3;

        private const int MaxProspectLevels = 100;

        private enum Tab
        {
            General,
            Schema,
            Energy,
            Learning,
            Progress
        }

        public ItsSorceryMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<ItsSorcerySettings>();
        }

        public override string SettingsCategory() => "ISF_Settings_Category".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            // set up tabs for settings
            tabsList.Clear();
            tabsList.Add(new TabRecord("ISF_Settings_General".Translate(), delegate ()
            {
                tab = Tab.General;
            }, tab == Tab.General));
            tabsList.Add(new TabRecord("ISF_Settings_Schema".Translate(), delegate ()
            {
                tab = Tab.Schema;
            }, tab == Tab.Schema));
            tabsList.Add(new TabRecord("ISF_Settings_Energy".Translate(), delegate ()
            {
                tab = Tab.Energy;
            }, tab == Tab.Energy));
            tabsList.Add(new TabRecord("ISF_Settings_Learning".Translate(), delegate ()
            {
                tab = Tab.Learning;
            }, tab == Tab.Learning));
            tabsList.Add(new TabRecord("ISF_Settings_Progress".Translate(), delegate ()
            {
                tab = Tab.Progress;
            }, tab == Tab.Progress));

            Rect tabRect = new Rect(inRect)
            {
                yMin = 80
            };
            TabDrawer.DrawTabs(tabRect, tabsList, 200);

            Rect leftHalf = new Rect(tabRect.x, tabRect.y, tabRect.width / 2, tabRect.height); // Prefs.UIScale
            leftHalf = leftHalf.ContractedBy(10f);
            Rect leftHalfView = new Rect(leftHalf.x, leftHalf.y, leftHalf.width - 20f, leftHalfViewHeight); // Prefs.UIScale
            Rect rightHalf = new Rect(tabRect.x + tabRect.width / 2, tabRect.y, tabRect.width / 2, tabRect.height); // Prefs.UIScale

            // left half - tab specific settings
            Widgets.BeginScrollView(leftHalf, ref scrollPosition, leftHalfView, true);           
            var listing = new Listing_Standard();
            listing.Begin(leftHalf.ContractedBy(10f));
            switch (tab)
            {
                case Tab.General:
                    GeneralSettings(ref listing);
                    break;
                case Tab.Schema:
                    SchemaSettings(ref listing);
                    break;
                case Tab.Energy:
                    EnergySettings(ref listing);
                    break;
                case Tab.Learning:
                    LearningSettings(ref listing);
                    break;
                case Tab.Progress:
                    ProgressSettings(ref listing);
                    break;
                default: break;
            }
            listing.End();
            leftHalfViewHeight = listing.CurHeight;
            Widgets.EndScrollView();

            // right half - general settings
            DrawRightPart(rightHalf);

            base.DoSettingsWindowContents(inRect);
        }

        public int IncByFive(int num)
        {
            return (int) (Mathf.Round(num / 5f) * 5f);
        }

        public void GeneralSettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_General".Translate());
            listing.GapLine();

            Text.Font = GameFont.Small;
            listing.Label("ISF_Settings_GeneralStatCacheTicks".Translate(settings.GeneralStatCacheTicks.ToString(), -1,
                "ISF_Settings_GeneralStatCacheTicksDesc".Translate()));
            settings.GeneralStatCacheTicks = (int)listing.Slider(IncByFive(settings.GeneralStatCacheTicks), MinStatCacheTicks, MaxStatCacheTicks);

            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsGeneralDefault();
        }

        public void SchemaSettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_Schema".Translate());
            listing.GapLine();

            Text.Font = GameFont.Small;
            listing.CheckboxLabeled("ISF_Settings_SchemaShowEnergyBars".Translate(),
                ref settings.SchemaShowEnergyBar, "ISF_Settings_SchemaShowEnergyBarsDesc".Translate());
            listing.CheckboxLabeled("ISF_Settings_SchemaShowSkillPoints".Translate(),
                ref settings.SchemaShowSkillPoints, "ISF_Settings_SchemaShowSkillPointsDesc".Translate());

            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsSchemaDefault();
        }

        public void EnergySettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_Energy".Translate());
            listing.GapLine();

            Text.Font = GameFont.Small;
            listing.Label("ISF_Settings_EnergyStatCacheTicks".Translate(settings.EnergyStatCacheTicks.ToString(), -1,
                "ISF_Settings_EnergyStatCacheTicksDesc".Translate()));
            settings.EnergyStatCacheTicks = (int)listing.Slider(IncByFive(settings.EnergyStatCacheTicks), MinStatCacheTicks, MaxStatCacheTicks);

            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsEnergyDefault();
        }

        public void LearningSettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_Learning".Translate());
            listing.GapLine();

            Text.Font = GameFont.Small;
            listing.Label("ISF_Settings_None".Translate());

            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsLearningDefault();
        }

        public void ProgressSettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_Progress".Translate());
            listing.GapLine();

            Text.Font = GameFont.Small;
            listing.CheckboxLabeled("ISF_Settings_ProgressShowXPMotes".Translate(),
                ref settings.ProgressShowXPMotes, "ISF_Settings_ProgressShowXPMotesDesc".Translate());
            listing.Label("ISF_Settings_ProgressShowViewProspectsNum".Translate(settings.ProgressViewProspectsNum.ToString(), -1,
                "ISF_Settings_ProgressShowViewProspectsNumDesc".Translate()));
            settings.ProgressViewProspectsNum = (int)listing.Slider(settings.ProgressViewProspectsNum, MinProspectLevels, MaxProspectLevels);

            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsProgressDefault();
        }

        public void DrawRightPart(Rect rect)
        {
            Rect generalRect = rect.ContractedBy(10f);
            Widgets.DrawMenuSection(generalRect);
                        
            var listing = new Listing_Standard();
            listing.Begin(generalRect.ContractedBy(10f));

            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_PresetDebug".Translate());
            listing.GapLine();

            Text.Font = GameFont.Small;
            if (listing.ButtonText("ISF_Settings_PresetDebugPerf".Translate())) SettingsPerf();
            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_PresetDebugDefault".Translate())) SettingsGlobalDefault();
            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_PresetDebugHigh".Translate())) SettingsHigh();
            listing.Gap(8f);

            listing.CheckboxLabeled("ISF_Settings_PresetDebugShowDebug".Translate(),
                ref settings.ShowItsSorceryDebug, "ISF_Settings_PresetDebugShowDebugDesc".Translate());

            listing.End();
        }

        public void SettingsGeneralDefault(bool msg = true)
        {
            settings.GeneralStatCacheTicks = 300;

            if (msg) Messages.Message(new Message("ISF_Settings_GeneralDefaultMessage".Translate(), MessageTypeDefOf.NeutralEvent));
        }

        public void SettingsSchemaDefault(bool msg = true)
        {
            settings.SchemaShowEnergyBar = true;
            settings.SchemaShowSkillPoints = false;

            if(msg) Messages.Message(new Message("ISF_Settings_SchemaDefaultMessage".Translate(), MessageTypeDefOf.NeutralEvent));
        }

        public void SettingsEnergyDefault(bool msg = true)
        {
            settings.EnergyStatCacheTicks = 300;

            if (msg) Messages.Message(new Message("ISF_Settings_EnergyDefaultMessage".Translate(), MessageTypeDefOf.NeutralEvent));
        }

        public void SettingsLearningDefault(bool msg = true)
        {
            if (msg) Messages.Message(new Message("ISF_Settings_LearningDefaultMessage".Translate(), MessageTypeDefOf.NeutralEvent));
        }

        public void SettingsProgressDefault(bool msg = true)
        {
            settings.ProgressShowXPMotes = true;
            settings.ProgressViewProspectsNum = 5;

            if (msg) Messages.Message(new Message("ISF_Settings_ProgressDefaultMessage".Translate(), MessageTypeDefOf.NeutralEvent));
        }

        public void SettingsGlobalDefault()
        {
            SettingsGeneralDefault(false);
            SettingsSchemaDefault(false);
            SettingsEnergyDefault(false);
            SettingsLearningDefault(false);
            SettingsProgressDefault(false);

            Messages.Message(new Message("ISF_Settings_PresetDefaultMessage".Translate(), MessageTypeDefOf.NeutralEvent));
        }

        public void SettingsPerf()
        {
            settings.GeneralStatCacheTicks = MaxStatCacheTicks;

            settings.SchemaShowEnergyBar = false;
            settings.SchemaShowSkillPoints = false;

            settings.EnergyStatCacheTicks = MaxStatCacheTicks;

            settings.ProgressShowXPMotes = false;
            settings.ProgressViewProspectsNum = MinProspectLevels;

            Messages.Message(new Message("ISF_Settings_PresetPerfMessage".Translate(), MessageTypeDefOf.NeutralEvent));
        }

        public void SettingsHigh()
        {
            settings.GeneralStatCacheTicks = MinStatCacheTicks;

            settings.SchemaShowEnergyBar = true;
            settings.SchemaShowSkillPoints = true;

            settings.EnergyStatCacheTicks = MinStatCacheTicks;

            settings.ProgressShowXPMotes = true;
            settings.ProgressViewProspectsNum = MaxProspectLevels;

            Messages.Message(new Message("ISF_Settings_PresetHighMessage".Translate(), MessageTypeDefOf.NeutralEvent));
        }


    }

    public static class ItsSorceryUtility
    {
        public static ItsSorcerySettings settings = LoadedModManager.GetMod<ItsSorceryMod>().GetSettings<ItsSorcerySettings>();
    }
}
