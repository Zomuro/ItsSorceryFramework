﻿using System;
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
            if(pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff) == null)
                HealthUtility.AdjustSeverity(pawn, def.progressHediff, 1f);
            hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def.progressHediff);
            HediffStage newStage = new HediffStage();
            newStage.minSeverity = currLevel;
            newStage.label = "level " + currLevel.ToString() + "; " + currProgress.ToString("P2");
            hediff.def.stages.Add(newStage);
        }

        public override void addExperience(float experience)
        {
            float orgSev = currLevel;
            bool done = false;
            exp += experience;

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
            int sevInt = (int)sev;
            bool check = false;
            foreach(ProgressLevelModulo modulo in def.levelModulo.OrderByDescending(x => x.levelFactor))
            {
                if(sevInt % modulo.levelFactor == 0)
                {
                    Log.Message("curr sev: " + sevInt.ToString());
                    Log.Message("modulo: " + modulo.levelFactor.ToString());
                    modulo.statOffsets.ToStringSafeEnumerable();

                    adjustTotalStatMods(statOffsetsTotal, modulo.statOffsets);
                    adjustTotalStatMods(statFactorsTotal, modulo.statFactors);

                    points += modulo.pointGain;
                    check = true;
                    break;
                }
            }
            if(!check) points += 1;

            HediffStage newStage = new HediffStage();
            newStage.minSeverity = sevInt;
            newStage.label = "level " + sevInt.ToString() +"; "+ currProgress.ToString("P2");
            newStage.statOffsets = createStatModifiers(statOffsetsTotal).ToList();
            newStage.statFactors = createStatModifiers(statFactorsTotal).ToList();
            hediff.def.stages.Add(newStage);
        }

        public override void adjustTotalStatMods(Dictionary<StatDef, float> stats, List<StatModifier> statMods)
        {
            if (statMods.NullOrEmpty()) return;
            
            foreach(StatModifier statMod in statMods)
            {
                if (stats.Keys.Contains(statMod.stat))
                {
                    stats[statMod.stat] += statMod.value;
                    continue;
                }

                stats[statMod.stat] = statMod.value;
            }
        }

        // for later
        public override void adjustTotalCapMods(List<PawnCapacityModifier> capModsTotal, List<PawnCapacityModifier> capMods)
        {
            if (capMods.NullOrEmpty()) return;

            List<PawnCapacityDef> capacities = (from capMod in capMods select capMod.capacity).ToList();

            foreach (PawnCapacityModifier capMod in capMods)
            {
                
            }

        }

        public override IEnumerable<StatModifier> createStatModifiers(Dictionary<StatDef, float> stats)
        {
            StatModifier mod = new StatModifier();
            foreach (var pair in stats)
            {
                mod.stat = pair.Key; mod.value = pair.Value;
                yield return mod;
            }

            yield break;
        }

        public override void notifyTotalLevelUp(float orgSev)
        {
            Find.LetterStack.ReceiveLetter("Level up: "+ pawn.Name,
                "This pawn has leveled up.", LetterDefOf.NeutralEvent, null);
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
