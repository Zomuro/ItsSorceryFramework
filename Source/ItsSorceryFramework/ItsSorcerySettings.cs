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
        public int EnergyStatCacheTicks = 60;

        public bool ProgressShowXPMotes = true;



        public override void ExposeData()
        {
            Scribe_Values.Look(ref EnergyStatCacheTicks, "EnergyStatCacheTicks", 60);
            Scribe_Values.Look(ref ProgressShowXPMotes, "ProgressShowXPMotes", true);
            
            base.ExposeData();
        }
    }

    public class ItsSorceryMod : Mod
    {
        ItsSorcerySettings settings;

        public ItsSorceryMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<ItsSorcerySettings>();
        }

        public override string SettingsCategory() => "ISF_Settings_Category".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect.ContractedBy(10f));

            listing.Label("ISF_Settings_EnergyStatCacheTicks".Translate(settings.EnergyStatCacheTicks.ToString(), -1,
                "ISF_Settings_EnergyStatCacheTicksDesc".Translate()));
            settings.EnergyStatCacheTicks = (int)listing.Slider(settings.EnergyStatCacheTicks, 20, 180);

            listing.CheckboxLabeled("ISF_Settings_ProgressShowXPMotes".Translate(settings.ProgressShowXPMotes.ToString()),
                ref settings.ProgressShowXPMotes, "ISF_Settings_ProgressShowXPMotesDesc".Translate());


            listing.Gap(8f);
            if (listing.ButtonText("ISF_Settings_Default".Translate())) SettingsDefault();

            listing.End();

            base.DoSettingsWindowContents(inRect);
        }

        public void SettingsDefault()
        {
            settings.EnergyStatCacheTicks = 60;
            settings.ProgressShowXPMotes = true;
        }

        
    }

    public static class ItsSorceryUtility
    {
        public static ItsSorcerySettings settings = LoadedModManager.GetMod<ItsSorceryMod>().GetSettings<ItsSorcerySettings>();
    }
}
