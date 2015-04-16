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
	public partial class CPDInfo : Form
	{

		public string Author { get; set; }
		public string Comments { get; set; }

		public CPDInfo()
		{
			InitializeComponent();
		}

		private void CPDInfo_Load(object sender, EventArgs e)
		{
			tbAuthor.Text = Author;
			tbComments.Text = Comments;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{

			Author = tbAuthor.Text.Trim();
			Comments = tbComments.Text.Trim();

		}


	}
}
