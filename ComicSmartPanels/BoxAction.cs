using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicSmartPanels
{
	public enum BoxAction
	{
		None = 0,
		Move,
		ResizeRight,
		ResizeLeft,
		ResizeBottom,
		ResizeTop,
		ResizeRightTop,
		ResizeRightBottom,
		ResizeLeftBottom,
		ResizeLeftTop
	};
}
