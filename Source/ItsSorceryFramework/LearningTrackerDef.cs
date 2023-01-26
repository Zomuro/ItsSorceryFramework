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
    public class LearningTrackerDef : Def
    {
		public Texture2D BGIcon
		{
			get
			{
				if (trackerBG == null)
				{
					if (!bgPath.NullOrEmpty())
					{
						trackerBG = ContentFinder<Texture2D>.Get(bgPath, true);
					}
					else
					{
						trackerBG = BaseContent.BadTex;
					}
				}
				return trackerBG;
			}
		}

		public Type learningTrackerClass = typeof(LearningTracker);

        public string bgPath;

		private Texture2D trackerBG;


    }
}
