using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicSmartPanels
{
	public class ComicPanel
	{
		public bool IsRectangle { get; set; }
		public PanelDesignerColor DesignColor { get; set; }
		public bool IsDesignVisible { get; set; }

		public RectangleF RelativePosition { get; set; }
		//public RectangleF PxPosition { get; set; }

		public int PanelNum { get; set; }

		public ComicPanel(int panelNum, RectangleF rectanglePosition, Random rnd)
		{
			this.IsRectangle = true;

			// Set Random color
			Array values = Enum.GetValues(typeof(PanelDesignerColor));
			if (rnd != null)
				this.DesignColor = (PanelDesignerColor)values.GetValue(rnd.Next(values.Length));
			else
				this.DesignColor = PanelDesignerColor.Aqua;


			this.IsDesignVisible = true;

			this.RelativePosition = Utils.GetRelativeRect(rectanglePosition);

			this.PanelNum = panelNum;
		}

	}
}
