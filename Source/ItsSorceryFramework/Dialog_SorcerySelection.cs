using System.Collections.Generic;
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
			titleRect.y += titleRect.height + 10f;
			Text.Font = GameFont.Small;

			// fields for elements list
			float scale = 50f;
			Color colHighlight = Color.white;
			Color colBorder = Color.white;

			Rect bounds = new Rect(titleRect.x, titleRect.y, rect.width, rect.yMax - titleRect.y); // scroll boundary rect
			Rect viewRect = new Rect(bounds.x, bounds.y, bounds.width - 10f, scrollViewHeight); // scroll view rect
			//Widgets.DrawBoxSolidWithOutline(bounds, Color.clear, Color.green); // outline bound box
			//Widgets.DrawBoxSolidWithOutline(viewRect, Color.clear, Color.grey); // outline view

			Widgets.BeginScrollView(bounds, ref scrollPosition, viewRect, true); // start scroll

			Rect sorceriesRect = GenUI.DrawElementStack(viewRect, scale, allSorcery,
				delegate (Rect r, Sorcery sorcery)
				{
					// changes highlight of element rect- 100% color if visible, grayed out if not
					if (sorcery.visible) colHighlight = Color.white;
					else colHighlight = new Color(0.25f, 0.25f, 0.25f);

					// GUI.DrawTexture(r, BaseContent.ClearTex);
					if (Mouse.IsOver(r)) // if hovering over element
					{
						Widgets.DrawHighlight(r); // highlight rect
						TipSignal tip = new TipSignal(() => sorcery.SorceryTooltip,
							(int)bounds.y * 37);
						TooltipHandler.TipRegion(r, tip); // shows tip box where mouse is hovered over
					}
					if (Widgets.ButtonImage(r, sorcery.sorceryDef.uiIcon, colHighlight, true)) // if element rect is selected
					{
						sorcery.visible = !sorcery.visible; // toggles sorcery visibility
					}

					// draw white outline around the element rectangle for visible sorceries, black for non-visible
					if (sorcery.visible) colBorder = Color.white;
                    else colBorder = Color.black;

					Color color = GUI.color;
					GUI.color = colBorder;
					Widgets.DrawBox(r, 1, null);
					GUI.color = color;

				}, (Sorcery sorcery) => scale, 5f, 5f, true);
			scrollViewHeight = sorceriesRect.height; // set view rect height to elementwindow height

			Widgets.EndScrollView();


		}

		public List<Sorcery> allSorcery;

		private Vector2 scrollPosition = Vector2.zero;

		private float scrollViewHeight;
	}
}
