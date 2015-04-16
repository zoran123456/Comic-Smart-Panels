using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ComicSmartPanels
{
	public partial class MainForm : Form
	{
		#region Members

		private List<ComicPage> ComicPages = new List<ComicPage>();
		private List<ComicPanel> CurrentPagePanels;

		private const int DrawingRectDragMargin = 4;
		private Random rnd = new Random();
		private Cursor handCursor = new Cursor("hand.ico");

		#endregion

		#region Properties with direct connection to Utils Class

		private static double ImageZoomLevel
		{
			get
			{
				return Utils.ZoomLevel;
			}

			set
			{
				Utils.ZoomLevel = value;
			}
		}
		private static RectangleF CurrentDrawingRectangle
		{
			get
			{
				return Utils.DrawRect;
			}

			set
			{
				Utils.DrawRect = value;
			}
		}
		private static PointF startMove
		{
			get
			{
				return Utils.StartMove;
			}

			set
			{
				Utils.StartMove = value;
			}
		}

		#endregion

		#region Form Constructor and Events (load, keypress, resize)

		public MainForm()
		{

			InitializeComponent();

		}



		private void Form1_Load(object sender, EventArgs e)
		{
			splitContainer1.SplitterDistance = 230;

			pbWorkingImage.SizeMode = PictureBoxSizeMode.Zoom;

			ImageZoomLevel = 0.3;

			trackBar1.Value = (int)(ImageZoomLevel * 100);

			SetListViewItemHeight(listView1, 24);

			stripMenuAddPanelColors();

			tabPage2.Enabled = false;

			enableTools(false);

			msiUseGrid.Checked = useGrid;
			setMenuItemGridChecked(gridSize);

		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			string[] panelNums = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

			shiftPressed = e.Shift;

			if (e.KeyCode == Keys.Delete && !e.Control && !e.Shift)
			{
				var selIndex = listViewHasSelection() ? listView1.SelectedIndices[0] : -1;

				if (selIndex > -1)
				{
					PanelAction_DeletePanel(selIndex);
					bindPagesToComboBox();

				}

				return;
			}

			string s = "-";
			switch (e.KeyData)
			{
				case Keys.D0:
				case Keys.D1:
				case Keys.D2:
				case Keys.D3:
				case Keys.D4:
				case Keys.D5:
				case Keys.D6:
				case Keys.D7:
				case Keys.D8:
				case Keys.D9:
					s = Convert.ToChar(e.KeyData).ToString();
					break;
			}


			if (panelNums.Contains(s))
				selectListItem(s);
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			shiftPressed = e.Shift;
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			reloadPicture();
		}

		#endregion

		private void reloadPicture()
		{
			if (pbWorkingImage.Image == null)
				return;

			int width = Convert.ToInt32(pbWorkingImage.Image.Width * ImageZoomLevel);
			int height = Convert.ToInt32(pbWorkingImage.Image.Height * ImageZoomLevel);

			pbWorkingImage.Size = new Size(width, height);

			int imgLeft = pnlImgHolder.Width / 2 - pbWorkingImage.Width / 2;
			int imgTop = pnlImgHolder.Height / 2 - pbWorkingImage.Height / 2;

			if (imgLeft < 0) imgLeft = 0;
			if (imgTop < 0) imgTop = 0;

			pnlImgHolder.AutoScrollPosition = Point.Empty;

			pbWorkingImage.Left = imgLeft;
			pbWorkingImage.Top = imgTop;

			int imgWidth = pbWorkingImage.Image.Width;
			int imgHeight = pbWorkingImage.Image.Height;
			int drawAreaWidth = pbWorkingImage.Width;
			int drawAreaHeight = pbWorkingImage.Height;

			Utils.SetData(DrawingRectDragMargin, CurrentDrawingRectangle, imgWidth, imgHeight, drawAreaWidth, drawAreaHeight);
		}

		// On mouse up, update panel position and info
		private void updateComicPanelInfo()
		{
			if (!listViewHasSelection())
				return;

			var selIndex = listView1.SelectedIndices[0];
			var panel = CurrentPagePanels[selIndex];

			// Get Relative (%) Rect position from Absolute (px) position
			panel.RelativePosition = Utils.GetRelativeRect(CurrentDrawingRectangle);
			panel.IsRectangle = drawingRectIsRectangle;

			listView1.Items[selIndex].SubItems[2].Text = (panel.IsRectangle) ? "Rectangle" : "Ellipse";
			listView1.Invalidate();
		}

		private void createComicPanelGrid(int cols, int rows, double margin, bool mangaLayout)
		{
			CurrentPagePanels.Clear();

			double panelWidth = (double)pbWorkingImage.Width / cols;
			double panelHeight = (double)pbWorkingImage.Height / rows;

			panelWidth -= (panelWidth * margin / 100);
			panelHeight -= (panelHeight * margin / 100);

			float ph = (float)panelHeight + ((float)panelHeight * (float)margin / 100);
			float pw = (float)panelWidth + ((float)panelWidth * (float)margin / 100);


			float startX = ((float)pbWorkingImage.Width) - (float)(panelWidth * cols);
			float startY = ((float)pbWorkingImage.Height) - (float)(panelHeight * rows);

			startX /= (cols * 2);
			startY /= (rows * 2);

			int panelNum = 1;

			for (var y = 1; y <= rows; y++)
			{
				float posY = y * ph - ph;

				if (mangaLayout)
					panelNum = y * cols;

				for (var x = 1; x <= cols; x++)
				{
					float posX = x * pw - pw;

					RectangleF rf = new RectangleF(startX + posX, startY + posY, (float)panelWidth, (float)panelHeight);

					CurrentPagePanels.Add(new ComicPanel(panelNum, rf, rnd));

					if (mangaLayout)
						panelNum--;
					else
						panelNum++;

				}
			}
		}

		#region Drawing

		private bool drawingRectIsRectangle = true;
		private Color drawingRectangleColor = Color.Blue;
		private string drawingRectangleText = string.Empty;

		private void pbWorkingImage_Paint(object sender, PaintEventArgs e)
		{
			if (CurrentPagePanels == null)
				return;

			if (msiViewAntialias.Checked)
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			}
			else
			{
				e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
			}

			var selectedPanel = -1;
			if (listViewHasSelection())
			{
				selectedPanel = Convert.ToInt32(listView1.Items[listView1.SelectedIndices[0]].Text.Trim());
			}

			Drawing.DrawOverlayPanels(e.Graphics, CurrentPagePanels, selectedPanel, ImageZoomLevel);

			if (selectedPanel > -1)
				Drawing.DrawPanelWithAnchors(e.Graphics, drawingRectangleText, drawingRectangleColor, drawingRectIsRectangle, CurrentDrawingRectangle, DrawingRectDragMargin);
		}

		#endregion

		#region Mouse Events (down, up, move, double click)

		private Point pictureDragPt;
		private bool pictureDrag;
		private bool shiftPressed;
		private bool useGrid;
		private int gridSize = 8;
		private BoxAction boxAction = BoxAction.None;


		private void pbWorkingImage_MouseMove(object sender, MouseEventArgs e)
		{
			if (ComicPages == null || ComicPages.Count == 0)
				return;

			if (boxAction == BoxAction.None)
			{
				var ba = Utils.GetBoxAction(Utils.GetRelativePoint(e.Location));
				if (!listViewHasSelection())
					ba = BoxAction.None;

				pbWorkingImage.Cursor = Drawing.GetCursor(ba, handCursor);

			}

			if (boxAction != BoxAction.None && e.X > DrawingRectDragMargin && e.Y > DrawingRectDragMargin)
			{
				CurrentDrawingRectangle = Utils.ResizeRectangle(e.Location, CurrentDrawingRectangle, boxAction, shiftPressed, useGrid, gridSize);

				pbWorkingImage.Invalidate();
			}

			if (pictureDrag)
			{

				Point changePoint = new Point(e.Location.X - pictureDragPt.X,
								  e.Location.Y - pictureDragPt.Y);

				pnlImgHolder.AutoScrollPosition = new Point(-pnlImgHolder.AutoScrollPosition.X - changePoint.X,
													  -pnlImgHolder.AutoScrollPosition.Y - changePoint.Y);

			}

		}
		private void pbWorkingImage_MouseDown(object sender, MouseEventArgs e)
		{
			if (ComicPages == null || ComicPages.Count == 0)
				return;

			PointF p = Utils.GetRelativePoint(e.Location);

			if (listViewHasSelection() && Utils.RelativePointInRect(p, CurrentDrawingRectangle))
			{
				pictureDrag = false;

				startMove = Utils.GetRelativePoint(e.Location);

				boxAction = Utils.GetBoxAction(startMove);
			}
			else
			{

				pictureDragPt = e.Location;
				pictureDrag = true;

			}

		}
		private void pbWorkingImage_MouseUp(object sender, MouseEventArgs e)
		{
			if (ComicPages == null || ComicPages.Count == 0)
				return;

			boxAction = BoxAction.None;

			pictureDrag = false;

			updateComicPanelInfo();

			ComicPages[cbPages.SelectedIndex].CurrentScrollPos = pnlImgHolder.AutoScrollPosition;
		}
		private void pbWorkingImage_DoubleClick(object sender, EventArgs e)
		{
			if (!listViewHasSelection())
				return;

			if (boxAction == BoxAction.Move)
			{

				drawingRectIsRectangle = !drawingRectIsRectangle;
				updateComicPanelInfo();

				pbWorkingImage.Invalidate();

			}
		}
		#endregion

		#region ListView custom drawing and methods for ListView only

		private int getSelectedPanelNum()
		{
			var selectedPanel = -1;
			if (listViewHasSelection())
			{
				selectedPanel = Convert.ToInt32(listView1.Items[listView1.SelectedIndices[0]].Text.Trim());
			}

			return selectedPanel;
		}

		private void bindPanelsToListView()
		{
			int index = (listView1.SelectedIndices.Count > 0) ? listView1.SelectedIndices[0] : -1;

			listView1.Items.Clear();

			int max = (CurrentPagePanels.Count == 0) ? 0 : CurrentPagePanels.Max(pnl => pnl.PanelNum);

			for (var i = 1; i <= max; i++)
			{
				bool contains = CurrentPagePanels.Where(pnl => pnl.PanelNum == i).Count() > 0;

				if (contains)
				{
					ComicPanel panel = CurrentPagePanels.Single(pnl => pnl.PanelNum == i);

					ListViewItem li = new ListViewItem(panel.PanelNum.ToString());
					li.SubItems.Add(new ListViewItem.ListViewSubItem(li, panel.DesignColor.ToString()));
					li.SubItems.Add(new ListViewItem.ListViewSubItem(li, panel.IsRectangle ? "Rectangle" : "Ellipse"));
					li.SubItems.Add(new ListViewItem.ListViewSubItem(li, "V"));

					listView1.Items.Add(li);
				}
			}

			if (index != -1 && listView1.Items.Count > index)
				listView1.Items[index].Selected = true;

			if (CurrentPagePanels.Count == 0)
			{
				tabPage2.Enabled = false;
				tabControl1.SelectedIndex = 0;
			}
			else
			{
				tabPage2.Enabled = true;
			}

		}

		private void selectListItem(string panelNum)
		{
			if (panelNum == "0")
			{
				// Deselect All
				foreach (ListViewItem i in listView1.SelectedItems)
				{
					i.Selected = false;
				}
			}
			else
			{
				// Select ListViewItem with specific Panel num
				foreach (ListViewItem i in listView1.Items)
				{
					if (i.Text == panelNum)
					{
						i.Selected = true;
						break;
					}
				}
			}
		}

		private void SetListViewItemHeight(ListView listView, int height)
		{
			ImageList imgList = new ImageList();
			imgList.ImageSize = new Size(1, height);
			listView.SmallImageList = imgList;
		}

		private bool listViewHasSelection()
		{
			return listView1.SelectedIndices.Count > 0;
		}

		private void listViewClearSelection()
		{
			foreach (ListViewItem item in listView1.Items)
				item.Selected = false;
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewHasSelection())
			{
				int panelNum = Convert.ToInt32(listView1.Items[listView1.SelectedIndices[0]].Text.Trim());

				ComicPanel panel = CurrentPagePanels.Single(pnl => pnl.PanelNum == panelNum);

				drawingRectIsRectangle = panel.IsRectangle;
				drawingRectangleColor = Color.FromName(panel.DesignColor.ToString());
				drawingRectangleText = panel.PanelNum.ToString();

				CurrentDrawingRectangle = Utils.GetDisplayRectangleFromRelative(panel.RelativePosition, ImageZoomLevel);

				if (tabControl1.SelectedIndex == 1)
					ImagePreviewGetPanel();
			}

			pbWorkingImage.Invalidate();

		}

		private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}

		private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			ListView listView = (ListView)sender;

			Rectangle rowBounds = e.Bounds;
			int leftMargin = e.Item.GetBounds(ItemBoundsPortion.Label).Left;
			Rectangle bounds = new Rectangle(leftMargin, rowBounds.Top, rowBounds.Width - leftMargin, rowBounds.Height);

			if (e.Item.Selected)
			{
				using (Brush br = new SolidBrush(Color.FromArgb(153, 184, 218)))
				{
					e.Graphics.FillRectangle(br, bounds);
				}
			}
		}

		private Image eyeIcon = Properties.Resources.eye; // Image.FromFile("eye.png");
		private Image eyeNoIcon = Properties.Resources.eyeNo; //Image.FromFile("eyeNo.png");
		private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			const int TEXT_OFFSET = 1;    // I don't know why the text is located at 1px to the right. Maybe it's only for me.

			ListView listView = (ListView)sender;


			Rectangle rowBounds = e.SubItem.Bounds;
			Rectangle labelBounds = e.Item.GetBounds(ItemBoundsPortion.Label);
			int leftMargin = labelBounds.Left - TEXT_OFFSET;
			Rectangle bounds = new Rectangle(rowBounds.Left + leftMargin, rowBounds.Top, e.ColumnIndex == 0 ? labelBounds.Width : (rowBounds.Width - leftMargin - TEXT_OFFSET), rowBounds.Height);

			TextFormatFlags align = TextFormatFlags.HorizontalCenter;

			Color c = (e.Item.Selected) ? Color.White : Color.Black;

			if (e.ColumnIndex == 1)
			{
				Rectangle r = bounds;
				r.Inflate(-6, -6);

				switch (e.Item.SubItems[2].Text)
				{
					case "Rectangle":
					default:
						e.Graphics.FillRectangle(new SolidBrush(Color.FromName(e.SubItem.Text)), r);
						break;
					case "Ellipse":
						e.Graphics.FillEllipse(new SolidBrush(Color.FromName(e.SubItem.Text)), r);
						break;
				}
			}
			else if (e.ColumnIndex == 3)
			{
				Rectangle eyeRect = bounds;
				eyeRect.Inflate(3, 3);

				int eyeLeft = eyeRect.Left + (eyeRect.Width / 2 - eyeIcon.Width / 2);
				int eyeTop = eyeRect.Top + (eyeRect.Height / 2 - eyeIcon.Height / 2);

				eyeRect = new Rectangle(eyeLeft, eyeTop, eyeIcon.Width, eyeIcon.Height);

				ComicPanel panel = CurrentPagePanels.Single(pnl => pnl.PanelNum == Convert.ToInt32(e.Item.Text));

				if (panel.IsDesignVisible)
					e.Graphics.DrawImage(eyeIcon, eyeRect);
				else
					e.Graphics.DrawImage(eyeNoIcon, eyeRect);
			}
			else
			{
				TextRenderer.DrawText(e.Graphics, e.SubItem.Text, listView.Font, bounds, c,
					align | TextFormatFlags.SingleLine | TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis);
			}
		}

		private void listView1_MouseDown(object sender, MouseEventArgs e)
		{
			var info = listView1.HitTest(e.X, e.Y);
			if (info.Item == null)
				return;

			var row = info.Item.Index;
			var col = info.Item.SubItems.IndexOf(info.SubItem);
			var value = info.Item.SubItems[col].Text;

			var panelNum = Convert.ToInt32(listView1.Items[row].Text);
			ComicPanel panel = CurrentPagePanels.Single(p => p.PanelNum == panelNum);

			if (col == 3)
			{
				panel.IsDesignVisible = !panel.IsDesignVisible;

				bindPanelsToListView();
				pbWorkingImage.Invalidate();
			}
		}

		private void listView1_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
		{
			e.NewWidth = listView1.Columns[e.ColumnIndex].Width;

			e.Cancel = true;
		}
		#endregion

		#region Context Menu Actions

		private void ctxMenuListView_Opening(object sender, CancelEventArgs e)
		{
			bool hasSelection = listViewHasSelection();

			msiDeletePanel.Enabled = hasSelection;
			msiPanelColor.Enabled = hasSelection;
			msiSwitchType.Enabled = hasSelection;
			msiPanelVisibility.Enabled = hasSelection;
			msiReadOrderUp.Enabled = hasSelection;
			msiReadOrderDown.Enabled = hasSelection;

		}

		// Click on context menu item for ListView
		private void msiClick(object sender, EventArgs e)
		{
			var selIndex = listViewHasSelection() ? listView1.SelectedIndices[0] : -1;

			switch (((ToolStripMenuItem)(sender)).Tag.ToString())
			{
				case "10":
					// Add panel
					PanelAction_AddPanel();
					bindPagesToComboBox();
					break;

				case "20":
					// Switch panel type
					PanelAction_SwitchType(selIndex);
					break;

				case "30":
					// Delete panel
					PanelAction_DeletePanel(selIndex);
					bindPagesToComboBox();
					break;

				case "40":
					// Toggle panel visibility
					PanelAction_ToggleVisibility(selIndex);
					break;
			}

		}

		private void stripMenuAddPanelColors()
		{
			Array values = Enum.GetValues(typeof(PanelDesignerColor));
			msiPanelColor.DropDownItems.Clear();

			msiPanelColor.DropDownItems.Add("Random Color", null, (sender, e) =>
			{
				var selIndex = listViewHasSelection() ? listView1.SelectedIndices[0] : -1;

				var panelColor = (PanelDesignerColor)values.GetValue(rnd.Next(values.Length));

				PanelAction_SetColor(selIndex, panelColor);
			});

			msiPanelColor.DropDownItems.Add(new ToolStripSeparator());

			foreach (var item in values)
			{
				var pdc = ((PanelDesignerColor)item).ToString();

				msiPanelColor.DropDownItems.Add(pdc, null, (sender, e) =>
				{
					ToolStripItem tsi = (ToolStripItem)sender;

					var selIndex = listViewHasSelection() ? listView1.SelectedIndices[0] : -1;
					if (selIndex == -1) return;

					PanelAction_SetColor(selIndex, (PanelDesignerColor)item);
				});
			}
		}

		private void PanelAction_AddPanel()
		{
			var point = pnlImgHolder.AutoScrollPosition;

			var x = -point.X;
			var y = -point.Y;

			if (x < 0) x = 0;
			if (y < 0) y = 0;
			if (x + 200 > pbWorkingImage.Width) x = pbWorkingImage.Width - 200;
			if (y + 200 > pbWorkingImage.Height) x = pbWorkingImage.Height - 200;

			CurrentPagePanels.Add(new ComicPanel(CurrentPagePanels.Count + 1, new RectangleF(x + 25, y + 25, 200, 200), rnd));

			bindPanelsToListView();
			listView1.Items[listView1.Items.Count - 1].Selected = true;

			pbWorkingImage.Invalidate();
		}

		private void PanelAction_SwitchType(int index)
		{
			if (index == -1)
				return;

			var panel = CurrentPagePanels[index];
			var isRet = panel.IsRectangle;
			isRet = !isRet;
			panel.IsRectangle = isRet;

			bindPanelsToListView();
			pbWorkingImage.Invalidate();
		}

		private void PanelAction_SetColor(int index, PanelDesignerColor color)
		{
			if (index == -1)
				return;

			var panel = CurrentPagePanels[index];
			panel.DesignColor = color;

			bindPanelsToListView();
			pbWorkingImage.Invalidate();
		}

		private void PanelAction_DeletePanel(int index)
		{
			Action<int> updateComicPanelsNumbers = (deletedPanelNum) =>
			{
				for (var i = 0; i < CurrentPagePanels.Count; i++)
				{
					var panel = CurrentPagePanels[i];
					if (panel.PanelNum > deletedPanelNum)
						panel.PanelNum--;
				}
			};

			if (index == -1)
				return;

			var panelNum = getSelectedPanelNum();

			CurrentPagePanels.Remove(CurrentPagePanels.Single(pnl => pnl.PanelNum == panelNum));

			updateComicPanelsNumbers(panelNum);

			bindPanelsToListView();
			pbWorkingImage.Invalidate();
		}

		private void PanelAction_ToggleVisibility(int index)
		{
			if (index == -1)
				return;

			ComicPanel panel = CurrentPagePanels[index];
			var isVisible = panel.IsDesignVisible;
			isVisible = !isVisible;
			panel.IsDesignVisible = isVisible;

			bindPanelsToListView();
			pbWorkingImage.Invalidate();
		}


		private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var index = listViewHasSelection() ? listView1.SelectedIndices[0] : -1;
			if (index == -1)
				return;

			if (index == 0)
				return;

			CurrentPagePanels[index].PanelNum--;
			CurrentPagePanels[index - 1].PanelNum++;

			#region Move
			var newIndex = index - 1;
			var item = CurrentPagePanels[index];

			CurrentPagePanels.RemoveAt(index);

			CurrentPagePanels.Insert(newIndex, item);
			#endregion

			listViewClearSelection();
			listView1.Items[index - 1].Selected = true;

			bindPanelsToListView();
		}

		private void readingOrderDownToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var index = listViewHasSelection() ? listView1.SelectedIndices[0] : -1;
			if (index == -1)
				return;

			if (index == CurrentPagePanels.Count - 1)
				return;


			CurrentPagePanels[index].PanelNum++;
			CurrentPagePanels[index + 1].PanelNum--;

			#region Move
			var newIndex = index + 1;
			var item = CurrentPagePanels[index];

			CurrentPagePanels.RemoveAt(index);

			CurrentPagePanels.Insert(newIndex, item);
			#endregion


			listViewClearSelection();
			listView1.Items[index + 1].Selected = true;

			bindPanelsToListView();
		}

		#endregion

		#region Main Menu Actions

		private int dpg_lastResultCols = 1;
		private int dpg_lastResultRows = 3;
		private double dpg_lastResultMargin = 3.0;
		private bool dpg_lastResultManga = false;
		private string comicFile_RootFolder = string.Empty;
		private string comic_Filename = string.Empty;

		private void enableTools(bool enable)
		{
			msiMainMenuTools.Enabled = enable;
			msiAddPanel.Enabled = enable;
			msiSaveProjectAs.Enabled = enable;
			msiExportComicPanels.Enabled = enable;
			msiSaveProject.Enabled = enable;
			msiZoomFitToWidth.Enabled = enable;
			msiZoomFitToHeight.Enabled = enable;
		}

		private void msiOpenComic_Click(object sender, EventArgs e)
		{
			var result = openComicDialog.ShowDialog();

			if (result != System.Windows.Forms.DialogResult.OK)
				return;

			string filename = openComicDialog.FileName;

			FileFormatReader.ComicBookFormat fileFormat = FileFormatReader.GetComicBookFormat(filename);
			if (fileFormat == FileFormatReader.ComicBookFormat.Unknown)
			{
				MessageBox.Show("Sorry, this file format is unsupported.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (pbWorkingImage.Image != null)
				pbWorkingImage.Image.Dispose();

			if (pbPreviewPanels.Image != null)
				pbPreviewPanels.Image.Dispose();

			pbWorkingImage.Image = null;
			pbPreviewPanels.Image = null;

			FileFormatReader rdr = new FileFormatReader(openComicDialog.FileName, fileFormat);

			Cursor = Cursors.WaitCursor;

			rdr.ExtractToFolder("Temp");

			string[] files = rdr.ExtractedFiles;

			comicFile_RootFolder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(files[0]));
			if (comicFile_RootFolder.ToLower() == "temp")
				comicFile_RootFolder = "";

			ComicPages.Clear();
			cbPages.Items.Clear();

			int index = 1;
			foreach (string file in files)
			{
				ComicPage cpage = new ComicPage(file, "Page " + index.ToString());
				ComicPages.Add(cpage);

				cbPages.Items.Add(cpage.ToString());

				index++;
			}

			cbPages.SelectedIndex = 0;

			Cursor = Cursors.Default;

			statusLblComicName.Text = System.IO.Path.GetFileName(filename);
			statusLblProjectName.Text = "";
			WorkingProject_FileName = "";
			workingProject_ComicFile = openComicDialog.FileName;

			comic_Filename = System.IO.Path.GetFileName(filename);

			enableTools(true);
		}

		private void msiDrawPanelGrid_Click(object sender, EventArgs e)
		{
			bool dialogOk = true;

			if (CurrentPagePanels.Count > 0)
			{
				dialogOk = MessageBox.Show("This action will delete all panels on current page. Proceed?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK;
			}

			if (dialogOk)
			{
				using (DrawPanelGrid dpg = new DrawPanelGrid())
				{
					dpg.SetStartValues(dpg_lastResultCols, dpg_lastResultRows, dpg_lastResultMargin, dpg_lastResultManga);

					var result = dpg.ShowDialog();

					if (result == System.Windows.Forms.DialogResult.OK)
					{
						dpg_lastResultCols = dpg.ResultColumns;
						dpg_lastResultRows = dpg.ResultRows;
						dpg_lastResultMargin = dpg.ResultMargin;
						dpg_lastResultManga = dpg.ResultManga;

						createComicPanelGrid(dpg_lastResultCols, dpg_lastResultRows, dpg_lastResultMargin, dpg_lastResultManga);

						bindPanelsToListView();
						bindPagesToComboBox();

						pbWorkingImage.Invalidate();
					}
				}
			}
		}

		private void msiClearPanels_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Remove all panels?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.OK)
			{
				CurrentPagePanels.Clear();
				bindPanelsToListView();
				bindPagesToComboBox();

				pbWorkingImage.Invalidate();
			}
		}

		private void msiViewAntialias_Click(object sender, EventArgs e)
		{
			pbWorkingImage.Invalidate();
		}

		private void msiZoom_Click(object sender, EventArgs e)
		{
			var tsi = (ToolStripMenuItem)sender;
			string text = tsi.Tag.ToString();

			int minZoom = trackBar1.Minimum;
			int maxZoom = trackBar1.Maximum;
			int curZoom = maxZoom;


			if (text == "FITW")
			{
				int imgWidth = pbWorkingImage.Image.Width;

				int width = Convert.ToInt32(imgWidth * (float)curZoom / 100);

				while (width > pnlImgHolder.Width - 50 && curZoom > minZoom)
				{
					width = Convert.ToInt32(imgWidth * (float)curZoom / 100);
					curZoom -= 1;
				}

				trackBar1.Value = curZoom;
			}
			else if (text == "FITH")
			{
				int imgHeight = pbWorkingImage.Image.Height;
				int height = Convert.ToInt32(imgHeight * (float)curZoom / 100);

				while (height > pnlImgHolder.Height - 50 && curZoom > minZoom)
				{
					height = Convert.ToInt32(imgHeight * (float)curZoom / 100);
					curZoom -= 1;
				}

				trackBar1.Value = curZoom;
			}
			else
			{
				int zoom = Convert.ToInt32(text);
				trackBar1.Value = zoom;
			}

			trackBar1_Scroll(null, null);
		}

		#region Grid MenuItem Actions

		private void msiUseGrid_Click(object sender, EventArgs e)
		{

			useGrid = msiUseGrid.Checked;

		}

		private void msiGridClick(object sender, EventArgs e)
		{
			gridSize = Convert.ToInt32(((ToolStripMenuItem)sender).Text);

			setMenuItemGridChecked(gridSize);
		}

		private void setMenuItemGridChecked(int gridSize)
		{
			msiGrid4.Checked = false;
			msiGrid8.Checked = false;
			msiGrid10.Checked = false;
			msiGrid12.Checked = false;
			msiGrid14.Checked = false;
			msiGrid16.Checked = false;
			msiGrid18.Checked = false;
			msiGrid20.Checked = false;

			switch (gridSize)
			{
				case 4:
					msiGrid4.Checked = true;
					break;
				case 8:
					msiGrid8.Checked = true;
					break;
				case 10:
					msiGrid10.Checked = true;
					break;
				case 12:
					msiGrid12.Checked = true;
					break;
				case 14:
					msiGrid14.Checked = true;
					break;
				case 16:
					msiGrid16.Checked = true;
					break;
				case 18:
					msiGrid18.Checked = true;
					break;
				case 20:
					msiGrid20.Checked = true;
					break;
			}
		}

		#endregion

		#endregion

		#region ComboBox "Pages" Actions

		private bool cbInvokeChangeHandler = true;
		private void bindPagesToComboBox()
		{
			cbInvokeChangeHandler = false;

			int index = cbPages.SelectedIndex;

			cbPages.Items.Clear();

			foreach (var page in ComicPages)
				cbPages.Items.Add(page.ToString());

			cbPages.SelectedIndex = index;

			cbInvokeChangeHandler = true;
		}

		// ComboBox Pages Item Changed - Show Page and Get Panels for selected Page
		private void cbPages_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!cbInvokeChangeHandler)
				return;

			tabControl1.SelectedIndex = 0;

			ComicPage cpage = ComicPages[cbPages.SelectedIndex];

			CurrentPagePanels = cpage.Panels;
			bindPanelsToListView();

			if (listView1.Items.Count > 0)
				listView1.Items[0].Selected = true;

			string curPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
			string pathToImage = System.IO.Path.Combine(curPath, "Temp", comicFile_RootFolder, cpage.ImagePath);

			if (pbWorkingImage.Image != null)
				pbWorkingImage.Image.Dispose();
			pbWorkingImage.Image = Image.FromFile(pathToImage);


			int imgWidth = pbWorkingImage.Image.Width;
			int imgHeight = pbWorkingImage.Image.Height;
			int drawAreaWidth = pbWorkingImage.Width;
			int drawAreaHeight = pbWorkingImage.Height;

			Utils.SetData(DrawingRectDragMargin, CurrentDrawingRectangle, imgWidth, imgHeight, drawAreaWidth, drawAreaHeight);

			//trackBar1.Value = (int)(cpage.ZoomLevel * 100);
			trackBar1.Value = (int)(0.3 * 100);
			trackBar1_Scroll(null, null);
		}

		#endregion

		#region Preview Panel Controls & Actions

		private DeviceSizes panelPreviewSize = DeviceSizes.AutoSize;
		private bool panelPreviewLandscape = true;

		// TabControl Switch from Panel Mode to Preview Mode, or Vice Versa 
		private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Check if we are in preview mode
			bool previewMode = tabControl1.SelectedIndex == 1;

			if (!previewMode || pbWorkingImage.Image == null || CurrentPagePanels == null || CurrentPagePanels.Count == 0)
				return;

			// Just in case it's not properly set in designer ...
			pbPreviewPanels.SizeMode = PictureBoxSizeMode.Zoom;

			if (!listViewHasSelection())
				listView1.Items[0].Selected = true;

			ImagePreviewGetPanel();
		}

		private Point getDevicePreviewSize()
		{
			switch (panelPreviewSize)
			{
				case DeviceSizes.AutoSize:
					return new Point(0, 0);
				default:
					string preview = panelPreviewSize.ToString().Substring(panelPreviewSize.ToString().IndexOf("_") + 1);
					string[] sizes = preview.Split('x');
					int width = Convert.ToInt32(sizes[1]);
					int height = Convert.ToInt32(sizes[0]);

					if (panelPreviewLandscape)
						return new Point(width, height);
					else
						return new Point(height, width);
			}
		}

		private void ImagePreviewGetPanel()
		{
			if (!listViewHasSelection())
				return;


			int index = listView1.SelectedIndices[0];

			// Get image part
			Bitmap newBmp;
			using (Bitmap src = new Bitmap(pbWorkingImage.Image))
			{
				RectangleF r = Utils.GetDisplayRectangleFromRelative(CurrentPagePanels[index].RelativePosition, 1);
				r = Utils.NormalizeRect(r, new RectangleF(0, 0, src.Width, src.Height));

				newBmp = src.Clone(r, System.Drawing.Imaging.PixelFormat.DontCare);
			}


			if (pbPreviewPanels.Image != null)
				pbPreviewPanels.Image.Dispose();
			pbPreviewPanels.Image = newBmp;
			// --

			// Scale new image
			var zoom = 1.0;
			int borderMargin = 15; // Margin Left, Top, Right, Bottom, so it's not so close to Navigation Buttons

			if (panelPreviewSize == DeviceSizes.AutoSize)
			{
				int width = Convert.ToInt32(newBmp.Width * zoom);
				int height = Convert.ToInt32(newBmp.Height * zoom);
				while ((width > (pnlPreviewImageHolder.Width - borderMargin * 2) || height > (pnlPreviewImageHolder.Height - borderMargin * 2)) && zoom > 0.1)
				{
					zoom -= 0.03;

					width = Convert.ToInt32(newBmp.Width * zoom);
					height = Convert.ToInt32(newBmp.Height * zoom);
				}

				pbPreviewPanels.Size = new Size(width, height);
			}
			else
			{
				Point previewSize = getDevicePreviewSize();
				pbPreviewPanels.Size = new System.Drawing.Size(previewSize.X, previewSize.Y);
			}


			int imgLeft = pnlPreviewImageHolder.Width / 2 - pbPreviewPanels.Width / 2;
			int imgTop = pnlPreviewImageHolder.Height / 2 - pbPreviewPanels.Height / 2;

			if (imgLeft < 0) imgLeft = 0;
			if (imgTop < 0) imgTop = 0;

			pbPreviewPanels.Left = imgLeft;
			pbPreviewPanels.Top = imgTop;
		}

		private void pnlPreviewImageHolder_Resize(object sender, EventArgs e)
		{
			if (pbPreviewPanels.Image == null)
				return;

			// Scale new image
			var zoom = 1.0;
			int borderMargin = 15; // Margin left, top, right, bottom, so it's not so close to navigation buttons

			int originalWidth = pbPreviewPanels.Image.Width;
			int originalHeight = pbPreviewPanels.Image.Height;

			if (panelPreviewSize == DeviceSizes.AutoSize)
			{
				int width = Convert.ToInt32(originalWidth * zoom);
				int height = Convert.ToInt32(originalHeight * zoom);
				while ((width > (pnlPreviewImageHolder.Width - borderMargin * 2) || height > (pnlPreviewImageHolder.Height - borderMargin * 2)) && zoom > 0.1)
				{
					zoom -= 0.03;

					width = Convert.ToInt32(originalWidth * zoom);
					height = Convert.ToInt32(originalHeight * zoom);
				}
				pbPreviewPanels.Size = new Size(width, height);
			}
			else
			{
				Point p = getDevicePreviewSize();
				pbPreviewPanels.Size = new System.Drawing.Size(p.X, p.Y);
			}


			int imgLeft = pnlPreviewImageHolder.Width / 2 - pbPreviewPanels.Width / 2;
			int imgTop = pnlPreviewImageHolder.Height / 2 - pbPreviewPanels.Height / 2;

			if (imgLeft < 0) imgLeft = 0;
			if (imgTop < 0) imgTop = 0;

			pbPreviewPanels.Left = imgLeft;
			pbPreviewPanels.Top = imgTop;
		}

		private void btnPreviewGoLeft_Click(object sender, EventArgs e)
		{
			int index = listViewHasSelection() ? listView1.SelectedIndices[0] : -1;

			index -= 1;
			if (index < 0) index = 0;

			listViewClearSelection();
			listView1.Items[index].Selected = true;

			ImagePreviewGetPanel();
		}

		private void btnPreviewGoRight_Click(object sender, EventArgs e)
		{
			int index = listViewHasSelection() ? listView1.SelectedIndices[0] : -1;

			index += 1;
			if (index > listView1.Items.Count - 1) index = listView1.Items.Count - 1;


			listViewClearSelection();
			listView1.Items[index].Selected = true;

			ImagePreviewGetPanel();
		}

		private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
		{
			e.Cancel = !e.TabPage.Enabled;
		}

		#endregion

		// Trackbar Scroll - Change Zoom level
		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			ImageZoomLevel = (double)trackBar1.Value / 100;

			if (ImageZoomLevel < 0.1) ImageZoomLevel = 0.1;

			if (CurrentPagePanels == null)
				return;

			ComicPages[cbPages.SelectedIndex].ZoomLevel = ImageZoomLevel;

			RectangleF rf = Utils.GetRelativeRect(CurrentDrawingRectangle);
			CurrentDrawingRectangle = Utils.GetDisplayRectangleFromRelative(rf, ImageZoomLevel);


			reloadPicture();
		}

		private void pnlImgHolder_Scroll(object sender, ScrollEventArgs e)
		{
			ComicPages[cbPages.SelectedIndex].CurrentScrollPos = pnlImgHolder.AutoScrollPosition;
		}

		#region Export Comic Panels MenuItem

		private void msiExportComicPanels_Click(object sender, EventArgs e)
		{
			var result = saveComicPanelDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
			if (!result)
				return;


			string filename = workingProject_ComicFile;


			Serializing.CPDFileFormat cpdFile = new Serializing.CPDFileFormat(filename, cpdInfo_Author);
			cpdFile.Comments = cpdInfo_Comments;


			int index = 0;
			foreach (ComicPage page in ComicPages)
			{
				string imageName = System.IO.Path.GetFileName(page.ImagePath);

				var serializedPage = new Serializing.SerializedComicPage(index, imageName);

				foreach (ComicPanel panel in page.Panels)
				{
					serializedPage.Panels.Add(new Serializing.SerializedComicPanel(panel.IsRectangle, panel.RelativePosition, index, panel.PanelNum));
				}

				cpdFile.PanelDefinitions.Add(serializedPage);

				index++;
			}

			string saveFilename = saveComicPanelDialog.FileName;
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(cpdFile);

			System.IO.File.WriteAllText(saveFilename, json, Encoding.UTF8);
		}

		#endregion

		#region CPR/CPD Info MenuItem

		private string cpdInfo_Author = string.Empty;
		private string cpdInfo_Comments = string.Empty;
		private void msiCprCpdInfo_Click(object sender, EventArgs e)
		{
			CPDInfo cpdInfo = new CPDInfo();

			cpdInfo.Author = cpdInfo_Author;
			cpdInfo.Comments = cpdInfo_Comments;

			bool result = cpdInfo.ShowDialog() == System.Windows.Forms.DialogResult.OK;

			if (result)
			{
				cpdInfo_Author = cpdInfo.Author;
				cpdInfo_Comments = cpdInfo.Comments;
			}
		}

		#endregion

		#region Open Project MenuItem

		private string _workingProject_Filename = "";
		private string WorkingProject_FileName
		{
			get
			{
				return _workingProject_Filename;
			}

			set
			{
				_workingProject_Filename = value;
				if (!string.IsNullOrWhiteSpace(value))
					statusLblProjectName.Text = "[Project:" + System.IO.Path.GetFileName(value) + "]";
				else
					statusLblProjectName.Text = "";
			}
		}

		private string workingProject_ComicFile = string.Empty;

		private void msiOpenProject_Click(object sender, EventArgs e)
		{
			var result = openProjectDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
			if (!result)
				return;

			if (pbWorkingImage.Image != null)
				pbWorkingImage.Image.Dispose();

			if (pbPreviewPanels.Image != null)
				pbPreviewPanels.Image.Dispose();

			pbWorkingImage.Image = null;
			pbPreviewPanels.Image = null;

			WorkingProject_FileName = openProjectDialog.FileName;

			string json = System.IO.File.ReadAllText(openProjectDialog.FileName, Encoding.UTF8);

			Serializing.CPRFileFormat cpr = (Serializing.CPRFileFormat)Newtonsoft.Json.JsonConvert.DeserializeObject<Serializing.CPRFileFormat>(json);

			string comicFilePath = System.IO.Path.GetDirectoryName(openProjectDialog.FileName);
			comicFilePath = System.IO.Path.Combine(comicFilePath, cpr.ComicFile);
			workingProject_ComicFile = comicFilePath;

			FileFormatReader.ComicBookFormat fileFormat = FileFormatReader.GetComicBookFormat(comicFilePath);
			FileFormatReader rdr = new FileFormatReader(comicFilePath, fileFormat);
			Cursor = Cursors.WaitCursor;

			rdr.ExtractToFolder("Temp");

			string[] files = rdr.ExtractedFiles;
			comicFile_RootFolder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(files[0]));
			if (comicFile_RootFolder.ToLower() == "temp")
				comicFile_RootFolder = "";

			ComicPages.Clear();
			cbPages.Items.Clear();

			foreach (var cp in ComicPages)
				cp.Panels.Clear();

			Cursor = Cursors.Default;

			statusLblComicName.Text = System.IO.Path.GetFileName(comicFilePath);
			//statusLblProjectName.Text = "[Project: " + System.IO.Path.GetFileName(workingProject_FileName) + "]";

			comic_Filename = System.IO.Path.GetFileName(comicFilePath);

			reloadPicture();

			ComicPages = cpr.Pages;
			bindPagesToComboBox();
			if (cbPages.Items.Count > cpr.LastWorkingPage)
				cbPages.SelectedIndex = cpr.LastWorkingPage;
			else
				cbPages.SelectedIndex = 0;


			foreach (ListViewItem li in listView1.Items)
				li.Selected = false;

			enableTools(true);

			#region Read Grid settings

			useGrid = cpr.UseGrid;
			gridSize = cpr.GridSize;
			msiUseGrid.Checked = useGrid;
			setMenuItemGridChecked(gridSize);

			#endregion

			#region Read Project Info

			cpdInfo_Author = cpr.Author;
			cpdInfo_Comments = cpr.Comments;

			#endregion

		}

		#endregion

		#region Save Project As MenuItem

		private void saveProject(string filename)
		{
			if (filename.Length == 0)
				return;

			Serializing.CPRFileFormat cpr = new Serializing.CPRFileFormat();
			cpr.Author = cpdInfo_Author;
			cpr.Comments = cpdInfo_Comments;
			cpr.Pages = ComicPages;
			cpr.RootFolder = comicFile_RootFolder;
			cpr.ComicFile = comic_Filename;
			cpr.LastWorkingPage = cbPages.SelectedIndex;
			cpr.UseGrid = useGrid;
			cpr.GridSize = gridSize;

			string json = Newtonsoft.Json.JsonConvert.SerializeObject(cpr);

			System.IO.File.WriteAllText(filename, json, Encoding.UTF8);

			string onlyName = System.IO.Path.GetFileName(filename);
			MessageBox.Show("Project [" + onlyName + "] Saved");
		}

		private void msiSaveProjectAs_Click(object sender, EventArgs e)
		{
			var result = saveProjectDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;

			if (!result)
				return;

			string saveFilename = saveProjectDialog.FileName;

			WorkingProject_FileName = saveFilename;


			saveProject(saveFilename);
		}

		#endregion

		#region Save Project MenuItem

		private void msiSaveProject_Click(object sender, EventArgs e)
		{
			if (WorkingProject_FileName.Length == 0 || !System.IO.File.Exists(WorkingProject_FileName))
				return;

			saveProject(WorkingProject_FileName);
		}

		#endregion

		private void fileToolStripMenuItem_MouseEnter(object sender, EventArgs e)
		{
			msiSaveProject.Enabled = WorkingProject_FileName.Length > 0;
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var about = new About())
			{
				about.ShowDialog();
			}


		}

		private void zorbosoftHomepageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.zorbosoft.com");
		}


		private void msiPreviewSizeClick(object sender, EventArgs e)
		{
			string preview = ((ToolStripMenuItem)sender).Name.Trim().Replace("msiPreview", "").Trim();

			#region Uncheck all items

			msiPreviewAuto.Checked = false;
			msiPreviewR1.Checked = false;
			msiPreviewR2.Checked = false;
			msiPreviewR3.Checked = false;
			msiPreviewR4.Checked = false;
			msiPreviewR5.Checked = false;
			msiPreviewR6.Checked = false;
			msiPreviewR7.Checked = false;
			msiPreviewR8.Checked = false;
			msiPreviewR9.Checked = false;
			msiPreviewR10.Checked = false;
			msiPreviewR11.Checked = false;
			msiPreviewR12.Checked = false;

			#endregion

			((ToolStripMenuItem)sender).Checked = true;


			DeviceSizes ds = DeviceSizes.AutoSize;

			switch (preview)
			{
				case "Auto":
				default:
					break;
				case "R1":
					ds = DeviceSizes.iPhone1_320x480;
					break;
				case "R2":
					ds = DeviceSizes.iPhone4_640x960;
					break;
				case "R3":
					ds = DeviceSizes.iPhone5_640x1136;
					break;
				case "R4":
					ds = DeviceSizes.iPhone6_750x1134;
					break;
				case "R5":
					ds = DeviceSizes.iPad_768x1024;
					break;

				case "R6":
					ds = DeviceSizes.Android_320x480;
					break;
				case "R7":
					ds = DeviceSizes.Android_480x800;
					break;
				case "R8":
					ds = DeviceSizes.Android_480x854;
					break;
				case "R9":
					ds = DeviceSizes.Android_540x960;
					break;
				case "R10":
					ds = DeviceSizes.Android_600x1024;
					break;
				case "R11":
					ds = DeviceSizes.Android_720x1280;
					break;
				case "R12":
					ds = DeviceSizes.Android_800x1280;
					break;
			}

			panelPreviewSize = ds;
			ImagePreviewGetPanel();

		}

		private void msiPreviewLandscape_Click(object sender, EventArgs e)
		{
			panelPreviewLandscape = msiPreviewLandscape.Checked;
			ImagePreviewGetPanel();
		}

		private void cbPages_DrawItem(object sender, DrawItemEventArgs e)
		{
			string s = (e.Index < 0) ? "" : cbPages.Items[e.Index].ToString();

			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Alignment = StringAlignment.Near;

			e.Graphics.FillRectangle(new SolidBrush(Color.DarkGray), e.Bounds);
			if (cbPages.SelectedIndex > -1)
			{
				if (s.Contains("(x"))
				{
					string part1 = s.Substring(0, s.LastIndexOf(" "));
					string part2 = s.Substring(s.LastIndexOf(" ") + 1);

					e.Graphics.DrawString(part1, this.Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), format);
					e.Graphics.DrawString(part2, this.Font, new SolidBrush(Color.FromArgb(255, 70, 70, 70)), new RectangleF(e.Bounds.X + 50, e.Bounds.Y, e.Bounds.Width - 50, e.Bounds.Height), format);
				}
				else
				{
					e.Graphics.DrawString(s, this.Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), format);
				}
			}

			if (e.Index < 0)
				return;

			if ((e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit)
				return;

			//e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			// Draw the background of the item.
			if (
				((e.State & DrawItemState.Focus) == DrawItemState.Focus) ||
				((e.State & DrawItemState.Selected) == DrawItemState.Selected) ||
				((e.State & DrawItemState.HotLight) == DrawItemState.HotLight)
			   )
			{
				//e.DrawBackground();

				Brush backgroundBrush = new SolidBrush(Color.DarkGray);
				e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
			}
			else
			{
				Brush backgroundBrush = new SolidBrush(Color.Silver);
				e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
			}

			//Draw item text


			if (s.Contains("(x"))
			{
				string part1 = s.Substring(0, s.LastIndexOf(" "));
				string part2 = s.Substring(s.LastIndexOf(" ") + 1);

				e.Graphics.DrawString(part1, this.Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), format);
				e.Graphics.DrawString(part2, this.Font, new SolidBrush(Color.FromArgb(255, 70, 70, 70)), new RectangleF(e.Bounds.X + 50, e.Bounds.Y, e.Bounds.Width - 50, e.Bounds.Height), format);
			}
			else
			{
				e.Graphics.DrawString(s, this.Font, Brushes.Black, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), format);
			}

		}

		private void msiExitApp_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Exit Application?", "Exit Comic Smart Panels Creator", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
			{
				
				Application.Exit();

			}
		}

		private void projectOnGitHubToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/zoran123456/Comic-Smart-Panels");
		}


	}

}
