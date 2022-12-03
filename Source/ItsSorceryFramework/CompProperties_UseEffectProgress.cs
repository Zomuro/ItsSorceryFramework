using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
	public class CompProperties_UseEffectProgress : CompProperties_UseEffect
	{
		public CompProperties_UseEffectProgress()
		{
			this.compClass = typeof(CompUseEffectProgress);
		}

		public SorcerySchemaDef sorcerySchemaDef;
	}
}
