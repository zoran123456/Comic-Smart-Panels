using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComicSmartPanels
{
	public partial class DrawPanelGrid : Form
	{
		public int ResultColumns { get; set; }
		public int ResultRows { get; set; }
		public double ResultMargin { get; set; }
		public bool ResultManga { get; set; }

		public DrawPanelGrid()
		{
			InitializeComponent();
		}

		public void SetStartValues(int cols, int rows, double margin, bool manga)
		{
			tbColumns.Text = cols.ToString();
			tbRows.Text = rows.ToString();
			tbMargin.Text = margin.ToString("0.0");
			cbManga.Checked = manga;
		}

		private void DrawPanelGrid_Load(object sender, EventArgs e)
		{
		}

		private void ShowError(string message)
		{
			MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			string cols = tbColumns.Text.Trim();
			string rows = tbRows.Text.Trim();
			string margin = tbMargin.Text.Trim();

			int iCols = 0;
			int iRows = 0;
			double dMargin = 0.0;

			int.TryParse(cols, out iCols);
			int.TryParse(rows, out iRows);
			double.TryParse(margin, out dMargin);

			if (iCols < 1 || iCols > 10)
			{
				ShowError("Number of Columns must be in range 1-10");
				tbColumns.Focus();
				return;
			}
			if (iRows < 1 || iRows > 10)
			{
				ShowError("Number of Rows must be in range 1-10");
				tbRows.Focus();
				return;
			}

			if (dMargin < 1 || dMargin > 15)
			{
				ShowError("Margin must be in range 1-15");
				tbMargin.Focus();
				return;
			}

			ResultColumns = iCols;
			ResultRows = iRows;
			ResultMargin = dMargin;
			ResultManga = cbManga.Checked;

			DialogResult = System.Windows.Forms.DialogResult.OK;
		}


		
	}
}
