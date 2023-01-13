using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
	public class CompSchemaItem : CompUsable
	{
		public override void PostExposeData()
		{
			base.PostExposeData();
			//Scribe_Defs.Look<SorcerySchemaDef>(ref this.schemaDef, "schemaDef");
		}

		public CompProperties_SchemaItem SchemaProps
		{
			get
			{
				return (CompProperties_SchemaItem)this.props;
			}
		}

		protected override string FloatMenuOptionLabel(Pawn pawn)
		{
			if (SchemaProps.schemaDef == null) return "Need to add a sorcery schema def.";
			return string.Format(base.Props.useLabel, SchemaProps.schemaDef.label);
		}

		//public SorcerySchemaDef schemaDef;
	}
}
