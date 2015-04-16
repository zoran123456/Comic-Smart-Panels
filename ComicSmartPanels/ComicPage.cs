using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ComicSmartPanels
{
	public class ComicPage
	{
		public string ImagePath { get; private set; }
		public string PageName { get; private set; }
		public List<ComicPanel> Panels { get; private set; }
		public double ZoomLevel { get; set; }
		public Point CurrentScrollPos { get; set; }

		public ComicPage(string imagePath, string pageName)
		{
			this.ImagePath = Path.GetFileName(imagePath);
			this.PageName = pageName;
			this.Panels = new List<ComicPanel>();
			this.ZoomLevel = 0.3;
			this.CurrentScrollPos = new Point(0, 0);
		}

		public override string ToString()
		{
			if (Panels.Count > 0)
				return PageName + " (x" + Panels.Count + ")";
			else
				return PageName;
		}


	}
}
