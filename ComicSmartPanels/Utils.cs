using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicSmartPanels
{
	class Utils
	{
		public static double ZoomLevel { get; set; }
		public static PointF StartMove { get; set; }
		public static RectangleF DrawRect { get; set; }

		private static int _dragMargin;
		private static int _originalImageWidth;
		private static int _originalImageHeight;
		private static int _drawingAreaWidth;
		private static int _drawingAreaHeight;


		public static void SetData(int dragMargin, RectangleF drawRect,
								   int originalImageWidth, int originalImageHeight, int drawingAreaWidth,
								   int drawingAreaHeight)
		{
			DrawRect = drawRect;

			_dragMargin = dragMargin;
			_originalImageWidth = originalImageWidth;
			_originalImageHeight = originalImageHeight;
			_drawingAreaWidth = drawingAreaWidth;
			_drawingAreaHeight = drawingAreaHeight;
		}

		public static bool RelativePointInRect(PointF p, RectangleF r)
		{
			return (p.X > -_dragMargin && p.Y > -_dragMargin && p.X < r.Width + _dragMargin && p.Y < r.Height + _dragMargin);
		}

		public static PointF GetRelativePoint(Point absolutePoint)
		{
			return new PointF(absolutePoint.X - DrawRect.Left, absolutePoint.Y - DrawRect.Top);
		}

		public static RectangleF GetRectWithBounds(float x, float y, float width, float height)
		{
			if (x < 0) x = 0;
			if (y < 0) y = 0;
			if (width < 50) width = 50;
			if (height < 50) height = 50;

			return new RectangleF(x, y, width, height);
		}

		public static RectangleF ResizeRectangle(Point mousePos, RectangleF source, BoxAction boxAction, bool shiftPressed, bool useGrid, int gridSize)
		{
			RectangleF result = source;

			PointF p = GetRelativePoint(mousePos);
			float width;
			float height;

			int grid = gridSize;
			float gridTemp;
			float gridTemp2;

			switch (boxAction)
			{
				case BoxAction.Move:
					float posX = mousePos.X - StartMove.X;
					float posY = mousePos.Y - StartMove.Y;

					if (useGrid)
					{
						posX = (float)Math.Truncate(posX / grid) * grid;
						posY = (float)Math.Truncate(posY / grid) * grid;
					}

					result = GetRectWithBounds(posX, posY, DrawRect.Width, DrawRect.Height);
					break;

				case BoxAction.ResizeRight:
					width = mousePos.X - DrawRect.X;

					if (useGrid)
						width = ((float)Math.Truncate((float)mousePos.X / grid) * grid) - DrawRect.X;

					result = GetRectWithBounds(DrawRect.X, DrawRect.Y, width, DrawRect.Height);
					break;

				case BoxAction.ResizeLeft:

					gridTemp = mousePos.X - StartMove.X;

					if (useGrid)
						gridTemp = (float)Math.Truncate((float)mousePos.X / grid) * grid - StartMove.X;

					width = DrawRect.Right - gridTemp;

					result = GetRectWithBounds(gridTemp, DrawRect.Y, width, DrawRect.Height);
					break;

				case BoxAction.ResizeBottom:
					height = mousePos.Y - DrawRect.Y;

					if (useGrid)
						height = ((float)Math.Truncate((float)mousePos.Y / grid) * grid) - DrawRect.Y;

					result = GetRectWithBounds(DrawRect.X, DrawRect.Y, DrawRect.Width, height);
					break;

				case BoxAction.ResizeTop:

					gridTemp = mousePos.Y - StartMove.Y;

					if (useGrid)
						gridTemp = (float)Math.Truncate((float)mousePos.Y / grid) * grid - StartMove.Y;

					height = DrawRect.Bottom - gridTemp;
					//height = DrawRect.Height - (p.Y - StartMove.Y);

					result = GetRectWithBounds(DrawRect.X, gridTemp, DrawRect.Width, height);
					break;


				case BoxAction.ResizeRightTop:
					width = mousePos.X - DrawRect.X;
					height = DrawRect.Height - (p.Y - StartMove.Y);

					gridTemp = mousePos.Y - StartMove.Y;

					if (useGrid)
					{
						width = ((float)Math.Truncate((float)mousePos.X / grid) * grid) - DrawRect.X;
						gridTemp = (float)Math.Truncate((float)mousePos.Y / grid) * grid - StartMove.Y;

						height = DrawRect.Bottom - gridTemp;
					}

					if (shiftPressed)
						width = height;

					result = GetRectWithBounds(DrawRect.X, gridTemp, width, height);
					break;

				case BoxAction.ResizeRightBottom:
					width = mousePos.X - DrawRect.X;
					height = mousePos.Y - DrawRect.Y;

					if (useGrid)
					{
						width = ((float)Math.Truncate((float)mousePos.X / grid) * grid) - DrawRect.X;
						height = ((float)Math.Truncate((float)mousePos.Y / grid) * grid) - DrawRect.Y;
					}

					if (shiftPressed)
						width = height;

					result = GetRectWithBounds(DrawRect.X, DrawRect.Y, width, height);
					break;

				case BoxAction.ResizeLeftBottom:
					width = DrawRect.Width - (p.X - StartMove.X);
					height = mousePos.Y - DrawRect.Y;

					gridTemp = mousePos.X - StartMove.X;

					if (useGrid)
					{
						gridTemp = (float)Math.Truncate((float)mousePos.X / grid) * grid - StartMove.X;

						width = DrawRect.Right - gridTemp;
						height = ((float)Math.Truncate((float)mousePos.Y / grid) * grid) - DrawRect.Y;
					}

					if (shiftPressed)
						height = width;

					result = GetRectWithBounds(gridTemp, DrawRect.Y, width, height);
					break;

				case BoxAction.ResizeLeftTop:
					gridTemp = mousePos.X - StartMove.X;
					gridTemp2 = mousePos.Y - StartMove.Y;

					width = DrawRect.Width - (p.X - StartMove.X);
					height = DrawRect.Height - (p.Y - StartMove.Y);

					if (useGrid)
					{
						gridTemp = (float)Math.Truncate((float)mousePos.X / grid) * grid - StartMove.X;
						gridTemp2 = (float)Math.Truncate((float)mousePos.Y / grid) * grid - StartMove.Y;

						width = DrawRect.Right - gridTemp;
						height = DrawRect.Bottom - gridTemp2;
					}

					if (shiftPressed)
						height = width;

					result = GetRectWithBounds(gridTemp, gridTemp2, width, height);
					break;
			}

			return result;
		}

		public static RectangleF GetRelativeRect(RectangleF rect)
		{
			float left = rect.Left * 100f / (float)_drawingAreaWidth;
			float top = rect.Top * 100f / (float)_drawingAreaHeight;
			float width = rect.Width * 100f / (float)_drawingAreaWidth;
			float height = rect.Height * 100f / (float)_drawingAreaHeight;

			return new RectangleF(left, top, width, height);
		}

		public static RectangleF GetDisplayRectangleFromRelative(RectangleF relativeRect, double zoomLevel)
		{
			float width = (float)_originalImageWidth * (float)zoomLevel;
			float height = (float)_originalImageHeight * (float)zoomLevel;

			float rleft = width * relativeRect.Left / 100;
			float rtop = height * relativeRect.Top / 100;

			float rwidth = width * relativeRect.Width / 100;
			float rheight = height * relativeRect.Height / 100;

			return new RectangleF(rleft, rtop, rwidth, rheight);
		}

		public static BoxAction GetBoxAction(PointF relativePoint)
		{
			BoxAction boxAction = BoxAction.Move;

			if (RelativePointInRect(relativePoint, DrawRect))
			{
				if (relativePoint.X + _dragMargin >= DrawRect.Width) boxAction = BoxAction.ResizeRight;
				if ((DrawRect.Left > _dragMargin) && (relativePoint.X - _dragMargin <= _dragMargin)) boxAction = BoxAction.ResizeLeft;

				if (relativePoint.Y + _dragMargin >= DrawRect.Height) boxAction = BoxAction.ResizeBottom;
				if ((DrawRect.Top > _dragMargin) && (relativePoint.Y - _dragMargin <= _dragMargin)) boxAction = BoxAction.ResizeTop;

				if ((relativePoint.X + _dragMargin >= DrawRect.Width) && ((DrawRect.Top > _dragMargin) && (relativePoint.Y - _dragMargin <= _dragMargin))) boxAction = BoxAction.ResizeRightTop;
				if ((relativePoint.X + _dragMargin >= DrawRect.Width) && (relativePoint.Y + _dragMargin >= DrawRect.Height)) boxAction = BoxAction.ResizeRightBottom;

				if ((DrawRect.Left > _dragMargin) && (relativePoint.X - _dragMargin <= _dragMargin) && (relativePoint.Y + _dragMargin >= DrawRect.Height)) boxAction = BoxAction.ResizeLeftBottom;
				if ((DrawRect.Left > _dragMargin) && (relativePoint.X - _dragMargin <= _dragMargin) &&
					(DrawRect.Top > _dragMargin) && (relativePoint.Y - _dragMargin <= _dragMargin)) boxAction = BoxAction.ResizeLeftTop;

			}
			else
				boxAction = BoxAction.None;


			return boxAction;
		}

		/// <summary>
		/// Checks for left/top < 0 and width/height > source width/height
		/// </summary>
		public static RectangleF NormalizeRect(RectangleF r, RectangleF maxRectangle)
		{
			float l = r.Left;
			float t = r.Top;
			float w = r.Width;
			float h = r.Height;

			if (l < 0) l = 0;
			if (t < 0) t = 0;

			if (l + w > maxRectangle.Width) w = maxRectangle.Width - l;
			if (t + h > maxRectangle.Height) h = maxRectangle.Height - t;

			return new RectangleF(l, t, w, h);
		}

	}

}
