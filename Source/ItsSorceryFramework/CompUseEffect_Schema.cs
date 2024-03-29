﻿using RimWorld;
using Verse;

namespace ItsSorceryFramework
{
    public class CompUseEffect_Schema : CompUseEffect
	{
		public CompProperties_UseEffectSchema Props
		{
			get
			{
				return (CompProperties_UseEffectSchema)this.props;
			}
		}

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);

			SorcerySchemaUtility.AddSorcerySchema(usedBy, Props.schemaDef);
			if (PawnUtility.ShouldSendNotificationAbout(usedBy))
			{
				Messages.Message("ISF_UsedSchemaItem".Translate(usedBy.Named("USER"), Props.schemaDef.label), usedBy, MessageTypeDefOf.PositiveEvent, true);
			}
		}

		public override bool CanBeUsedBy(Pawn p, out string failReason)
		{
			if (SorcerySchemaUtility.FindSorcerySchema(p, Props.schemaDef) != null)
			{
				failReason = "ISF_UseSchemaItemFail".Translate(p.Named("USER"), Props.schemaDef.label);
				return false;
			}
			return base.CanBeUsedBy(p, out failReason);
		}
	}
}
