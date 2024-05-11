using System;
using Verse;

namespace ItsSorceryFramework
{
    public class LearningTrackerDef : Def
    {
		public Type learningTrackerClass = typeof(LearningTracker);

        public bool defaultLocked = false;

        public string lockedLabel = "Locked";

        public string unlockTip = "Unlock this skill tree through a variety of methods.";
    }
}
