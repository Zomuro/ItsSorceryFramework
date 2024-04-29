using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class ItsSorcerySettings : ModSettings
    {
        // ITab
        public bool SchemaShowEnergyBar = true;

        public bool SchemaShowSkillPoints = false;

        // SorcerySchema

        // EnergyTracker
        public int EnergyStatCacheTicks = 60;

        // LearningTracker

        // ProgressTracker
        public bool ProgressShowXPMotes = true;

        public override void ExposeData()
        {
            // ITab
            Scribe_Values.Look(ref SchemaShowEnergyBar, "SchemaShowEnergyBar", true);
            Scribe_Values.Look(ref SchemaShowSkillPoints, "SchemaShowSkillPoints", false);

            // SorcerySchema

            // EnergyTracker
            Scribe_Values.Look(ref EnergyStatCacheTicks, "EnergyStatCacheTicks", 60);

            // LearningTracker

            // ProgressTracker
            Scribe_Values.Look(ref ProgressShowXPMotes, "ProgressShowXPMotes", true);
            
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

        private enum Tab
        {
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

        public void SchemaSettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_Schema".Translate());

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

            Text.Font = GameFont.Small;
            listing.Label("ISF_Settings_EnergyStatCacheTicks".Translate(settings.EnergyStatCacheTicks.ToString(), -1,
                "ISF_Settings_EnergyStatCacheTicksDesc".Translate()));
            settings.EnergyStatCacheTicks = (int)listing.Slider(settings.EnergyStatCacheTicks, 20, 180);

            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsEnergyDefault();
        }

        public void LearningSettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_Learning".Translate());

            Text.Font = GameFont.Small;
            listing.Label("ISF_Settings_None".Translate());

            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsLearningDefault();
        }

        public void ProgressSettings(ref Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("ISF_Settings_Progress".Translate());

            Text.Font = GameFont.Small;
            listing.CheckboxLabeled("ISF_Settings_ProgressShowXPMotes".Translate(),
                ref settings.ProgressShowXPMotes, "ISF_Settings_ProgressShowXPMotesDesc".Translate());

            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsProgressDefault();
        }

        public void DrawRightPart(Rect rect)
        {
            Rect generalRect = rect.ContractedBy(10f);
            Widgets.DrawMenuSection(generalRect);
            
            //Widgets.DrawBoxSolidWithOutline(generalRect, Color.clear, Color.grey);
            
            var listing = new Listing_Standard();
            listing.Begin(generalRect.ContractedBy(10f));

            if (listing.ButtonText("ISF_Settings_DefaultGlobal".Translate())) SettingsGlobalDefault();

            listing.End();
        }

        public void SettingsSchemaDefault()
        {
            settings.SchemaShowEnergyBar = true;
            settings.SchemaShowSkillPoints = false;
        }

        public void SettingsEnergyDefault()
        {
            settings.EnergyStatCacheTicks = 60;
        }

        public void SettingsLearningDefault()
        {

        }

        public void SettingsProgressDefault()
        {
            settings.ProgressShowXPMotes = true;
        }

        public void SettingsGlobalDefault()
        {
            SettingsSchemaDefault();
            SettingsEnergyDefault();
            SettingsLearningDefault();
            SettingsProgressDefault();
        }


    }

    public static class ItsSorceryUtility
    {
        public static ItsSorcerySettings settings = LoadedModManager.GetMod<ItsSorceryMod>().GetSettings<ItsSorcerySettings>();
    }
}
