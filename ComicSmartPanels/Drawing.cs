using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComicSmartPanels
{
	public class Drawing
	{

		public static void DrawPanelWithAnchors(Graphics g, string panelText, Color panelColor, bool drawRectangle, RectangleF rectanglePositionInPx, int dragMargin)
		{
			RectangleF rectF = new RectangleF(rectanglePositionInPx.Left, rectanglePositionInPx.Top, rectanglePositionInPx.Width, rectanglePositionInPx.Height);
			FontFamily fontFamily = new FontFamily("Calibri");

			// Draw working rectangles and panel number
			using (Brush b = new SolidBrush(Color.FromArgb(128, panelColor)))
			{
				if (drawRectangle)
					g.FillRectangle(b, rectanglePositionInPx);
				else
					g.FillEllipse(b, rectanglePositionInPx);
			}

			// Part for panel number
			using (Brush b = new SolidBrush(panelColor))
			{
				using (Font f = new Font(fontFamily, 48, FontStyle.Bold, GraphicsUnit.Pixel))
				{
					using (StringFormat fmt = new StringFormat())
					{
						fmt.Alignment = StringAlignment.Center;
						fmt.LineAlignment = StringAlignment.Center;

						GraphicsPath gpath = new GraphicsPath();
						gpath.AddString(panelText, fontFamily, (int)FontStyle.Bold, 48, rectF, fmt);

						g.DrawString(panelText, f, b, rectF, fmt);
						g.DrawPath(new Pen(Color.Black, 1), gpath);
					}
				}
			}

			// Draw panel rectangle anchor points (resizing)
			List<RectangleF> anchors = new List<RectangleF>();

			RectangleF rTop = new RectangleF(rectanglePositionInPx.Left + (rectanglePositionInPx.Width / 2) - dragMargin, rectanglePositionInPx.Top - dragMargin, dragMargin * 2, dragMargin * 2);
			RectangleF rBottom = new RectangleF(rectanglePositionInPx.Left + (rectanglePositionInPx.Width / 2) - dragMargin, rectanglePositionInPx.Top + rectanglePositionInPx.Height - dragMargin, dragMargin * 2, dragMargin * 2);
			RectangleF rLeft = new RectangleF(rectanglePositionInPx.Left - dragMargin, rectanglePositionInPx.Top + (rectanglePositionInPx.Height / 2) - dragMargin, dragMargin * 2, dragMargin * 2);
			RectangleF rRight = new RectangleF(rectanglePositionInPx.Left + rectanglePositionInPx.Width - dragMargin, rectanglePositionInPx.Top + (rectanglePositionInPx.Height / 2) - dragMargin, dragMargin * 2, dragMargin * 2);

			RectangleF rLeftTop = new RectangleF(rectanglePositionInPx.Left - dragMargin, rectanglePositionInPx.Top - dragMargin, dragMargin * 2, dragMargin * 2);
			RectangleF rRightTop = new RectangleF(rectanglePositionInPx.Left + rectanglePositionInPx.Width - dragMargin, rectanglePositionInPx.Top - dragMargin, dragMargin * 2, dragMargin * 2);
			RectangleF rRightBottom = new RectangleF(rectanglePositionInPx.Left + rectanglePositionInPx.Width - dragMargin, rectanglePositionInPx.Top + rectanglePositionInPx.Height - dragMargin, dragMargin * 2, dragMargin * 2);
			RectangleF rLeftBottom = new RectangleF(rectanglePositionInPx.Left - dragMargin, rectanglePositionInPx.Top + rectanglePositionInPx.Height - dragMargin, dragMargin * 2, dragMargin * 2);

			anchors.Add(rTop);
			anchors.Add(rBottom);
			anchors.Add(rLeft);
			anchors.Add(rRight);
			anchors.Add(rLeftTop);
			anchors.Add(rRightTop);
			anchors.Add(rRightBottom);
			anchors.Add(rLeftBottom);

			using (Brush b = new SolidBrush(panelColor))
			{
				using (Pen p = new Pen(Color.Black, 1))
				{
					foreach (var rect in anchors)
					{
						g.FillEllipse(b, rect);
						g.DrawEllipse(p, rect);
					}
				}
			}
		}

		public static void DrawOverlayPanels(Graphics g, List<ComicPanel> panels, int selectedPanelNum, double zoomLevel)
		{

			for (var i = 0; i < panels.Count; i++)
			{
				var panelNum = panels[i].PanelNum;
				var panel = panels[i];

				if (panelNum != selectedPanelNum && panel.IsDesignVisible)
				{
					var rectanglePositionInPx = Utils.GetDisplayRectangleFromRelative(panel.RelativePosition, zoomLevel);
					var panelColor = Color.FromName(panel.DesignColor.ToString());
					var drawRectangle = panel.IsRectangle;
					var panelText = panel.PanelNum.ToString();

					RectangleF rectF = new RectangleF(rectanglePositionInPx.Left, rectanglePositionInPx.Top, rectanglePositionInPx.Width, rectanglePositionInPx.Height);
					FontFamily fontFamily = new FontFamily("Calibri");

					using (Brush b = new HatchBrush(HatchStyle.Percent40, Color.FromArgb(255, panelColor), Color.Transparent))
					{
						using (Pen p = new Pen(Color.Black, 1))
						{
							if (drawRectangle)
							{
								g.FillRectangle(b, rectanglePositionInPx);
								g.DrawRectangle(p, rectanglePositionInPx.Left, rectanglePositionInPx.Top, rectanglePositionInPx.Width, rectanglePositionInPx.Height);
							}
							else
							{
								g.FillEllipse(b, rectanglePositionInPx);
								g.DrawEllipse(p, rectanglePositionInPx);
							}
						}
					}

					using (Brush textBrush = new SolidBrush(Color.FromArgb(128, panelColor)))
					{
						using (Font f = new Font(fontFamily, 48, FontStyle.Bold, GraphicsUnit.Pixel))
						{
							using (StringFormat fmt = new StringFormat())
							{
								fmt.Alignment = StringAlignment.Center;
								fmt.LineAlignment = StringAlignment.Center;

								GraphicsPath gpath = new GraphicsPath();
								gpath.AddString(panelText, fontFamily, (int)FontStyle.Bold, 48, rectF, fmt);

								g.DrawString(panelText, f, textBrush, rectF, fmt);
								g.DrawPath(new Pen(Color.Black, 1), gpath);
							}
						}
					}

				}
			}
		}

		public static Cursor GetCursor(BoxAction ba, Cursor customHandCursor)
		{
			Cursor c;

			switch (ba)
			{
				case BoxAction.None:
				default:
					c = customHandCursor;
					break;

				case BoxAction.Move:
					c = Cursors.SizeAll;
					break;

				case BoxAction.ResizeLeft:
				case BoxAction.ResizeRight:
					c = Cursors.SizeWE;
					break;

				case BoxAction.ResizeBottom:
				case BoxAction.ResizeTop:
					c = Cursors.SizeNS;
					break;

				case BoxAction.ResizeRightTop:
					c = Cursors.SizeNESW;
					break;

				case BoxAction.ResizeRightBottom:
					c = Cursors.SizeNWSE;
					break;

				case BoxAction.ResizeLeftBottom:
					c = Cursors.SizeNESW;
					break;

				case BoxAction.ResizeLeftTop:
					c = Cursors.SizeNWSE;
					break;
			}

			return c;
		}


	}
}
