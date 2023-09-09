using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;


namespace ItsSorceryFramework
{
    public class Dialog_SorcerySelection : Window
	{
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(600, 400);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return false;
			}
		}

		public Dialog_SorcerySelection(List<Sorcery> sorceries) : base()
		{
			allSorcery = sorceries;
			//this.pawn = pawn;
			closeOnClickedOutside = true;
			forcePause = true;
			closeOnCancel = true;
			doCloseX = true; // set to false
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(inRect);

			// Title and subtitle of dialog
			Text.Font = GameFont.Medium;
			Rect titleRect = new Rect(rect);
			Widgets.LabelCacheHeight(ref titleRect, "Sorcery Selection", true, false);
			titleRect.y+= titleRect.height;
			Text.Font = GameFont.Tiny;
			Widgets.LabelCacheHeight(ref titleRect, "Toggle sorceries for visibility. Grayed-out sorceries are not visible.", true, false);
			titleRect.y += titleRect.height;
			Text.Font = GameFont.Small;

			// fields for elements list
			float scale = 50f;
			Color col = Color.white;

			Rect bounds = new Rect(titleRect.x, titleRect.y, rect.width, rect.yMax - titleRect.y); // scroll boundary rect
			Rect viewRect = new Rect(bounds.x, bounds.y, bounds.width - 10f, scrollViewHeight); // scroll view rect
			Widgets.DrawBoxSolidWithOutline(bounds, Color.clear, Color.green); // outline bound
			Widgets.DrawBoxSolidWithOutline(viewRect, Color.clear, Color.grey); // outline view

			Widgets.BeginScrollView(bounds, ref scrollPosition, viewRect, true); // start scroll

			Rect sorceriesRect = GenUI.DrawElementStack(bounds, scale, allSorcery,
				delegate (Rect r, Sorcery sorcery)
				{
					if (sorcery.visible) col = Color.white;
					else col = new Color(0.25f, 0.25f, 0.25f);

					GUI.DrawTexture(r, BaseContent.ClearTex);
					if (Mouse.IsOver(r))
					{
						Widgets.DrawHighlight(r);
					}
					if (Widgets.ButtonImage(r, sorcery.sorceryDef.uiIcon, col, true))
					{
						//Find.WindowStack.Add(new Dialog_InfoCard(sorcery.sorceryDef, null));
						sorcery.visible = !sorcery.visible;
					}
					if (Mouse.IsOver(r))
					{
						TipSignal tip = new TipSignal(() => sorcery.SorceryTooltip + "\n\n" + "ClickToLearnMore".Translate().Colorize(ColoredText.SubtleGrayColor),
							(int)bounds.y * 37);
						TooltipHandler.TipRegion(r, tip);
					}

				}, (Sorcery sorcery) => scale, 4f, 5f, true);
			scrollViewHeight = sorceriesRect.height; // set view rect height to elementwindow height

			Widgets.EndScrollView();


		}

		public List<Sorcery> allSorcery;

		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;
	}
}
