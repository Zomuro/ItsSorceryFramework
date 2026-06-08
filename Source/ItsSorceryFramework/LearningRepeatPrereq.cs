using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningRepeatPrereq : ISF_PrereqDef
    {
        public int level;

        public int pointReq = 1;

        public bool condVisiblePrereq = false;

        [NoTranslate]
        public string iconPath;

        private Texture2D uiIcon = null;

        public List<AbilityDef> abilityGain = new List<AbilityDef>();

        public List<AbilityDef> abilityRemove = new List<AbilityDef>();

        public List<NodeHediffProps> hediffAdd = new List<NodeHediffProps>();

        public List<NodeHediffProps> hediffAdjust = new List<NodeHediffProps>();

        public List<HediffDef> hediffRemove = new List<HediffDef>();

        public List<StatModifier> statOffsets;

        public List<StatModifier> statFactorOffsets;

        public List<PawnCapacityModifier> capMods;

        public List<LearningTrackerDef> unlocks;


    }
}
